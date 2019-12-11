using System.Threading.Tasks;

namespace AuthenticationChallenge.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
