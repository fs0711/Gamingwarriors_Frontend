using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text;

namespace POS.Pages
{
    public class SigninModel : PageModel
    {
        private readonly ILogger<SigninModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SigninModel(ILogger<SigninModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        public class LoginInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }


        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var loginData = new
            {
                email_address = Input.Email,
                password = Input.Password
            };

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://127.0.0.1:5000/api/users/login", content);

            if (response.IsSuccessStatusCode)
            {
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<ResponseModel>(responseContent);

                string accessToken = responseObject.response_data.access_token;
                
                HttpContext.Session.SetString("SessionToken", accessToken);
                
                if(accessToken != null)
                {
                    
                    return RedirectToPage("/Index");
                }
                return RedirectToPage("/Response");
            }
            else
            {
                ErrorMessage = "Invalid login attempt.";
                return Page();
            }
        }
        public class ResponseModel
        {
            public int response_code { get; set; }
            public ResponseData response_data { get; set; }
            public string response_message { get; set; }
        }
        
        public class ResponseData
        {
            public string access_token { get; set; }
            // You can include other properties if needed
        }
    }

}
