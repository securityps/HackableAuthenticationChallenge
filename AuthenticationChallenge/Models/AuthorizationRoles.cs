using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationChallenge.Models
{
    public class AuthorizationRoles
    {
        public const string Administrator = "Administrator";
        public const string User = "User";
        public static List<string> AllRoles = new List<string> { Administrator,User};
    }
}
