using System.ComponentModel.DataAnnotations;

namespace AuthenticationChallenge.Models.AccountViewModels
{
    public class SetupChallengeQuestionsViewModel
    {
        public string Username { get; set; }

        public ChallengeQuestion Question1 { get; set; }
        public ChallengeQuestion Question2 { get; set; }
        [Required]
        [MinLength(1)]
        public string Answer1 { get; set; }
        [Required]
        [MinLength(1)]
        public string Answer2 { get; set; }

    }
}
