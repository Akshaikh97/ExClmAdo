using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExClmMvc.Models
{
    public class ExpenseClaim
    {
        [Key]
        public int ExpenseClaimId { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Subcategory is required.")]
        public string? SubcategoryIds { get; set; }

        [Required(ErrorMessage = "Claim Amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Claim Amount must be a positive value.")]
        public decimal ClaimAmount { get; set; }

        [Required(ErrorMessage = "Expense Date is required.")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/1753", "12/31/9999", ErrorMessage = "Expense Date must be between 1/1/1753 and 12/31/9999.")]
        public DateTime ExpenseDate { get; set; }

        public string? ExpenseLocation { get; set; }
        public string? BillAttachment { get; set; }
        public string? Remarks { get; set; }

        //one to many
        public virtual Employee? Employee { get; set; }
        public virtual ExpenseCategory? Category { get; set; }
        public string? SubcategoryNames { get; set; }
    }

}