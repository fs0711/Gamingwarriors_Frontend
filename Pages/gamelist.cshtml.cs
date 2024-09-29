using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Diagnostics;
using System.Text.Json.Serialization;
using static POS.Pages.addemployeeModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]

    public class GameResponseModel
    {
        [JsonPropertyName("response_code")]
        public int ResponseCode { get; set; }

        [JsonPropertyName("response_data")]
        public List<GameResponseData> ResponseData { get; set; }

        [JsonPropertyName("response_message")]
        public string ResponseMessage { get; set; }
    }


    public class GameResponseData
    {
        [JsonPropertyName("game_id")]
        public string Game_Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("unit_status")]
        public string Unit_Status { get; set; }


        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("organization")]
        public string Organization { get; set; }

    }

    public class gamelistModel : PageModel
    {
        public GameResponseModel GameResponse { get; set; }

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public gamelistModel(IHttpClientFactory httpClientFactory, ILogger<ErrorModel> logger)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }



        [BindProperty]
        public ORGInputModel ORG { get; set; }

        public string SelectedOrganizationId { get; set; } // Property to hold the selected person's ID

        public SelectList OrganizationList { get; set; }

        public class ORGInputModel
        {
            [Required]
            public string Organization { get; set; }
        }


        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = HttpContext.Session.GetString("SessionToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }

            using (var client = new HttpClient())
            {

                accessToken = HttpContext.Session.GetString("SessionToken");

                client.DefaultRequestHeaders.Add("x-session-key", accessToken);
                var response = await client.GetAsync("http://127.0.0.1:5000/api/gameunit/list_gameunits");
                response.EnsureSuccessStatusCode(); // This will throw an exception if the status code is not successful

                var responseContent = await response.Content.ReadAsStringAsync();
                GameResponse = JsonSerializer.Deserialize<GameResponseModel>(responseContent);
            }

            using (var client = new HttpClient())
            {
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
            }


            return Page();
        }



    }

}

