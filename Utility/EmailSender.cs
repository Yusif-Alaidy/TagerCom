


namespace TagerCom.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 465)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                // Here You will enter Your Gmail And App Password
                Credentials = new NetworkCredential("", "")
            };

            return client.SendMailAsync(
    message: new MailMessage(from: "",
                             to: email,
                             subject,
                             htmlMessage
                            )
    {
        IsBodyHtml = true
    });


        }
    }
}
