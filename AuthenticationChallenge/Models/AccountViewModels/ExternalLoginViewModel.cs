using System.ComponentModel.DataAnnotations;

namespace AuthenticationChallenge.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
