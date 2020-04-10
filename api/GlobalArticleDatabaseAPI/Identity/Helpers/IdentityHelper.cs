using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Identity.Helpers
{
    public class IdentityHelper
    {
        public static string ErrorsToString(IdentityResult result)
        {
            string errors = "";

            if (!result.Succeeded)
            {
                errors = String.Join(Environment.NewLine, result.Errors.Select(s => $"{s.Description}".Trim()).ToArray());
            }

            return errors;
        }
    }
}
