using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Transactions;
using dluhy6_0;

namespace Dluhy6_0.Pages;

public class GroupTransactionsModel : PageModel
{
    private readonly SettleUpAppGroups _settleUpAppGroups;
    private readonly SettleUpApp _settleUpApp;
    public List<Transactions> Debts { get; set; }
    public IConfiguration Configuration { get; }
    [BindProperty]
    public int UserId { get; set; }
    [BindProperty]
    public int GroupId { get; set; }
    [BindProperty]
    public string Username { get; set; }
    [BindProperty]
    public decimal Amount { get; set; }
    public GroupTransactionsModel(IConfiguration configuration, SettleUpAppGroups settleUpAppGroups, IHttpContextAccessor httpContextAccessor, SettleUpApp settleUpApp)
    {
        Configuration = configuration;
        _settleUpAppGroups = new SettleUpAppGroups();
        _settleUpApp = new SettleUpApp();
        Debts = new List<Transactions>();

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
    public IActionResult OnPostCreateDebt(int GroupId, decimal Amount)
    {
        _settleUpAppGroups.CreateGroupTransaction(UserId, GroupId, Amount);
        return Page();
    }
    public List<dluhy6_0.User> GetUsers()
    {
        List<dluhy6_0.Group> groupid = _settleUpAppGroups.GetGroupsForUser(UserId);
        List<dluhy6_0.User> Getusers = new List<dluhy6_0.User>();
        for (int i = 0; i < groupid.Count; i++)
        {
            Getusers = _settleUpAppGroups.GetUsersForGroup(groupid[i].Id);
        }
        return Getusers;
    }
    public List<dluhy6_0.Group> GetGroups()
    {
        return _settleUpAppGroups.GetGroupsForUser(UserId);
    }

    public void OnGet()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            Debts = _settleUpAppGroups.GetDebtsForUser(int.Parse(userId));
        }
    }
    public IActionResult OnPostAddUser(int groupId, string username)
    {
        if (_settleUpApp.UserExists(0, username))
        {
            if (!_settleUpAppGroups.UserInGroup(groupId, _settleUpApp.GetIdByUsername(username)))
            {
                _settleUpAppGroups.AddUserToGroup(_settleUpApp.GetUserByUsername(username).ID, groupId);
                return RedirectToPage(); // or return a redirect to a different page
            }
            else // user is already in group
            {
                ModelState.AddModelError("", "User is already in group");
                return Page();
            }
            
        }
        else
        {
            ModelState.AddModelError("", "Invalid username");
            return Page();
        }
    }

}
