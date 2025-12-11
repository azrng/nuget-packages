# 说明
## 操作说明
```c#
startup类中使用：
services.AddEmail(info =>
{
    info.Host = "smtp.163.com";
    info.Post = 587;
    info.FromName = "发送者用户名";
    info.FromAddress = "发送者地址";
    info.FromPassword = "发送者密码(授权码)";
});
然后注入：IEmailHelper
```
#操作事例：
* 如果是传递文本文件直接传递就行
* 如果发送的内容是html，那么调用发送html的那个方法
* 如果html的图片是通过本地上传的，那么需要使用：
    ```c#
    //带图片
    var path = "D:\\bg.jpg";
    var builder = new BodyBuilder();
    var image = builder.LinkedResources.Add(path);
    image.ContentId = MimeUtils.GenerateMessageId();
    builder.HtmlBody = $"当前时间:{DateTime.Now:yyyy-MM-dd HH:mm:ss} <img src=\"cid:{image.ContentId}\"/>";
    ```
* 还没有封装包含附件的那种方式邮件
