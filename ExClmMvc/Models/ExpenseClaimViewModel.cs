using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExClmMvc.Models
{
    public class ExpenseClaimViewModel
    {
        public int ExpenseClaimId { get; set; }

        // For creating or updating claims
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public List<int> SubcategoryIds { get; set; } = new List<int>();

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Claim amount must be greater than zero.")]
        public decimal ClaimAmount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [Required]
        public string ExpenseLocation { get; set; } = string.Empty;

        public string? BillAttachment { get; set; }  

        public string? Remarks { get; set; }

        // For displaying information
        public string EmployeeName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubcategoryNames { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }

}