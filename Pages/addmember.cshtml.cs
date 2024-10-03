using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using static POS.Pages.SigninModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class addmemberModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<addmemberModel> _logger;

        public addmemberModel(IHttpClientFactory httpClientFactory, ILogger<addmemberModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        [BindProperty]
        public MemberInputModel Member { get; set; }

        public string SelectedrfcardId { get; set; } // Property to hold the selected person's ID
        public SelectList rfcardList { get; set; }
        public SelectList parentList { get; set; }


        public class MemberInputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Membership_Level { get; set; }

            [Required]
            [Phone]
            public string Mobile { get; set; }

            [Required]
            public string NIC { get; set; }

            [Required]
            public string Card_id { get; set; }

            [Required]
            public string City { get; set; }

            [Required]
            public string Credit { get; set; }

            [Required]
            public string Reward { get; set; }

            [Required]
            public string Type { get; set; }

            [Required]
            public string Parent { get; set; }


        }

        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = HttpContext.Session.GetString("SessionToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }

            var cardData = new
            {
                assigned = "False"

            };

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-session-key", accessToken);
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

            //client.DefaultRequestHeaders.Add("x-session-key", accessToken);
            //var responsemember = await client.GetAsync("http://127.0.0.1:5000/api/members/list_members_id");

            //if (responsemember.IsSuccessStatusCode)
            //{
            //    var json = await responsemember.Content.ReadAsStringAsync();


            //    using (JsonDocument doc = JsonDocument.Parse(json))
            //    {
            //        var responseDatamember = doc.RootElement.GetProperty("response_data").EnumerateArray();

            //        var members = responseDatamember.Select(mem => new
            //        {
            //            Id = mem.GetProperty("member_id").GetString(),
            //            member_id = mem.GetProperty("id").GetString()
            //        }).ToList();

            //        parentList = new SelectList(members, "member_id", "Id");
            //    }
            //}


            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var client = _httpClientFactory.CreateClient();

            var accessToken = HttpContext.Session.GetString("SessionToken");

            client.DefaultRequestHeaders.Add("x-session-key", accessToken);

            var memberData = new
            {
                name = Member.Name,
                email_address = Member.Email,
                membership_level = Member.Membership_Level,
                phone_number = Member.Mobile,
                nic = Member.NIC,
                card_id = Member.Card_id,
                city = Member.City,
                credit = Member.Credit,
                reward = Member.Reward,
                type = Member.Type,
                parent = Member.Parent,
            };

            var content = new StringContent(JsonSerializer.Serialize(memberData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://127.0.0.1:5000/api/members/create", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                HttpContext.Session.SetString("FullResponse", responseContent);
                return RedirectToPage("/response");
                //return Page();
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
