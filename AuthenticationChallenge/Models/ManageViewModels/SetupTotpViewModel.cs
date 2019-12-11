using System.ComponentModel.DataAnnotations;

namespace AuthenticationChallenge.Models.ManageViewModels
{
    public class SetupTotpViewModel
    {
        public string QRCodeBase64 { get; set; }

        [Required]
        [MinLength(12)]
        public string ConfirmPasswordAndPIN { get; set; }
    }
}
