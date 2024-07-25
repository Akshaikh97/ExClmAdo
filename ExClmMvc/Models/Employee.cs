using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExClmMvc.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [StringLength(10)]
        public string? Code { get; set; }

        [Required]
        [StringLength(50)]
        public string? Designation { get; set; }

        [Required]
        [StringLength(50)]
        public string? Department { get; set; }
    }
}