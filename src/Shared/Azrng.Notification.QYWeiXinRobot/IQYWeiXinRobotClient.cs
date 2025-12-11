using Azrng.Notification.QYWeiXinRobot.Model;
using System.Threading.Tasks;

namespace Azrng.Notification.QYWeiXinRobot
{
    /// <summary>
    /// 企业微信机器人通知
    /// </summary>
    public interface IQyWeiXinRobotClient
    {
        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="messageDto">
        /// 文本：<inheritdoc cref="TextMessageDto"/>、
        /// markdown：<inheritdoc cref="MarkdownMessageDto"/>
        /// images:<inheritdoc cref="ImageMessageDto"/>
        /// 图文消息：<inheritdoc cref="NewsMessageDto"/>
        /// </param>
        /// <returns></returns>
        Task<ApiResult> SendMsgAsync<T>(T messageDto) where T : IBaseSendMessageDto;

        /// <summary>
        /// 推送图片消息
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        Task<ApiResult> SendImageMsgAsync(byte[] imageBytes);

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ApiResult<FileUploadResult>> UpdateMediaAsync(UploadMediaRequest request);
    }
}