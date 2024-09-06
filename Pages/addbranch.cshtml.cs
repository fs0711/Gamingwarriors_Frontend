using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.IO.Ports;

namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class addbranchModel : PageModel
    {
        
        

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);


        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<addbranchModel> _logger;

        public addbranchModel(IHttpClientFactory httpClientFactory, ILogger<addbranchModel> logger)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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

        [BindProperty]
        [Required]
        public string opening_time { get; set; }


        [BindProperty]
        [Required]
        public string closing_time { get; set; }

        [BindProperty]
        public string SerialData { get; set; }




        public IActionResult OnGet()
        {
            try
            {
                using (SerialPort serialPort = new SerialPort("COM8", 9600))
                {
                    serialPort.Open();
                    SerialData = serialPort.ReadLine();
                    serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from serial port.");
                SerialData = "Error reading from serial port.";
            }



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
                game_types,
                opening_time,
                closing_time

            };


            var content = new StringContent(JsonSerializer.Serialize(branchData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://127.0.0.1:5000/api/branch/create", content);

            if (response.IsSuccessStatusCode)
            {

                var responseContent = await response.Content.ReadAsStringAsync();
                HttpContext.Session.SetString("FullResponse", responseContent);
                return RedirectToPage("/response");
                //return Page ();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while adding the branch.");
                return Page();
            }
        }
    }

}
