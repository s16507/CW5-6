using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CW2.Models
{
    public class PromoteStudentsDTO
    {
        [Required]
        public string Studies { get; set; }

        [Required]
        public int Semester { get; set; }

    }
}
