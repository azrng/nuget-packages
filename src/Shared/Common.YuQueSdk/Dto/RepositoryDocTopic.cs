namespace Common.YuQueSdk.Dto
{
    /// <summary>
    /// 仓库下文档的目录
    /// </summary>
    public class RepositoryDocTopic
    {
        /// <summary>
        /// 节点id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 节点类型 文档(DOC) 目录(TITLE)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 节点唯一 id
        /// </summary>
        public string Uuid { get; set; }

        /// <summary>
        /// 链接或文档 slug
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 上一个节点 uuid
        /// </summary>
        public string Prev_Uuid { get; set; }

        /// <summary>
        /// 下一个节点 uuid
        /// </summary>
        public string Sibling_uuid { get; set; }

        /// <summary>
        /// 第一个子节点 uuid
        /// </summary>
        public string Child_uuid { get; set; }

        /// <summary>
        /// 父亲节点 uuid
        /// </summary>
        public string Parent_uuid { get; set; }

        /// <summary>
        /// 仅文档类型节点，doc id
        /// </summary>
        public string Doc_id { get; set; }

        /// <summary>
        /// 节点层级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 链接是否在新窗口打开，0 在当前页面打开，1 在新窗口打开
        /// </summary>
        public int Open_window { get; set; }

        /// <summary>
        /// 节点是否可见，0 不可见，1 可见
        /// </summary>
        public int Visible { get; set; }

        /// <summary>
        /// 深度
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// 文档节点标识
        /// </summary>
        public string Slug { get; set; }
    }
}