DROP PROCEDURE IF EXISTS spPromoteStudents
GO

CREATE PROCEDURE spPromoteStudents
	@studies NVARCHAR(100), @semester INT
AS
BEGIN
	-- sprawdzenie czy jest enrollment dla nastêpnego semestru
	DECLARE @idEnrollmentNextSemester INT = (SELECT TOP 1 IdEnrollment 
		FROM Enrollment 
		WHERE IdStudy = (SELECT TOP 1 IdStudy FROM Studies WHERE Name = @studies)
			AND Semester = @semester + 1
		ORDER BY StartDate DESC)
		

	IF (@idEnrollmentNextSemester IS NULL)
	BEGIN
		-- zrobienie nowego enrollmentu z semestrem + 1
		SET @idEnrollmentNextSemester = (SELECT MAX(IdEnrollment) + 1 FROM Enrollment)

		INSERT INTO [Enrollment]
                ([IdEnrollment]
                ,[Semester]
                ,[IdStudy]
                ,[StartDate])
                VALUES
                (@idEnrollmentNextSemester
                ,@semester + 1
                ,(SELECT TOP 1 IdStudy FROM Studies WHERE Name = @studies)
                ,GETDATE())
	END

	-- promujemy studentow
	UPDATE Student
	SET IdEnrollment = @idEnrollmentNextSemester
	WHERE IdEnrollment IN (SELECT e.IdEnrollment FROM Enrollment e JOIN Studies s ON e.IdStudy = s.IdStudy WHERE s.Name = @studies AND e.Semester = @semester)

	-- zwrocony enrollment
	SELECT [IdEnrollment]
                ,[Semester]
                ,[IdStudy]
                ,[StartDate]
	FROM Enrollment 
	WHERE IdEnrollment = @idEnrollmentNextSemester
END
GO
