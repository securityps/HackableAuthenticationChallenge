using System.ComponentModel.DataAnnotations;

namespace AuthenticationChallenge.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
