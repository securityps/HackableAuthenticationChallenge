using System.ComponentModel.DataAnnotations;

namespace AuthenticationChallenge.Models.AccountViewModels
{
    public class AlternateLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
