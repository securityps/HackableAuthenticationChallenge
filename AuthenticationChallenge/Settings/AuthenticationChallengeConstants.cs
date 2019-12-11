using AuthenticationChallenge.Data;
using AuthenticationChallenge.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthenticationChallenge.Settings
{
    public class AuthenticationChallengeConstants
    {
        public const string DatabaseName = "AuthenticationChallengeDB";
        public const string SessionKeyUserID = "User ID";
        public const string SessionKeyUsername = "Username";
        public const string SessionKeyTotpKey = "TotpKey";
        public const string SessionKeyAnsweredChallengeQuestions = "ChallengeQuestions";
        public const string LoginPageMessageRegisterSuccess = "RegisterSuccess";
        public const string LoginPageMessageFailure = "LoginFailure";
        public const string LoginPageMessagePasswordReset = "PasswordReset";


        public static string GetMessage(string key)
        {
            string messageValue = "";
            switch (key)
            {
                case LoginPageMessageRegisterSuccess:
                    messageValue = "Registration was successful. Please log in.";
                    break;
                case LoginPageMessageFailure:
                    messageValue = "Login Failed";
                    break;
                case LoginPageMessagePasswordReset:
                    messageValue = "Password reset succcessfully. Please log in.";
                    break;
            }
            return messageValue;
        }

        public static void AddInitialData(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            context.Add(new ChallengeQuestion { Question = "What's your favorite color?" });
            context.Add(new ChallengeQuestion { Question = "What year were you born?" });
            context.Add(new ChallengeQuestion { Question = "What is your mother's maiden name?" });
            context.Add(new ChallengeQuestion { Question = "What is the name of your first pet?" });
            context.SaveChanges();

            // Create 4 users in the format:
            // test#@test.com : Passw0rd#!
            // where # is between 1 - 4
            List<Task> userTasks = new List<Task>();
            for (int i = 1; i < 5; i++)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = "test" + i + "@test.com",
                    Email = "test" + i + "@test.com",
                    SocialSecurityNumber = $"12312123{i}",
                    AccountNumber = new string((char)(48 + i), 9),
                    LastStatementBalance = "1.1" + i
                };

                userTasks.Add(userManager.CreateAsync(user, "Passw0rd" + i + "!"));
            }

            // Create another 3 users with random details
            Random random = new Random();
            for (int i = 1; i < 4; i++)
            {
                string usernamesuffix = random.Next(0, 999).ToString("000");
                ApplicationUser user = new ApplicationUser
                {
                    UserName = "target" + usernamesuffix + "@test.com",
                    Email = "target" + usernamesuffix + "@test.com",
                    SocialSecurityNumber = random.Next(0, 999999999).ToString("000000000"),
                    AccountNumber = random.Next(0, 999999999).ToString("000000000"),
                    LastStatementBalance = random.Next(1, 999999999).ToString("0.00"),
                    Answer1 = random.Next(1, 999999999).ToString(),
                    Answer2 = random.Next(1, 999999999).ToString()
                };

                if (i == 1)
                {
                    byte[] totpbuffer = new byte[16];
                    random.NextBytes(totpbuffer);
                    user.TotpEnabled = true;
                    user.TotpSecret = totpbuffer;
                }

                userTasks.Add(userManager.CreateAsync(user, "Passw0rd" + System.Guid.NewGuid().ToString() + i + "!"));
            }

            Task.WaitAll(userTasks.ToArray());
        }
    }
}
