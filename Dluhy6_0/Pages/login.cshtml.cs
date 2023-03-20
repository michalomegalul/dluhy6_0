using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using dluhy6_0;

namespace Dluhy6_0.Pages;

public class loginModel : PageModel
{
    private readonly SettleUpApp _settleUpApp;
    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }
    public loginModel(SettleUpApp settleUpApp)
    {
        _settleUpApp = settleUpApp;
    }
    public void OnGet()
    {
    }
    public async Task<IActionResult> OnPostAsync()
    {
        // Check if the user entered a valid username and password
        if (!_settleUpApp.Login(Username, Password))
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return Page();
        }
        else
        {
            Response.Cookies.Append("username", Username);
            Response.Cookies.Append("UserId", _settleUpApp.GetIdByUsername(Username).ToString());

            return RedirectToPage("/Transactions");
        }
    }
}
