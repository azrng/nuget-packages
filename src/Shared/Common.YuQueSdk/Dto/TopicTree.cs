using Common.YuQueSdk.Enums;
using System.Collections.Generic;

namespace Common.YuQueSdk.Dto
{
    /// <summary>
    /// 目录树形结构的输出对象
    /// </summary>
    public class TopicTree
    {
        public TopicTree(string type, string title, string uuid, string parent_uuid, int level, string slug)
        {
            Type = type;
            Title = title;
            Uuid = uuid;
            Parent_uuid = parent_uuid;
            Level = level;
            Slug = slug;
            DocType = DocTypeEnum.Doc;
            Child = new List<TopicTree>();
        }

        /// <summary>
        /// 文档类型
        /// </summary>
        public DocTypeEnum DocType { get; private set; }

        /// <summary>
        /// 节点类型 文档(DOC) 目录(TITLE)
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 节点唯一 id
        /// </summary>
        public string Uuid { get; private set; }

        /// <summary>
        /// 父亲节点 uuid
        /// </summary>
        public string Parent_uuid { get; private set; }

        /// <summary>
        /// 节点层级
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// 文档节点标识
        /// </summary>
        public string Slug { get; private set; }

        /// <summary>
        /// 树结构的子集
        /// </summary>
        public List<TopicTree> Child { get; private set; }

        /// <summary>
        /// 设置子节点
        /// </summary>
        /// <param name="topicTrees"></param>
        public void SetChild(List<TopicTree> topicTrees)
        {
            Child = topicTrees;
            if (topicTrees.Count > 0)
            {
                DocType = DocTypeEnum.Menu;
                if (Type.Equals("DOC", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    DocType = DocTypeEnum.DocAndMenu;
                }
            }
        }
    }
}