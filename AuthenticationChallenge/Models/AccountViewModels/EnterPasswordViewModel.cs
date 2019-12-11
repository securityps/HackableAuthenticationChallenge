using System.ComponentModel.DataAnnotations;

namespace AuthenticationChallenge.Models.AccountViewModels
{
    public class EnterPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Username { get; set; }
    }
}
