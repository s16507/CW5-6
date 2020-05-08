using System;
using CW2.DAL;
using CW2.Models;
using Microsoft.AspNetCore.Mvc;

namespace CW2.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        
        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }



        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            return Ok(_dbService.GetStudents());
        }



        [HttpPost]
        public IActionResult AddStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }



        [HttpGet("{id}")]
        public IActionResult GetStudentById([FromRoute] string id)
        {
            var student = _dbService.GetStudentByIndexNumber(id);
            if (student != null)
            {
                return Ok(student);
            }

            return NotFound($"Brak studenta o id {id}");
        }



        [HttpPut("{id}")]
        public IActionResult UpdateStudent(Student student, [FromRoute] int id)
        {
            return Ok($"Dokonczona aktualizacja o id {id}");
        }



        [HttpDelete("{id}")]
        public IActionResult DeleteStudent([FromRoute] int id)
        {
           
            return Ok($"Ukonczone usuwanie studenta o id {id}");
        }
    }
}