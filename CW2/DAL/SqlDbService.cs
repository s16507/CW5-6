using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CW2.Models;

namespace CW2.DAL
{
    public class SqlDbService : IDbService
    {
        public IEnumerable<Student> GetStudents()
        {
            using (var client = new SqlConnection(
                "Data Source=db-mssql;Initial Catalog=s16507;Integrated Security=True"))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = client;
                cmd.CommandText = @"SELECT FirstName, LastName, BirthDate, e.Semester, s.Name
                FROM Student AS student
                JOIN Enrollment AS e ON student.IdEnrollment = e.IdEnrollment
                JOIN Studies AS s ON e.IdStudy = s.IdStudy";


                client.Open();
                var reader = cmd.ExecuteReader();
                var students = new List<Student>();


                while (reader.Read())
                {
                    var student = new Student();
                    student.FirstName = reader["FirstName"].ToString();
                    student.LastName = reader["LastName"].ToString();
                    student.BirthDate = Convert.ToDateTime(reader["BirthDate"]);
                    student.StudyName = reader["Name"].ToString();
                    student.SemesterNumber = Convert.ToInt32(reader["Semester"]);
                    students.Add(student);
                }
                return students;
            }
        }


        public Student GetStudentByIndexNumber(string indexNumber)
        {
            using (var client = new SqlConnection(
                "Data Source=db-mssql;Initial Catalog=s16507;Integrated Security=True"))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = client;
                cmd.CommandText = @"SELECT IndexNumber, FirstName, LastName, BirthDate, e.Semester, s.Name
                  FROM Student AS student
                  JOIN Enrollment AS e ON student.IdEnrollment = e.IdEnrollment
                  JOIN Studies AS s ON e.IdStudy = s.IdStudy
                  WHERE IndexNumber = @indexNumber";
                cmd.Parameters.AddWithValue("indexNumber", indexNumber);


                client.Open();
                var reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    var student = new Student();
                    student.IndexNumber = reader["IndexNumber"].ToString();
                    student.FirstName = reader["FirstName"].ToString();
                    student.LastName = reader["LastName"].ToString();
                    student.BirthDate = Convert.ToDateTime(reader["BirthDate"]);
                    student.StudyName = reader["Name"].ToString();
                    student.SemesterNumber = Convert.ToInt32(reader["Semester"]);
                    return student;
                }

                return null;
            }
        }


        public Studies GetStudiesByName(string name)
        {
            using (var client = new SqlConnection(
                "Data Source=db-mssql;Initial Catalog=s16507;Integrated Security=True"))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = client;
                cmd.CommandText = @"SELECT IdStudy, Name
                  FROM Studies AS studies
                  WHERE Name = @name";
                cmd.Parameters.AddWithValue("name", name);


                client.Open();
                var reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    var studies = new Studies();
                    studies.IdStudy = Convert.ToInt32(reader["IdStudy"]);
                    studies.Name = reader["Name"].ToString();
                    return studies;
                }

                return null;
            }
        }


        public Enrollment GetLatestEnrollment(int semester, int idStudy)
        {
            using (var client = new SqlConnection(
                "Data Source=db-mssql;Initial Catalog=s16507;Integrated Security=True"))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = client;
                cmd.CommandText = @"SELECT TOP 1 IdEnrollment, Semester, IdStudy, StartDate
                FROM ENROLLMENT
                WHERE IdStudy= @idStudy AND Semester = @semester
                ORDER BY StartDate DESC";
                cmd.Parameters.AddWithValue("idStudy", idStudy);
                cmd.Parameters.AddWithValue("semester", semester);


                client.Open();
                var reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    var enrollment = new Enrollment();
                    enrollment.IdEnrollment = Convert.ToInt32(reader["IdEnrollment"]);
                    enrollment.Semester = Convert.ToInt32(reader["Semester"]);
                    enrollment.IdStudy = Convert.ToInt32(reader["IdStudy"]);
                    enrollment.StartDate = Convert.ToDateTime(reader["StartDate"]);
                    return enrollment;
                }

                return null;
            }
        }


        public void AddStudentWithExistingEnrollment(CreateStudentDTO student, int existingIdEnrollment)
        {
            using (var client = new SqlConnection(
                "Data Source=db-mssql;Initial Catalog=s16507;Integrated Security=True"))
            {
                client.Open();
                using (var transaction = client.BeginTransaction())
                {
                    try
                    {
                        AddStudent(client, transaction, student, existingIdEnrollment);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public Enrollment AddStudentWithNewEnrollment(CreateStudentDTO student, Enrollment enrollment)
        {
            using (var client = new SqlConnection(
                "Data Source=db-mssql;Initial Catalog=s16507;Integrated Security=True"))
            {
                client.Open();
                using (var transaction = client.BeginTransaction())
                {
                    try
                    { 
                        enrollment = AddEnrollment(client, transaction, enrollment);
                        AddStudent(client, transaction, student, enrollment.IdEnrollment);
                        transaction.Commit();
                        return enrollment;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public Enrollment PromoteStudents(PromoteStudentsDTO promoteStudentsDto)
        {
            using (var client = new SqlConnection(
                "Data Source=db-mssql;Initial Catalog=s16507;Integrated Security=True"))
            using (var cmd = new SqlCommand("spPromoteStudents"))
            {
                cmd.Connection = client;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("studies", promoteStudentsDto.Studies);
                cmd.Parameters.AddWithValue("semester", promoteStudentsDto.Semester);


                client.Open();
                var reader = cmd.ExecuteReader();
               
                
                while (reader.Read())
                {
                    var enrollment = new Enrollment();
                    enrollment.IdEnrollment = Convert.ToInt32(reader["IdEnrollment"]);
                    enrollment.Semester = Convert.ToInt32(reader["Semester"]);
                    enrollment.IdStudy = Convert.ToInt32(reader["IdStudy"]);
                    enrollment.StartDate = Convert.ToDateTime(reader["StartDate"]);
                    return enrollment;
                }

                return null;
            }
        }


        private Enrollment AddEnrollment(SqlConnection client, SqlTransaction transaction, Enrollment enrollment)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = client;
                cmd.Transaction = transaction;
                cmd.CommandText = @"INSERT INTO [Enrollment]
                ([IdEnrollment]
                ,[Semester]
                ,[IdStudy]
                ,[StartDate])
                OUTPUT INSERTED.IdEnrollment VALUES
                ((SELECT MAX(IdEnrollment) + 1 FROM Enrollment)
                ,@semester
                ,@idStudy
                ,@startDate)";
                cmd.Parameters.AddWithValue("idStudy", enrollment.IdStudy);
                cmd.Parameters.AddWithValue("semester", enrollment.Semester);
                cmd.Parameters.AddWithValue("startDate", enrollment.StartDate);

                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    enrollment.IdEnrollment = Convert.ToInt32(reader["IdEnrollment"]);
                    return enrollment;
                }
            }
        }


        private void AddStudent(SqlConnection client, SqlTransaction transaction, CreateStudentDTO student, int idEnrollment)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = client;
                cmd.Transaction = transaction;
                cmd.CommandText = @"INSERT INTO [Student]
                       ([IndexNumber]
                       ,[FirstName]
                       ,[LastName]
                       ,[BirthDate]
                       ,[IdEnrollment])
                 VALUES
                       (@indexNumber
                       ,@firstName
                       ,@lastName
                       ,@birthDate
                       ,@enrollment)";
                cmd.Parameters.AddWithValue("indexNumber", student.IndexNumber);
                cmd.Parameters.AddWithValue("firstName", student.FirstName);
                cmd.Parameters.AddWithValue("lastName", student.LastName);
                cmd.Parameters.AddWithValue("birthDate", student.BirthDate);
                cmd.Parameters.AddWithValue("enrollment", idEnrollment);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
