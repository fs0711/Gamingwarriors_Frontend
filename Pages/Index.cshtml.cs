using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace POS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        public IndexModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var accessToken = HttpContext.Session.GetString("SessionToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }

            // Handle GET request if needed
            return Page();
        }
    }
}
