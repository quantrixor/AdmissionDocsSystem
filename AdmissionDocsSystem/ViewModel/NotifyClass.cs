using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AdmissionDocsSystem.ViewModel
{
    public static class NotifyClass
    {
        public static async Task SendEmailAsync(string to, string subject, string body)
        {
            using (var client = new SmtpClient("smtp.mail.ru", 587))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential("no-reply-quantrixor@mail.ru", "mEpMUie2LLHt6ASVvAM6");

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("no-reply-quantrixor@mail.ru"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
