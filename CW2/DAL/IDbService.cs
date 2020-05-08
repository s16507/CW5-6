using CW2.Models;
using System.Collections.Generic;

namespace CW2.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
        public Student GetStudentByIndexNumber(string indexNumber);
        public Studies GetStudiesByName(string name);
        public Enrollment GetLatestEnrollment(int semester, int idStudy);
        public void AddStudentWithExistingEnrollment(CreateStudentDTO student, int existingIdEnrollment);
        public Enrollment AddStudentWithNewEnrollment(CreateStudentDTO student, Enrollment enrollment);
        public Enrollment PromoteStudents(PromoteStudentsDTO promoteStudentsDto);
    }
}
