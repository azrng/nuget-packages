using System;
using System.Collections.Generic;
using System.Text;

namespace CommonCollect.UploadFile
{
    /// <summary>
    /// 返回实体
    /// </summary>
    public class MessageEntity
    {
        private int _code = 0;
        private string _msg = string.Empty;
        private object _data = new object();

        /// <summary>
        /// 状态标识
        /// </summary>
        public int Code { get => _code; set => _code = value; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Msg { get => _msg; set => _msg = value; }
        /// <summary>
        /// 返回数据
        /// </summary>
        public object Data { get => _data; set => _data = value; }
    }
}
