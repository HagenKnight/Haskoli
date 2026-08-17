using Haskoli.Application.Contracts.ExternalServices;
using Haskoli.Application.Models;
using Haskoli.Domain.Exceptions.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Haskoli.Infrastructure.Common.Services
{
    public class EmailService : IEmailService
    {
        public EmailSettings _emailSettings { get; }
        public ILogger<EmailService> _logger { get; }

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmail(Email email)
        {
            try
            {

                var client = new SendGridClient(_emailSettings.ApiKey);
                var subject = email.Subject;
                var to = new EmailAddress(email.To);
                var emailBody = email.Body;
                var from = new EmailAddress
                {
                    Email = _emailSettings.FromAddress,
                    Name = _emailSettings.FromName
                };

                var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, emailBody, emailBody);

                var response = await client.SendEmailAsync(sendGridMessage);
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    return true;
                }
                else
                {
                    _logger.LogError($"El email no pudo ser enviado. Destinatario {email.To}");
                    throw new ApiException($"Error al enviar correo, {response.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                return false;
            }
        }
    }
}
