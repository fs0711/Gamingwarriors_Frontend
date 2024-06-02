using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class addbranchModel : PageModel
    {
        
        private readonly HttpClient _httpClient;

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<addbranchModel> _logger;

        public addbranchModel(ILogger<addbranchModel> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }
        [BindProperty]
        [Required]
        public string name { get; set; }

        [BindProperty]
        [Required]
        public string users { get; set; }

        //[BindProperty]
        //[Required]
        //[DataType(DataType.Password)]
        //public string Password { get; set; }

        //[BindProperty]
        //[Required]
        //[Phone]
        //public string Phone { get; set; }

        //[BindProperty]
        //[Required]
        //public string BranchID { get; set; }

        [BindProperty]
        [Required]
        public string city { get; set; }

        [BindProperty]
        [Required]
        public string game_types { get; set; }

        //[BindProperty]
        //[Required]
        //[EmailAddress]
        //public string Email { get; set; }



        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var branchData = new
            {
                //BranchName,
                //Phone,
                //BranchID,
                //Location,
                //Email
                name,
                users,
                city,
                game_types
            };

            var json = JsonSerializer.Serialize(branchData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://127.0.0.1:5000/api/branch/create", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Branch added: {BranchName}", name);
                return RedirectToPage("Index",response);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while adding the branch.");
                return Page();
            }
        }
    }

}
