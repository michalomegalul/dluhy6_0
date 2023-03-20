using dluhy6_0;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;

namespace Dluhy6_0.Pages
{
    public class TransactionsModel : PageModel
    {
        private readonly SettleUpApp _settleUpApp;
        public int UserId { get; }
        public string UserName { get; }
        public decimal Balance { get; set; }
        public TransactionsModel(SettleUpApp settleUpApp, IHttpContextAccessor httpContextAccessor)
        {
            _settleUpApp = settleUpApp;

            // Retrieve and validate the user ID from the cookie
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

            UserName = Convert.ToString(_settleUpApp.GetUserById(UserId).username);
        }

        public void OnGet()
        {
            // This method can be empty since the user ID has already been retrieved and validated in the constructor
        }

        public List<Transactions> GetTransactions()
        {
            return _settleUpApp.GetTransactionsForUser(UserId);
        }

        public double GetBalance()
        {
            return _settleUpApp.GetBalance(UserId);
        }

        public void CreateTransaction(int giver, int receiver, decimal amount)
        {
            if (giver == null || receiver == null)
            {
                return;
            }

            _settleUpApp.CreateTransaction(giver, receiver, amount);
        }


        public void OnPost()
        {
            
            decimal amount = 0;
            var GID = UserId;
            var RIDString = Request.Form["receiver"].ToString();
            var amountString = Request.Form["amount"].ToString();

            // Check for errors
            if (GID == 0 || string.IsNullOrWhiteSpace(RIDString))
            {
                TempData["ErrorMessage"] = "Please select a giver and a receiver.";
                return;
            }
            else if (string.IsNullOrWhiteSpace(amountString))
            {
                TempData["ErrorMessage"] = "Please enter an amount.";
                return;
            }
            else if (!decimal.TryParse(amountString, out amount) || amount <= 0)
            {
                TempData["ErrorMessage"] = "Invalid amount. Please enter a positive number.";
                return;
            }
            else if (amount > 99999999999999999)
            {
                TempData["ErrorMessage"] = "Amount is too large. Please enter a smaller amount.";
                return;
            }
            
            


            // Check that RIDString is not null or empty
            if (string.IsNullOrWhiteSpace(RIDString))
            {
                TempData["ErrorMessage"] = "Please select a receiver.";
                return;
            }

            var RID = int.Parse(RIDString);
            CreateTransaction(GID, RID, amount);
            if (GID == RID)
            {
                TempData["ErrorMessage"] = "You cannot send money to yourself.";
                return;
            }
            if (_settleUpApp.UserExists(RID, UserName))
            {
                TempData["ErrorMessage"] = "User does not exist. Please enter valid user";
            }
            // Redirect to current page to refresh data
            Response.Redirect("/transactions");
        }

    }
}
