using Azure.Communication.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using KarateInstitute.Model;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;
using System.Runtime.Serialization;
using System;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace KarateInstitute.Functions
{
    public class ProcessFormInput
    {
        private readonly ILogger<ProcessFormInput> _logger;
        private readonly EmailClient _emailClient;

        public ProcessFormInput(ILogger<ProcessFormInput> logger, EmailClient emailClient)
        {
            _logger = logger;
            _emailClient = emailClient;
        }

        [Function("ProcessFormInput")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
        [FromBody] FormInput formInput
        )

        {

            // Email verification
            bool isEmailValid = VerifiedEmail(formInput.Email);

            if (!isEmailValid)
            {
                _logger.LogWarning($"Invalid email address: {formInput.Email}");
                return new BadRequestObjectResult("Invalid email address.");
            }

            var subject = "Welcome to Azure Communication Service Email APIs.";
            var htmlContent = $@"
            <html>
                <table border='1'>
                    <tr><th>Property</th><th>Value</th></tr>
                    <tr><td>Ime</td><td>{formInput.Ime}</td></tr>
                    <tr><td>Priimek</td><td>{formInput.Priimek}</td></tr>
                    <tr><td>NaslovBivalisca</td><td>{formInput.NaslovBivalisca}</td></tr>
                    <tr><td>PostnaSt</td><td>{formInput.PostnaSt}</td></tr>
                    <tr><td>Kraj</td><td>{formInput.Kraj}</td></tr>
                    <tr><td>DatumRoj</td><td>{formInput.DatumRoj}</td></tr>
                    <tr><td>Email</td><td>{formInput.Email}</td></tr>
                    <tr><td>Telefon</td><td>{formInput.Telefon}</td></tr>
                    <tr><td>Tecaj</td><td>{formInput.Tecaj}</td></tr>
                    <tr><td>Obisk</td><td>{formInput.Obisk}</td></tr>
                </table>
                </html>";

            var sender = "donotreply@mojespretnosti.si";
            var recipient = new EmailAddress(formInput.Email);
            var reviewerRecipient = new EmailAddress("erin.coralic@gmail.com"); // nina

            // create email message
            var emailRecipients = new EmailRecipients([recipient, reviewerRecipient]);
            var emailContent = new EmailContent(subject);
            emailContent.Html = htmlContent;
            var emailMessage = new EmailMessage(sender, emailRecipients, emailContent);

            // Call UpdateStatus on the email send operation to poll for the status
            // manually.
            try
            {
                // Send the email message with WaitUntil.Started
                EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Email send failed with Code = {ex.ErrorCode} and Message = {ex.Message}");
                return new BadRequestObjectResult(ex);
            }

            return new OkObjectResult(formInput);
        }
        private bool VerifiedEmail(string emailInput)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(emailInput, emailPattern);
        }
    }
}
