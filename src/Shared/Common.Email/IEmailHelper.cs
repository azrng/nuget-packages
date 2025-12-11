using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Email
{
    public interface IEmailHelper
    {
        /// <summary>
        /// 给单个用户发送邮件
        /// </summary>
        /// <param name="content"></param>
        /// <param name="toAccess"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        Task<bool> SendTextToUserAsync(string subject, string content, ToAccessVm toAccess);

        /// <summary>
        /// 给多个用户发送邮件
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="toAccesses"></param>
        /// <returns></returns>
        Task<bool> SendTextToUsersAsync(string subject, string content, IEnumerable<ToAccessVm> toAccesses);

        /// <summary>
        /// 发送html格式邮件
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="toAccess"></param>
        /// <returns></returns>
        Task<bool> SendHtmlToUserAsync(string subject, string content, ToAccessVm toAccess);

        /// <summary>
        /// 发送html格式邮件
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="toAccesses"></param>
        /// <returns></returns>
        Task<bool> SendHtmlToUserAsync(string subject, string content, IEnumerable<ToAccessVm> toAccesses);

        /// <summary>
        ///发送任意格式
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="builder"></param>
        /// <param name="toAccess"></param>
        /// <returns></returns>
        Task<bool> SendToUserAsync(string subject, BodyBuilder builder, ToAccessVm toAccess);

        /// <summary>
        /// 发送任意格式
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="builder"></param>
        /// <param name="toAccesses"></param>
        /// <returns></returns>
        Task<bool> SendToUsersAsync(string subject, BodyBuilder builder, IEnumerable<ToAccessVm> toAccesses);
    }
}