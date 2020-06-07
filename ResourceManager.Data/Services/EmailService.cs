using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager.Data.Services
{
    public class EmailService : ControllerBase, IEmailService
    {
        public async Task<IActionResult> NotifyByEmail(Guid receiverID, string receiverAddress, string subject, string message, IConfiguration _configuration)
        {
                var msg = new MimeMessage();
                var bodyBuilder = new BodyBuilder();
                var pass = _configuration.GetSection("NotificationEmail").GetSection("GmailAppPassword").Value;
                var username = _configuration.GetSection("NotificationEmail").GetSection("GmailAddress").Value;

                var from = new MailboxAddress("AutoMail", "auto@creativecookies.it");
                var to = new MailboxAddress(receiverID.ToString(), receiverAddress);

                msg.From.Add(from);
                msg.To.Add(to);

                msg.Subject = subject;

                bodyBuilder.HtmlBody = $"<h1>Resource Manager</h1><br /><div class=\"container\">{message}</div>";

                msg.Body = bodyBuilder.ToMessageBody();

                var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync("smtp.gmail.com", 587, false);
                await smtpClient.AuthenticateAsync(username, pass);
                await smtpClient.SendAsync(msg);
                smtpClient.Disconnect(true);
                return Ok("Should be sent already...");
        }
    }
}
