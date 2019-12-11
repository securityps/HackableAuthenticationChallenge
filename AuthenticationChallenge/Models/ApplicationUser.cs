using Microsoft.AspNetCore.Identity;

namespace AuthenticationChallenge.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        #region TOTP
        public byte[] TotpSecret { get; set; }

        public bool TotpEnabled { get; set; }
        #endregion

        #region ChallengeQuestions
        public virtual ChallengeQuestion Question1 { get; set; }
        public virtual ChallengeQuestion Question2 { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }

        public bool VerifyAnswers(string answer1, string answer2)
        {
            return Answer1.Trim().ToLower() == answer1.Trim().ToLower() && Answer2.Trim().ToLower() == answer2.Trim().ToLower();
        }

        public bool HasSetupChallengeQuestions()
        {
            return Answer1 != null && Answer2 != null;
        }
        #endregion

        #region forgotpassword
        public string SocialSecurityNumber { get; set; }
        public string AccountNumber { get; set; }
        public string LastStatementBalance { get; set; }

        public bool ValidateForgotPasswordFields(string socialSecurityNumber, string accountNumber, string lastStatementBalance)
        {
            return SocialSecurityNumber == socialSecurityNumber && accountNumber == AccountNumber && lastStatementBalance == LastStatementBalance;
        }
        #endregion
    }
}

