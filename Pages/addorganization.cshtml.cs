using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text;
using static POS.Pages.addemployeeModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace POS.Pages
{
    public class addorganizationModel : PageModel
    {
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //[IgnoreAntiforgeryToken]

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<addorganizationModel> _logger;

        [BindProperty]
        public OrganizationInputModel ORG { get; set; }



        public class OrganizationInputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            public string City { get; set; }

            [Required]
            public string Country { get; set; }

            [Required]
            public string CP_Name { get; set; }

            [Required]
            public string CP_Email_Address { get; set; }

            [Required]
            public string CP_Phone_Number { get; set; }

           
            public string NTN { get; set; }
        }
            public addorganizationModel(IHttpClientFactory httpClientFactory, ILogger<addorganizationModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public IActionResult OnGet()
        {


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

            var orgData = new
            {
                name = ORG.Name,
                address = ORG.Address,
                city = ORG.City,
                country = ORG.Country,
                cp_name = ORG.CP_Name,
                cp_email_address = ORG.CP_Email_Address,
                cp_phone_number = ORG.CP_Phone_Number,
                ntn = ORG.NTN
            };

            var content = new StringContent(JsonSerializer.Serialize(orgData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://127.0.0.1:5000/api/organization/create", content);

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
