using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Email
{
    public class EmailHelper : IEmailHelper
    {
        private readonly ILogger<EmailHelper> _logger;
        private readonly EmailConfig _emailConfig;

        public EmailHelper(ILogger<EmailHelper> logger,
            IOptions<EmailConfig> options)
        {
            _logger = logger;
            _emailConfig = options.Value;
        }

        ///<inheritdoc cref="IEmailHelper.SendTextToUserAsync(string, string, ToAccessVm)"/>
        public async Task<bool> SendTextToUserAsync(string subject, string content, ToAccessVm toAccess)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentNullException("标题不能为空");
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException("内容不能为空");
            if (toAccess is null)
                throw new ArgumentNullException("接收人不能为空");

            var address = new MailboxAddress(toAccess.ToName, toAccess.Address);//收件人地址
            return await SendAsync(subject, new BodyBuilder { TextBody = content }, new List<MailboxAddress> { address });
        }

        ///<inheritdoc cref="IEmailHelper.SendToUsersAsync(string, BodyBuilder, IEnumerable{ToAccessVm})"/>
        public async Task<bool> SendTextToUsersAsync(string subject, string content, IEnumerable<ToAccessVm> toAccesses)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentNullException("标题不能为空");
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException("内容不能为空");
            if (!toAccesses.Any())
                throw new ArgumentNullException("接收人不能为空");

            var address = toAccesses.Select(t => new MailboxAddress(t.ToName, t.Address));//收件人地址
            return await SendAsync(subject, new BodyBuilder { TextBody = content }, address);
        }

        ///<inheritdoc cref="IEmailHelper.SendHtmlToUserAsync(string, string, ToAccessVm)"/>
        public async Task<bool> SendHtmlToUserAsync(string subject, string content, ToAccessVm toAccess)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentNullException("标题不能为空");
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException("内容不能为空");
            if (toAccess is null)
                throw new ArgumentNullException("接收人不能为空");

            var address = new MailboxAddress(toAccess.ToName, toAccess.Address);//收件人地址
            return await SendAsync(subject, new BodyBuilder { HtmlBody = content }, new List<MailboxAddress> { address });
        }

        ///<inheritdoc cref="IEmailHelper.SendHtmlToUserAsync(string, string, IEnumerable{ToAccessVm})"/>
        public async Task<bool> SendHtmlToUserAsync(string subject, string content, IEnumerable<ToAccessVm> toAccesses)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentNullException("标题不能为空");
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException("内容不能为空");
            if (!toAccesses.Any())
                throw new ArgumentNullException("接收人不能为空");

            var address = toAccesses.Select(t => new MailboxAddress(t.ToName, t.Address));//收件人地址
            return await SendAsync(subject, new BodyBuilder { HtmlBody = content }, address);
        }

        ///<inheritdoc cref="IEmailHelper.SendToUserAsync(string, BodyBuilder, ToAccessVm)"/>
        public async Task<bool> SendToUserAsync(string subject, BodyBuilder builder, ToAccessVm toAccess)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentNullException("标题不能为空");
            if (toAccess is null)
                throw new ArgumentNullException("接收人不能为空");

            var address = new MailboxAddress(toAccess.ToName, toAccess.Address);//收件人地址
            return await SendAsync(subject, builder, new List<MailboxAddress> { address });
        }

        ///<inheritdoc cref="IEmailHelper.SendToUserAsync(string, BodyBuilder, ToAccessVm)"/>
        public async Task<bool> SendToUsersAsync(string subject, BodyBuilder builder, IEnumerable<ToAccessVm> toAccesses)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentNullException("标题不能为空");
            if (!toAccesses.Any())
                throw new ArgumentNullException("接收人不能为空");

            var address = toAccesses.Select(t => new MailboxAddress(t.ToName, t.Address));//收件人地址
            return await SendAsync(subject, builder, address);
        }

        #region 私有方法

        private async Task<bool> SendAsync(string subject, BodyBuilder bodyBuilder, IEnumerable<MailboxAddress> address)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromAddress));//源邮件地址和发件人
            message.To.AddRange(address);
            message.Subject = subject;
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient
            {
                ServerCertificateValidationCallback = (s, c, h, e) => true
            };
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            try
            {
                await client.ConnectAsync(_emailConfig.Host, _emailConfig.Post, _emailConfig.Ssl);//根据发件人邮箱指定对应SMTP服务器地址  端口   加密
                await client.AuthenticateAsync(_emailConfig.FromAddress, _emailConfig.FromPassword);//用户名   后面是授权码
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"send email fail {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}