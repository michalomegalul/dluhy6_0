using System.ComponentModel.DataAnnotations;
using dluhy6_0;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dluhy6_0.Pages;

public class RegisterModel : PageModel
{
    private readonly SettleUpApp _settleUpApp;

    public RegisterModel(SettleUpApp settleUpApp)
    {
        _settleUpApp = settleUpApp;
    }

    [BindProperty]
    [Required]
    public string Username { get; set; }

    [BindProperty]
    [DataType(DataType.Password)]
    [Required]
    public string Password { get; set; }

    [BindProperty]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    [Required]
    public string ConfirmPassword { get; set; }

    public string ErrorMessage { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Something went wrong");
            return Page();
        }
        else
        {
            _settleUpApp.CreateUser(Username, Password);
            Response.Cookies.Append("UserId", _settleUpApp.GetIdByUsername(Username).ToString());
            return RedirectToPage("/Transactions");
        }
    }
}
