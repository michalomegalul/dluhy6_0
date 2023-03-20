using dluhy6_0;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dluhy6_0.Pages
{
    public class CreateGroupModel : PageModel
    {
        private readonly SettleUpAppGroups _settleUpAppGroups;

        [BindProperty]
        public string GroupName { get; set; }
        [BindProperty]
        public string GroupDescription { get; set; }
        [BindProperty]
        public int UserId { get; set; }

        public string ErrorMessage { get; set; }

        public CreateGroupModel(SettleUpAppGroups settleUpAppGroups, IHttpContextAccessor httpContextAccessor)
        {
            _settleUpAppGroups = settleUpAppGroups;
            if (!httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("UserId", out string userId))
            {
                TempData["ErrorMessage"] = "Handle user ID not found in cookie";
                return;
            }

            if (!int.TryParse(userId, out int id))
            {
                TempData["ErrorMessage"] = "Handle invalid user ID in cookie";
                return;
            }
            UserId = id;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please enter a group name.";
                return Page();
            }

            // Check if group already exists
            if (_settleUpAppGroups.GroupExists(-8, GroupName))
            {
                ErrorMessage = "A group with this name already exists.";
                return Page();
            }


            // Create the new group
            _settleUpAppGroups.CreateGroup(GroupName, GroupDescription, UserId);

            return RedirectToPage("GroupTransactions");
        }
    }
}
