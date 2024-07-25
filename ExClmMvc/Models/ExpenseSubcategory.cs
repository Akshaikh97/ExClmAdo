using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExClmMvc.Models
{
    public class ExpenseSubcategory
    {
        [Key]
        public int SubcategoryId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string? SubcategoryName { get; set; }
    }
}