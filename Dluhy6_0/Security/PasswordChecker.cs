using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace dluhy6_0.Security;

public class PasswordChecker
{
    public static bool CheckPassword(string password)
    {
        if (password.Length < 6)
        {
            return false;
        }
        else if (password.Length > 20)
        {
            return false;
        }

        /*            else if (!password.Any(char.IsUpper))
                    {
                        return false;
                    }
                    else if (!password.Any(char.IsLower))
                    {
                        return false;
                    }
                    else if (!password.Any(char.IsDigit))
                    {
                        return false;
                    }
                    else if (!password.Any(char.IsSymbol))
                    {
                        return false;
                    }*/
        else
        {
            return true;
        }
    }
}