using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Functions;
using EmailProvider.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailProvider.Services;

public class EmailService(EmailClient emailClient, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailClient _emailClient = emailClient;
    private readonly ILogger<EmailService> _logger = logger;

    // Hur Eposten ska vara uppbyggd för kunna skicka iväg ett mail
    // Bygg upp en model och implementera den här.
    // Stoppa in "ServiceBusReceivedMessage message" från metoden ovanför
    public EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            // Packar upp Requesten. Meddalandets body och gör om det till en string
            var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(message.Body.ToString());
            if (emailRequest != null)
                return emailRequest;
        }
        catch (Exception ex)
        {
            // Anger vilken metod som ger Error
            _logger.LogError($"ERROR : EmailSender.UnpackEmailRequest() {ex.Message}");
        }

        return null!;
    }

    public bool SendEmail(EmailRequest emailRequest)
    {
        try
        {
            var result = _emailClient.Send(
                WaitUntil.Completed,

                senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                recipientAddress: emailRequest.To,
                subject: emailRequest.Subject,
                htmlContent: emailRequest.HtmlBody,
                plainTextContent: emailRequest.PlainText);

            if (result.HasCompleted)
                return true;
        }
        catch (Exception ex)
        {
            // Anger vilken metod som ger Error
            _logger.LogError($"ERROR : EmailSender.SendEmail() {ex.Message}");
        }

        return false;
    }
}
