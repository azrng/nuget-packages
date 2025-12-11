//using Common.EventNotification.Attributes;
//using Common.EventNotification.Interface;
//using Microsoft.AspNetCore.Mvc;
//using System;

//namespace ChannelsSample.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PulisherAndSubscriberController : ControllerBase
//    {
//        private readonly IEventPublisher _channelPublisher;

//        public PulisherAndSubscriberController(IEventPublisher channelPublisher)
//        {
//            _channelPublisher = channelPublisher;
//        }

//        [HttpGet]
//        public string Pullisher()
//        {
//            _channelPublisher.Send("test", new UserInfo1(Guid.NewGuid().ToString(), "张三"));
//            return "成功";
//        }

//        // 两个订阅者 分别在不同的组

//        [NonAction]
//        [EventSubscriber("test", "group1")]
//        public void GetInfo(UserInfo1 user)
//        {
//            Console.WriteLine(user.Id + " 1111 " + user.Name);
//        }

//        //[NonAction]
//        //[ChannelSubscriber("test", "group2")]
//        //public void GetInfo2(UserInfo1 user)
//        //{
//        //    Console.WriteLine(user.Id + " 222 " + user.Name);
//        //}
//    }

//    /// <summary>
//    /// 测试用户信息表
//    /// </summary>
//    public class UserInfo1
//    {
//        public UserInfo1()
//        {
//        }

//        public UserInfo1(string id, string name)
//        {
//            Id = id;
//            Name = name;
//        }

//        public string Id { get; set; }

//        public string Name { get; set; }
//    }
//}