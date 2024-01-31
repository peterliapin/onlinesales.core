// <copyright file="EmailService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using OnlineSales.Configuration;
using OnlineSales.DTOs;
using OnlineSales.Interfaces;

namespace OnlineSales.Services;

public class EmailService : IEmailService
{
    private readonly EmailConfig config = new EmailConfig();

    public EmailService(IConfiguration configuration)
    {
        var settings = configuration.GetSection("Email").Get<EmailConfig>();

        if (settings != null)
        {
            config = settings;
        }
        else
        {
            throw new MissingConfigurationException($"The specified configuration section for the type {typeof(EmailConfig).FullName} could not be found in the settings file.");
        }
    }

    public async Task<string> SendAsync(string subject, string fromEmail, string fromName, string[] recipients, string body, List<AttachmentDto>? attachments)
    {
        var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(config.Server, config.Port, config.UseSsl);

            await client.AuthenticateAsync(new NetworkCredential(config.UserName, config.Password));

            var message = await GenerateEmailBody(subject, fromEmail, fromName, recipients, body, attachments);

            await client.SendAsync(message);

            return message.MessageId;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error sending emails");

            switch (exception)
            {
                case AuthenticationException:
                    throw new EmailException("Error in authentication", exception);

                case ServiceNotAuthenticatedException:
                    throw new EmailException("Error in authentication", exception);

                case ServiceNotConnectedException:
                    throw new EmailException("Error connecting to smtp host", exception);

                default:
                    throw new EmailException("Error sending emails", exception);
            }
        }
        finally
        {
            client.Disconnect(true);

            client.Dispose();
        }
    }

    private static async Task<MimeMessage> GenerateEmailBody(string subject, string fromEmail, string fromName, string[] recipients, string body, List<AttachmentDto>? attachments)
    {
        var message = new MimeMessage();

        message.Subject = subject;

        message.From.Add(new MailboxAddress(fromName, fromEmail));

        foreach (var receipent in recipients)
        {
            message.To.Add(MailboxAddress.Parse(receipent));
        }

        var emailBody = new BodyBuilder()
        {
            HtmlBody = body,
        };

        if (attachments is not null)
        {
            foreach (var attachment in attachments)
            {
                using (var stream = new MemoryStream(attachment.File))
                {
                    await emailBody.Attachments.AddAsync(attachment.FileName, stream);
                }
            }
        }

        message.Body = emailBody.ToMessageBody();

        return message;
    }
}