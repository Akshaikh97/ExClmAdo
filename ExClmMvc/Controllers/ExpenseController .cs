using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExClmMvc.Models;

namespace ExClmMvc.Controllers
{
    [Route("[controller]")]
    public class ExpenseController : Controller
    {
        private readonly SqlConnection con;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public ExpenseController(SqlConnection _con, IWebHostEnvironment hostingEnvironment)
        {
            con = _con;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var claims = GetAllClaims();
            var employees = GetAllEmployees();
            var categories = GetAllCategories();
            var totalClaimAmount = GetTotalClaimAmount();

            foreach (var claim in claims)
            {
                var employee = employees.FirstOrDefault(e => e.EmployeeId == claim.EmployeeId);
                var category = categories.FirstOrDefault(c => c.CategoryId == claim.CategoryId);

                claim.EmployeeName = employee?.Name ?? "Unknown";
                claim.CategoryName = category?.CategoryName ?? "Unknown";

                var subcategoryIds = claim.SubcategoryIds;
                var subcategoryNames = GetSubcategoriesByIds(subcategoryIds).Select(s => s.SubcategoryName).ToList();
                claim.SubcategoryNames = string.Join(", ", subcategoryNames);
            }

            ViewBag.TotalClaimAmount = totalClaimAmount;

            return View(claims);
        }
        [HttpGet("Create")]
        public IActionResult Create()
        {
            var viewModel = new ExpenseClaimViewModel
            {
                Employees = GetAllEmployees().Select(e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = e.Name
                }),
                Categories = GetAllCategories().Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
            };

            return View(viewModel);
        }
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExpenseClaimViewModel viewModel, IFormFile billAttachment)
        {
            if (ModelState.IsValid)
            {
                var employee = GetAllEmployees().FirstOrDefault(e => e.EmployeeId == viewModel.EmployeeId);
                if (employee != null)
                {
                    switch (employee.Designation)
                    {
                        case "CEO":
                            if (viewModel.ClaimAmount > 10000)
                                ModelState.AddModelError("ClaimAmount", "Claim amount exceeds the limit for CEO.");
                            break;
                        case "Manager":
                            if (viewModel.ClaimAmount > 7000)
                                ModelState.AddModelError("ClaimAmount", "Claim amount exceeds the limit for Manager.");
                            break;
                        default:
                            if (viewModel.ClaimAmount > 5000)
                                ModelState.AddModelError("ClaimAmount", "Claim amount exceeds the limit for employees.");
                            break;
                    }
                }

                if (viewModel.ExpenseDate < new DateTime(1753, 1, 1) || viewModel.ExpenseDate > new DateTime(9999, 12, 31))
                {
                    ModelState.AddModelError("ExpenseDate", "Expense date is out of range.");
                }

                if (ModelState.IsValid)
                {
                    string filePath = null;
                    if (billAttachment != null)
                    {
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + billAttachment.FileName;
                        filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            billAttachment.CopyTo(stream);
                        }

                        // Store relative path for database
                        filePath = Path.Combine("Images", uniqueFileName);
                    }

                    var expenseClaim = new ExpenseClaim
                    {
                        EmployeeId = viewModel.EmployeeId,
                        CategoryId = viewModel.CategoryId,
                        SubcategoryIds = string.Join(",", viewModel.SubcategoryIds),
                        ClaimAmount = viewModel.ClaimAmount,
                        ExpenseDate = viewModel.ExpenseDate,
                        ExpenseLocation = viewModel.ExpenseLocation,
                        BillAttachment = filePath,
                        Remarks = viewModel.Remarks
                    };

                    AddClaim(expenseClaim);
                    return RedirectToAction(nameof(Index));
                }
            }

            viewModel.Employees = GetAllEmployees().Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = e.Name
            });
            viewModel.Categories = GetAllCategories().Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            });

            return View(viewModel);
        }
        [HttpGet("GetSubcategories")]
        public JsonResult GetSubcategories(int categoryId)
        {
            var subcategories = GetSubcategoriesByCategoryId(categoryId);
            return Json(subcategories);
        }
        private decimal GetTotalClaimAmount()
        {
            decimal totalClaimAmount = 0;
            var command = new SqlCommand("GetTotalClaimAmount", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            var result = command.ExecuteScalar();
            if (result != null)
            {
                totalClaimAmount = Convert.ToDecimal(result);
            }
            con.Close();

            return totalClaimAmount;
        }
        private List<ExpenseCategory> GetAllCategories()
        {
            var categories = new List<ExpenseCategory>();

            var command = new SqlCommand("GetAllCategories", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    categories.Add(new ExpenseCategory
                    {
                        CategoryId = reader.GetInt32(0),
                        CategoryName = reader.GetString(1)
                    });
                }
            }
            con.Close();

            return categories;
        }
        private List<ExpenseClaimViewModel> GetAllClaims()
        {
            var claims = new List<ExpenseClaimViewModel>();

            var command = new SqlCommand("GetAllClaims", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    claims.Add(new ExpenseClaimViewModel
                    {
                        ExpenseClaimId = reader.GetInt32(0),
                        EmployeeId = reader.GetInt32(1),
                        CategoryId = reader.GetInt32(2),
                        SubcategoryIds = reader.IsDBNull(3) ? new List<int>() : reader.GetString(3).Split(',').Select(int.Parse).ToList(),
                        ClaimAmount = reader.GetDecimal(4),
                        ExpenseDate = reader.GetDateTime(5),
                        ExpenseLocation = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        BillAttachment = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        Remarks = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                    });
                }
            }
            con.Close();

            return claims;
        }
        private List<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>();

            var command = new SqlCommand("GetAllEmployees", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    employees.Add(new Employee
                    {
                        EmployeeId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Code = reader.GetString(2),
                        Designation = reader.GetString(3),
                        Department = reader.GetString(4)
                    });
                }
            }
            con.Close();

            return employees;
        }
        private List<ExpenseSubcategory> GetSubcategoriesByIds(List<int> subcategoryIds)
        {
            var subcategories = new List<ExpenseSubcategory>();

            var ids = string.Join(",", subcategoryIds);
            var command = new SqlCommand("GetSubcategoriesByIds", con) { CommandType = CommandType.StoredProcedure };
            command.Parameters.AddWithValue("@SubcategoryIds", ids);
            con.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    subcategories.Add(new ExpenseSubcategory
                    {
                        SubcategoryId = reader.GetInt32(0),
                        CategoryId = reader.GetInt32(1),
                        SubcategoryName = reader.GetString(2)
                    });
                }
            }
            con.Close();

            return subcategories;
        }
        private List<ExpenseSubcategory> GetSubcategoriesByCategoryId(int categoryId)
        {
            var subcategories = new List<ExpenseSubcategory>();

            var command = new SqlCommand("GetSubcategoriesByCategoryId", con) { CommandType = CommandType.StoredProcedure };
            command.Parameters.AddWithValue("@CategoryId", categoryId);
            con.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    subcategories.Add(new ExpenseSubcategory
                    {
                        SubcategoryId = reader.GetInt32(0),
                        CategoryId = reader.GetInt32(1),
                        SubcategoryName = reader.GetString(2)
                    });
                }
            }
            con.Close();

            return subcategories;
        }
        private void AddClaim(ExpenseClaim claim)
        {
            var command = new SqlCommand("AddExpenseClaim", con) { CommandType = CommandType.StoredProcedure };
            command.Parameters.AddWithValue("@EmployeeId", claim.EmployeeId);
            command.Parameters.AddWithValue("@CategoryId", claim.CategoryId);
            command.Parameters.AddWithValue("@SubcategoryIds", claim.SubcategoryIds);
            command.Parameters.AddWithValue("@ClaimAmount", claim.ClaimAmount);
            command.Parameters.AddWithValue("@ExpenseDate", claim.ExpenseDate);
            command.Parameters.AddWithValue("@ExpenseLocation", claim.ExpenseLocation);
            command.Parameters.AddWithValue("@BillAttachment", claim.BillAttachment ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Remarks", claim.Remarks);
            con.Open();
            command.ExecuteNonQuery();
            con.Close();
        }
    }
}
