using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CW2.DAL;
using CW2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CW2.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {

        private readonly IDbService _dbService;


        public EnrollmentsController(IDbService dbService)
        {
            _dbService = dbService;
        }


        [HttpPost]
        public IActionResult CreateStudent(CreateStudentDTO student)
        {
            var studies = _dbService.GetStudiesByName(student.Studies);


            if (studies == null)
            {
                return BadRequest($"Nie znaleziono kierunku studiow {student.Studies}");
            }
            if (_dbService.GetStudentByIndexNumber(student.IndexNumber) != null)
            {
                return BadRequest($"Istnieje juz student z numerem indeksu {student.IndexNumber}");
            }
            var enrollment = _dbService.GetLatestEnrollment(1, studies.IdStudy);
            if (enrollment == null)
            {
                enrollment =  _dbService.AddStudentWithNewEnrollment(student, new Enrollment(){IdStudy = studies.IdStudy, Semester = 1, StartDate = DateTime.Now});
            }
            else
            {
                _dbService.AddStudentWithExistingEnrollment(student, enrollment.IdEnrollment);
            }           
            return Created("", enrollment);
        }


        [HttpPost("promotions")]
        public IActionResult PromoteStudents(PromoteStudentsDTO promoteStudents)
        {
            var studies = _dbService.GetStudiesByName(promoteStudents.Studies);
            if (studies == null)
            {
                return NotFound($"Nie znaleziono kierunku studiow {promoteStudents.Studies}");
            }
            if (_dbService.GetLatestEnrollment(promoteStudents.Semester, studies.IdStudy) == null)
            {
                return NotFound($"Nie znaleziono semestru {promoteStudents.Semester}");
            }
            var enrollment = _dbService.PromoteStudents(promoteStudents);            
            return Created("", enrollment);
        }
    }
}