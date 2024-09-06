using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using static POS.Pages.cardlistModel;
using System.Text.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class addemployeeModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<addemployeeModel> _logger;

        public addemployeeModel(IHttpClientFactory httpClientFactory, ILogger<addemployeeModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        [BindProperty]
        public EmployeeInputModel Employee { get; set; }
        public List<SelectListItem> RoleList { get; set; }

        public class EmployeeInputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [Phone]
            public string Mobile { get; set; }

            [Required]
            public string NIC { get; set; }

            [Required]
            public string Password { get; set; }

            [Required]
            public string Gender { get; set; }

            [Required]
            public string Card_id { get; set; }

            [Required]
            public string City { get; set; }

            [Required]
            public string Role { get; set; }

            [Required]
            public string Manager { get; set; }

            [Required]
            public string Organization { get; set; }

            [Required]
            public string Branch { get; set; }





        }

        public IActionResult OnGet()
        {
            RoleList = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Admin" },
            new SelectListItem { Value = "2", Text = "Owner" },
            new SelectListItem { Value = "3", Text = "Client" },
            new SelectListItem { Value = "4", Text = "Manager" },
            new SelectListItem { Value = "5", Text = "Floor Incharge" },
            new SelectListItem { Value = "6", Text = "Member" },
        };

            var accessToken = HttpContext.Session.GetString("SessionToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient();

            var accessToken = HttpContext.Session.GetString("SessionToken");

            client.DefaultRequestHeaders.Add("x-session-key", accessToken);

            var employeeData = new
            {
                name = Employee.Name,
                email_address = Employee.Email,
                phone_number = Employee.Mobile,
                nic = Employee.NIC,
                role = Employee.Role,
                password = Employee.Password,
                gender = Employee.Gender,
                card_id = Employee.Card_id,
                city = Employee.City,
                manager = Employee.Manager, 
                organization = Employee.Organization,
                branch = Employee.Branch
            };

            var content = new StringContent(JsonSerializer.Serialize(employeeData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://127.0.0.1:5000/api/users/create", content);

            if (response.IsSuccessStatusCode)
            {
                //var responseContent = await response.Content.ReadAsStringAsync();
                //HttpContext.Session.SetString("FullResponse", responseContent);
                //return RedirectToPage("/response");
                return Page();
            }
            else
            {
                _logger.LogError("Error posting member data: {StatusCode}", response.StatusCode);
                ModelState.AddModelError(string.Empty, "There was an error saving the member.");
                return Page();
            }
        }







    }

}
