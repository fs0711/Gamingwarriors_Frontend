using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using POS.Services;



namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]


    public class EmployeeViewModel
    {
        public int Role { get; set; }

    }
    public class addemployeeModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<addemployeeModel> _logger;
        private readonly SerialPortService _serialPortService;


        public addemployeeModel(IHttpClientFactory httpClientFactory, ILogger<addemployeeModel> logger, SerialPortService serialPortService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _serialPortService = serialPortService;
        }
        [BindProperty]
        public EmployeeInputModel Employee { get; set; }
        public EmployeeViewModel Emp { get; set; } = new EmployeeViewModel();

        public string ReceivedData { get; private set; }

        public string cleanedData { get; private set; }
        public string ExtractedcardUID { get; private set; }

        public string SelectedRoleId { get; set; }
        public SelectList RoleList { get; set; }

        public string SelectedOrganizationId { get; set; } // Property to hold the selected person's ID

        public SelectList OrganizationList { get; set; }

        public string SelectedBranchId { get; set; } // Property to hold the selected person's ID

        public SelectList BranchList { get; set; }

        public string SelectedrfcardId { get; set; } // Property to hold the selected person's ID

        public SelectList rfcardList { get; set; }

        public SelectList cardList { get; set; }


        public string SelectedManagerId { get; set; } // Property to hold the selected person's ID

        public SelectList ManagerList { get; set; }


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




        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = HttpContext.Session.GetString("SessionToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://127.0.0.1:5000/api/static-data");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var rolesArray = doc.RootElement.GetProperty("user_roles_and_rights").EnumerateArray();

                    var roles = rolesArray.Select(role => new
                    {
                        Id = role.GetProperty("user_role_id").GetInt32(),
                        Name = role.GetProperty("name").GetString()
                    }).ToList();

                    RoleList = new SelectList(roles, "Id", "Name");
                }
            }


            accessToken = HttpContext.Session.GetString("SessionToken");

            client.DefaultRequestHeaders.Add("x-session-key", accessToken);

            var responseorg = await client.GetAsync("http://127.0.0.1:5000/api/organization/list_organization");

            if (responseorg.IsSuccessStatusCode)
            {
                var json = await responseorg.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var responseDataOrg = doc.RootElement.GetProperty("response_data").EnumerateArray();

                    var orgs = responseDataOrg.Select(orgs => new
                    {
                        Id = orgs.GetProperty("id").GetString(),
                        Name = orgs.GetProperty("name").GetString()
                    }).ToList();

                    OrganizationList = new SelectList(orgs, "Id", "Name");
                }
            }

            BranchList = new SelectList(new List<SelectListItem>());


            var responsemanager = await client.GetAsync("http://127.0.0.1:5000/api/users/read");

            if (responsemanager.IsSuccessStatusCode)
            {
                var json = await responsemanager.Content.ReadAsStringAsync();


                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var responseDataManager = doc.RootElement.GetProperty("response_data").EnumerateArray();

                    var managers = responseDataManager.Select(managers => new
                    {
                        Id = managers.GetProperty("id").GetString(),
                        Name = managers.GetProperty("name").GetString()
                    }).ToList();

                    ManagerList = new SelectList(managers, "Id", "Name");
                }
            }

            var cardData = new
            {
                assigned = "False"

            };

            var responserfcard = await client.GetAsync("http://127.0.0.1:5000/api/rfid/list_rfcards");

            if (responserfcard.IsSuccessStatusCode)
            {
                var json = await responserfcard.Content.ReadAsStringAsync();


                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var responseDatarfcard = doc.RootElement.GetProperty("response_data").EnumerateArray();

                    var rfcards = responseDatarfcard.Select(rfcards => new
                    {
                        Id = rfcards.GetProperty("card_id").GetString(),
                        card_id = rfcards.GetProperty("id").GetString()
                    }).ToList();

                    rfcardList = new SelectList(rfcards, "card_id", "Id");
                }
            }

            //cardList = new SelectList(new List<SelectListItem>());

            return Page();
        }

        //[HttpGet]
        //public async Task<JsonResult> OnGetDataAsync()
        //{
        //    ReceivedData = _serialPortService.GetLatestData();
        //    if (!string.IsNullOrEmpty(ReceivedData))
        //    {
        //        cleanedData = ReceivedData.Replace("&#xD;", "").Replace("&#xA;", "").Trim();
        //        ExtractedcardUID = cleanedData.Substring(cleanedData.IndexOf(':') + 1).Replace(" ", "").Trim().ToUpper();
        //    }

        //    return new JsonResult(new { extractedCardUID = ExtractedcardUID });
        //}


        //[HttpGet]
        //public async Task<IActionResult> OnGetCardsAsync()
        //{
        //    var client = _httpClientFactory.CreateClient();
        //    var accessToken = HttpContext.Session.GetString("SessionToken");
        //    var responsecards = await client.GetAsync("http://127.0.0.1:5000/api/rfid/list_rfcard_ids");

        //    if (responsecards.IsSuccessStatusCode)
        //    {
        //        var json = await responsecards.Content.ReadAsStringAsync();

        //        using (JsonDocument doc = JsonDocument.Parse(json))
        //        {
        //            var responseDatarfcard = doc.RootElement.GetProperty("response_data").EnumerateArray();

        //            string targetCardUid = "C3CD1204";

        //            var filteredCards = responseDatarfcard
        //                .Where(card => card.GetProperty("card_uid").GetString() == targetCardUid)
        //                .Select(card => new
        //                {
        //                    CardId = card.GetProperty("card_id").GetString(),
        //                    Id = card.GetProperty("id").GetString()
        //                })
        //                .ToList();


        //            return new JsonResult(filteredCards.Select(b => new SelectListItem { Value = b.Id, Text = b.CardId }));
        //        }
        //    }
        //    return new JsonResult(new List<SelectListItem>());
        //}



        [HttpGet]
        public async Task<IActionResult> OnGetBranchesAsync(string organizationId)
        {
            var client = _httpClientFactory.CreateClient();
            var accessToken = HttpContext.Session.GetString("SessionToken");

            client.DefaultRequestHeaders.Add("x-session-key", accessToken);

            // Fetch the branches from the API
            var response = await client.GetAsync("http://127.0.0.1:5000/api/branch/list_branchs_ids");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var responseDataBranch = doc.RootElement.GetProperty("response_data").EnumerateArray();

                    // Filter branches based on the selected organizationId
                    var filteredBranches = responseDataBranch
                        .Where(branch => branch.GetProperty("organization").GetString() == organizationId)
                        .Select(branch => new
                        {
                            Id = branch.GetProperty("id").GetString(),
                            Name = branch.GetProperty("name").GetString()
                        })
                        .ToList();

                    // Return filtered branches
                    return new JsonResult(filteredBranches.Select(b => new SelectListItem { Value = b.Id, Text = b.Name }));
                }
            }

            return new JsonResult(new List<SelectListItem>());
        }




        public async Task<IActionResult> OnPostAsync()
        {


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
                
                return Page();
            }
            else
            {
                _logger.LogError("Error posting member data: {StatusCode}", response.StatusCode);
                ModelState.AddModelError(string.Empty, "There was an error saving the member.");
                //return Page();
                return RedirectToPage("/response");
            }
        }


    }

}
