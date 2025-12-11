using Azrng.Notification.QYWeiXinRobot;
using Azrng.Notification.QYWeiXinRobot.Model;

namespace Common.Notification.QYWeiXinRobot.Tests;

public class QyWeiXinRobotClientTest
{
    private readonly IQyWeiXinRobotClient _robotClient;
    private readonly HttpClient _httpClient;
    private readonly string _picUrl;

    public QyWeiXinRobotClientTest(IQyWeiXinRobotClient client,
        IHttpClientFactory httpClientFactory)
    {
        _robotClient = client;
        _httpClient = httpClientFactory.CreateClient();
        _picUrl = "http://res.mail.qq.com/node/ww/wwopenmng/images/independent/doc/test_pic_msg1.png";
    }

    /// <summary>
    /// 文本消息
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendTextMessage_ReturnOk()
    {
        var msg = new TextMessageDto
        {
            Text = new SendMentionedUser
            {
                Content = "文本消息测试",
            }
        };
        var result = await _robotClient.SendMsgAsync(msg);
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// md消息
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendMarkdownMessage_ReturnOk()
    {
        var msg = new MarkdownMessageDto()
        {
            Text = new SendMentionedUser
            {
                Content =
                    "实时新增用户反馈<font color=\"warning\">132例</font>，请相关同事注意。类型:<font color=\"comment\">用户反馈</font>>" +
                    "普通用户反馈:<font color=\"comment\">117例</font>>VIP用户反馈:<font color=\"comment\">15例</font>",
            }
        };
        var result = await _robotClient.SendMsgAsync(msg);
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// 图片消息
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendImageMessage_ReturnOk()
    {
        var bytes = await _httpClient.GetByteArrayAsync(_picUrl);
        var base64 = Helper.BytesToBase64(bytes);
        var md5 = Helper.GetFileMd5Hash(bytes);
        var msg = new ImageMessageDto()
        {
            Text = new ImageContentDto
            {
                Base64 = base64,
                Md5 = md5
            }
        };
        var result = await _robotClient.SendMsgAsync(msg);
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// 图片消息
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendBytesImageMessage_ReturnOk()
    {
        var bytes = await _httpClient.GetByteArrayAsync(_picUrl);

        var result = await _robotClient.SendImageMsgAsync(bytes);
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// 图文消息
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendNewsMessage_ReturnOk()
    {
        var msg = new NewsMessageDto
        {
            News = new NewsMessageDto.NewsContent
            {
                Articles = new List<ArticlesContentDto>
                {
                    new ArticlesContentDto
                    {
                        Title = "标题",
                        Description = "我是备注",
                        PicUrl = _picUrl,
                        Url = "www.qq.com",
                    }
                }
            }
        };
        var result = await _robotClient.SendMsgAsync(msg);
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// 上传图片
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UploadImage_ReturnOk()
    {
        var bytes = await _httpClient.GetByteArrayAsync(_picUrl);

        var fileRequest = new UploadMediaRequest
        {
            ContentType = "image/png",
            FileName = "111.png",
            Media = bytes,
        };
        var result = await _robotClient.UpdateMediaAsync(fileRequest);
        Assert.True(result.IsSuccess);

        var msg = new FileMessageDto
        {
            File = new FileMessageDto.FileContent
            {
                MediaId = result.Data.MediaId
            }
        };
        var sendResult = await _robotClient.SendMsgAsync(msg);
        Assert.True(sendResult.IsSuccess);
    }

    /// <summary>
    /// 上传文本
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UploadTxt_ReturnOk()
    {
        var filePath = "222.txt";
        if (!File.Exists(filePath))
            await File.WriteAllTextAsync(filePath, "123456");

        var bytes = await File.ReadAllBytesAsync(filePath);

        var fileRequest = new UploadMediaRequest
        {
            ContentType = "text/plain",
            FileName = filePath,
            Media = bytes,
        };
        var result = await _robotClient.UpdateMediaAsync(fileRequest);
        Assert.True(result.IsSuccess);

        var msg = new FileMessageDto
        {
            File = new FileMessageDto.FileContent
            {
                MediaId = result.Data.MediaId
            }
        };
        var sendResult = await _robotClient.SendMsgAsync(msg);
        Assert.True(sendResult.IsSuccess);

        File.Delete(filePath);
    }

    /// <summary>
    /// 上传文本通知模板卡片
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendTextNoticeTemplateCard_ReturnOk()
    {
        var msg = new TemplateCardMessageDto
        {
            TemplateCard = new TextNoticeTemplateCard
            {
                Source = new CardSourceStyleDto
                {
                    IconUrl = "https://wework.qpic.cn/wwpic/252813_jOfDHtcISzuodLa_1629280209/0",
                    Desc = "森亿智能",
                    DescColor = 1
                },
                MainTitle = new TitleContentDto
                {
                    Title = "森亿提醒",
                    Desc = "您的好友正在邀请您加入森亿智能"
                },
                Emphasiscontent = new TitleContentDto
                {
                    Title = "9999",
                    Desc = "长长久久"
                },
                QuoteArea = new QuoteAreaStyleDto
                {
                    Type = 1,
                    Url = "https://work.weixin.qq.com/?from=openApi",
                    Title = "引用文本标题",
                    QuoteText = "引用文献引用文案"
                },
                SubTitleText = "加入我们把！",
                HorizontalContentList = new List<HorizontalContentDto>
                {
                    new HorizontalContentDto
                    {
                        Type = 1,
                        KeyName = "官网",
                        Value = "点击访问",
                        Url = "https://work.weixin.qq.com/?from=openApi",
                    }
                },
                JumpList = new List<JumpStyleDto>
                {
                    new JumpStyleDto
                    {
                        Type = 1,
                        Url = "https://work.weixin.qq.com/?from=openApi",
                        Title = "官网"
                    }
                },
                CardAction = new CardActionDto
                {
                    Type = 1,
                    Url = "https://work.weixin.qq.com/?from=openApi"
                }
            }
        };
        var result = await _robotClient.SendMsgAsync(msg);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendNewsNoticeTemplateCard_ReturnOk()
    {
        var msg = new TemplateCardMessageDto
        {
            TemplateCard = new NewsNoticeTemplateCard
            {
                Source = new CardSourceStyleDto
                {
                    IconUrl = "https://wework.qpic.cn/wwpic/252813_jOfDHtcISzuodLa_1629280209/0",
                    Desc = "企业微信",
                    DescColor = 0,
                },
                MainTitle = new TitleContentDto
                {
                    Title = "欢迎使用企业微信",
                    Desc = "您的好友邀请您加入"
                },
                CardImage = new CardImageStyleDto
                {
                    AspectRatio = 2.25f,
                    Url = "https://wework.qpic.cn/wwpic/354393_4zpkKXd7SrGMvfg_1629280616/0",
                },
                ImageTextArea = new ImageTextAreaStyleDto
                {
                    Type = 1,
                    Url = "https://work.weixin.qq.com",
                    Title = "欢迎使用",
                    Desc = "您的好友邀请你加入",
                    ImageUrl = "https://wework.qpic.cn/wwpic/354393_4zpkKXd7SrGMvfg_1629280616/0"
                },
                QuoteArea = new QuoteAreaStyleDto
                {
                    Title = "引用文本标题",
                    Url = "https://work.weixin.qq.com/?from=openApi",
                    Type = 1,
                    QuoteText = "Jack：企业微信真的很好用~\nBalian：超级好的一款软件！"
                },
                VerticalContentList = new List<TitleContentDto>
                {
                    new TitleContentDto
                    {
                        Title = "惊喜红包等你拿",
                        Desc = "下载企业微信领红包"
                    },
                },
                HorizontalContentList = new List<HorizontalContentDto>
                {
                    new HorizontalContentDto
                    {
                        KeyName = "邀请人",
                        Value = "张三"
                    }
                },
                JumpList = new List<JumpStyleDto>
                {
                    new JumpStyleDto
                    {
                        Type = 1,
                        Url = "https://work.weixin.qq.com/?from=openApi",
                        Title = "官网"
                    }
                },
                CardAction = new CardActionDto
                {
                    Type = 1,
                    Url = "https://work.weixin.qq.com/?from=openApi"
                }
            }
        };
        var result = await _robotClient.SendMsgAsync(msg);
        Assert.True(result.IsSuccess);
    }
}