// <copyright file="EmailService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MimeKit;
using OnlineSales.Plugin.Email.Configuration;
using OnlineSales.Plugin.Email.Exceptions;
using Serilog;

namespace OnlineSales.Plugin.Email.Services;

public class EmailService : IEmailService
{
    private readonly PluginSettings pluginSettings = new PluginSettings();

    public EmailService(IConfiguration configuration)
    {
        var settings = configuration.Get<PluginSettings>();

        if (settings != null)
        {
            pluginSettings = settings;
        }
    }

    public async Task SendAsync(string subject, string fromEmail, string fromName, string recipient, string body, List<IFormFile>? attachments)
    {
        SmtpClient client = new ();

        try
        {
            await client.ConnectAsync(pluginSettings.Email.SmtpServer, pluginSettings.Email.Port, pluginSettings.Email.UseSsl);

            await client.AuthenticateAsync(new NetworkCredential(pluginSettings.Email.Username, pluginSettings.Email.Password));

            await client.SendAsync(await GenerateEmailBody(subject, fromEmail, fromName, recipient, body, attachments));
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

    private static async Task<MimeMessage> GenerateEmailBody(string subject, string fromEmail, string fromName, string recipient, string body, List<IFormFile>? attachments)
    {
        MimeMessage message = new MimeMessage();

        message.Subject = subject;

        message.From.Add(new MailboxAddress(fromName, fromEmail));

        message.To.Add(MailboxAddress.Parse(recipient));

        BodyBuilder emailBody = new ()
        {
            HtmlBody = body,
        };

        if (attachments is not null)
        {
            foreach (var attachment in attachments)
            {
                using (var stream = attachment.OpenReadStream())
                {
                    await emailBody.Attachments.AddAsync(attachment.FileName, stream);
                }
            } 
        }

        message.Body = emailBody.ToMessageBody();

        return message;
    }
}
