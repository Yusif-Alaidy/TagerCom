namespace TagerCom.Utility
{
    public class EmailSender : IEmailSender
    {
        #region Methods

        /// <summary>
        /// Sends an email asynchronously using Gmail SMTP.
        /// </summary>
        /// <param name="email">Receiver email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlMessage">Email body in HTML format</param>
        /// <returns>A Task representing the async operation</returns>
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Create SMTP client and configure Gmail settings
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("nourenaalaidy@gmail.com", "yjyl ydzw gzzp aocm")
            };

            // Send email with HTML body
            return client.SendMailAsync(
                new MailMessage(from: "nourenaalaidy@gmail.com",
                                to: email,
                                subject,
                                htmlMessage
                                )
                {
                    IsBodyHtml = true
                });
        }

        #endregion
    }
}

