using Haskoli.Application.Models;

namespace Haskoli.Application.Contracts.ExternalServices
{
    public interface IEmailService
    {

        Task<bool> SendEmail(Email email);
    }
}
