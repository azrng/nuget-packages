//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace CommonCollect
//{
//    /// <summary>
//    /// Lucene搜索引擎帮助类
//    /// </summary>
//    public class LuceneHelper
//    {
//        /// <summary>
//        /// 私有构造函数
//        /// </summary>
//        private LuceneHelper()
//        {
//        }

//        #region 属性

//        private static LuceneHelper _instance;

//        /// <summary>
//        /// 单一实例
//        /// </summary>
//        public static LuceneHelper Instance => _instance ?? (_instance = new LuceneHelper());

//        private Analyzer _analyzer;

//        /// <summary>
//        /// 分析器
//        /// </summary>
//        private Analyzer PanguAnalyzer => _analyzer ?? (_analyzer = new PanGuAnalyzer());

//        #endregion 属性

//        #region 获取目录

//        /// <summary>
//        /// 获取索引目录
//        /// </summary>
//        /// <param name="index">索引类型</param>
//        /// <returns>索引目录</returns>
//        private LcStore.Directory GetLuceneDirectory(IndexType index)
//        {
//            var indexPath = string.Empty;
//            try
//            {
//                var dirPath = ConfigHelper.GetAppSetting("LuceneIndexPath");

//                var indexName = Enum.EnumHelper.GetEnumDescription(index);

//                indexPath = Path.Combine(dirPath, indexName);

//                return LcStore.FSDirectory.Open(indexPath);
//            }
//            catch (Exception ex)
//            {
//                NLogger.Write($"获取索引目录失败" + Environment.NewLine +
//                              $"路径：{indexPath}" + Environment.NewLine +
//                              $"异常信息：{ex}",
//                             "Lucene", "x", "x",
//                             CustomException.UnknownError, CustomLogLevel.Error);
//                throw new Exception("获取索引目录异常，详情请查看相关日志");
//            }
//        }

//        #endregion 获取目录

//        #region 分词

//        /// <summary>
//        /// 盘古分词
//        /// </summary>
//        /// <param name="keyword">语句</param>
//        /// <returns>词组集合</returns>
//        public string[] GetSplitKeywords(string keyword)
//        {
//            try
//            {
//                string ret = null;
//                var reader = new StringReader(keyword);
//                var ts = PanguAnalyzer.TokenStream(keyword, reader);
//                var hasNext = ts.IncrementToken();
//                Lucene.Net.Analysis.Tokenattributes.ITermAttribute ita;
//                while (hasNext)
//                {
//                    ita = ts.GetAttribute<Lucene.Net.Analysis.Tokenattributes.ITermAttribute>();
//                    ret += ita.Term + "|";
//                    hasNext = ts.IncrementToken();
//                }
//                ts.CloneAttributes();
//                reader.Close();
//                PanguAnalyzer.Close();

//                if (string.IsNullOrWhiteSpace(ret)) return null;

//                ret = ret.Substring(0, ret.Length - 1);
//                return ret.Split('|');
//            }
//            catch (Exception ex)
//            {
//                NLogger.Write("分词异常" + Environment.NewLine +
//                              $"关键词：{keyword}" + Environment.NewLine +
//                              $"异常信息：{ex}",
//                             "Lucene", "x", "x",
//                             CustomException.UnknownError, CustomLogLevel.Error);
//                throw new Exception("分词出现异常，详情请查看相关日志");
//            }
//        }

//        #endregion 分词

//        #region 索引增删改查

//        /// <summary>
//        /// 创建索引或追加索引
//        /// </summary>
//        /// <param name="dataList">数据集合</param>
//        /// <param name="index">索引类型</param>
//        public void CreateOrAppendIndexes(List<Document> dataList, IndexType index)
//        {
//            if (dataList == null || dataList.Count == 0)
//                return;

//            IndexWriter writer;
//            var directory = GetLuceneDirectory(index);
//            try
//            {
//                //false表示追加（true表示删除之前的重新写入）
//                writer = new IndexWriter(directory, PanguAnalyzer, false, IndexWriter.MaxFieldLength.LIMITED);
//            }
//            catch
//            {
//                //false表示追加（true表示删除之前的重新写入）
//                writer = new IndexWriter(directory, PanguAnalyzer, true, IndexWriter.MaxFieldLength.LIMITED);
//            }
//            writer.MergeFactor = 1000;
//            //writer.SetMaxBufferedDocs(1000);
//            foreach (var doc in dataList)
//            {
//                writer.AddDocument(doc);
//            }
//            writer.Optimize();

//            writer.Dispose();
//            directory.Dispose();
//        }

//        /// <summary>
//        /// 删除索引
//        /// </summary>
//        /// <param name="field">字段名</param>
//        /// <param name="value">字段值</param>
//        /// <param name="index">索引类型</param>
//        public void DeleteIndexes(string field, string value, IndexType index)
//        {
//            IndexWriter writer = null;
//            var directory = GetLuceneDirectory(index);
//            try
//            {
//                writer = new IndexWriter(directory, PanguAnalyzer, false, IndexWriter.MaxFieldLength.LIMITED);
//                var term = new Term(field, value);
//                writer.DeleteDocuments(term);
//                //var isSuccess = writer.HasDeletions();
//                writer.Optimize();
//            }
//            catch (Exception ex)
//            {
//                NLogger.Write("删除索引异常" + Environment.NewLine +
//                              $"异常信息：{ex}", "Lucene", "x", "x",
//                             CustomException.UnknownError, CustomLogLevel.Error);
//                throw new Exception("删除索引异常，详情请查看相关日志");
//            }
//            finally
//            {
//                writer?.Dispose();
//                directory?.Dispose();
//            }
//        }

//        /// <summary>
//        /// 更新索引；这里实际上是先删除原有索引，在创建新索引。
//        /// 所以在更新索引时，一定要确保传入的Document的所有字段都有值
//        /// 否则将会被置为空
//        /// </summary>
//        /// <param name="field">字段名</param>
//        /// <param name="value">字段值</param>
//        /// <param name="doc">文档</param>
//        /// <param name="index">索引类型</param>
//        public void UpdateIndexes(string field, string value, Document doc, IndexType index)
//        {
//            IndexWriter writer = null;
//            var directory = GetLuceneDirectory(index);
//            try
//            {
//                writer = new IndexWriter(directory, PanguAnalyzer, false, IndexWriter.MaxFieldLength.LIMITED);
//                var term = new Term(field, value);
//                writer.UpdateDocument(term, doc);
//            }
//            catch (Exception ex)
//            {
//                NLogger.Write("更新索引异常" + Environment.NewLine +
//                              $"异常信息：{ex}", "Lucene", "x", "x",
//                             CustomException.UnknownError, CustomLogLevel.Error);
//                throw new Exception("更新索引异常，详情请查看相关日志");
//            }
//            finally
//            {
//                writer?.Dispose();
//                directory?.Dispose();
//            }
//        }

//        #endregion 索引增删改查

//        #region 查询

//        /// <summary>
//        /// 查询
//        /// </summary>
//        /// <typeparam name="T">实体类型</typeparam>
//        /// <param name="fields">条件字段</param>
//        /// <param name="keywords">关键词组</param>
//        /// <param name="index">索引类型</param>
//        /// <param name="sort">排序，可为空</param>
//        /// <param name="count">读取数量</param>
//        /// <returns>结果集</returns>
//        public List<T> Search<T>
//            (
//            string[] fields,
//            string[] keywords,
//            IndexType index,
//            Sort sort,
//            int count
//            ) where T : new()
//        {
//            if (fields == null || fields.Length == 0)
//                return null;
//            if (keywords == null || keywords.Length == 0)
//                return null;

//            //索引目录
//            var directory = GetLuceneDirectory(index);

//            //查询条件
//            var boolQuery = GetQuery(fields, keywords);

//            //索引查询器
//            var searcher = new IndexSearcher(directory, true);

//            TopDocs docs;
//            if (sort != null)
//                docs = searcher.Search(boolQuery, null, count, sort);
//            else
//                docs = searcher.Search(boolQuery, count);
//            if (docs == null || docs.TotalHits == 0)
//                return null;

//            //文档集合
//            var docList = docs.ScoreDocs.Select(sd => searcher.Doc(sd.Doc)).ToList();

//            //反射赋值
//            var list = ConvertDocToObj<T>(docList);

//            searcher.Dispose();
//            directory.Dispose();

//            return list;
//        }

//        /// <summary>
//        /// 查询分页数据（指定排序方式）
//        /// </summary>
//        /// <typeparam name="T">实体类型</typeparam>
//        /// <param name="fields">条件字段</param>
//        /// <param name="keywords">关键词组</param>
//        /// <param name="index">索引类型</param>
//        /// <param name="sort">排序，必填</param>
//        /// <param name="pageNumber">页码</param>
//        /// <param name="pageSize">页数</param>
//        /// <returns>结果集</returns>
//        public PagedResult<List<T>> SearchByPaged<T>
//            (
//            string[] fields,
//            string[] keywords,
//            IndexType index,
//            Sort sort,
//            int pageNumber = 1,
//            int pageSize = 20
//            ) where T : new()
//        {
//            if (fields == null || fields.Length == 0)
//                return null;
//            if (keywords == null || keywords.Length == 0)
//                return null;

//            //索引目录
//            var directory = GetLuceneDirectory(index);

//            //查询条件
//            var boolQuery = GetQuery(fields, keywords);

//            var collector = TopFieldCollector
//                .Create(sort, pageNumber * pageSize, false, false, false, false);

//            var searcher = new IndexSearcher(directory, true);

//            searcher.Search(boolQuery, collector);

//            if (collector == null || collector.TotalHits == 0)
//                return null;

//            //分页
//            var start = (pageNumber - 1) * pageSize;
//            var limit = pageSize;
//            var hits = collector.TopDocs(start, limit).ScoreDocs;
//            var totalCount = collector.TotalHits;

//            var docList = hits.Select(sd => searcher.Doc(sd.Doc)).ToList();

//            //反射赋值
//            var list = ConvertDocToObj<T>(docList);

//            searcher.Dispose();
//            directory.Dispose();

//            return new PagedResult<List<T>>
//            {
//                Total = totalCount,
//                Result = list
//            };
//        }

//        /// <summary>
//        /// 查询分页数据（默认排序方式）
//        /// </summary>
//        /// <typeparam name="T">实体类型</typeparam>
//        /// <param name="fields">条件字段</param>
//        /// <param name="keywords">关键词组</param>
//        /// <param name="index">索引类型</param>
//        /// <param name="pageNumber">页码</param>
//        /// <param name="pageSize">页数</param>
//        /// <returns>结果集</returns>
//        public PagedResult<List<T>> SearchByPaged<T>
//            (
//            string[] fields,
//            string[] keywords,
//            IndexType index,
//            int pageNumber = 1,
//            int pageSize = 20
//            ) where T : new()
//        {
//            if (fields == null || fields.Length == 0)
//                return null;
//            if (keywords == null || keywords.Length == 0)
//                return null;

//            //索引目录
//            var directory = GetLuceneDirectory(index);

//            //查询条件
//            var boolQuery = GetQuery(fields, keywords);

//            var collector = TopScoreDocCollector.Create(pageNumber * pageSize, false);
//            var searcher = new IndexSearcher(directory, true);

//            searcher.Search(boolQuery, collector);

//            if (collector == null || collector.TotalHits == 0)
//                return null;

//            //分页
//            var start = (pageNumber - 1) * pageSize;
//            var limit = pageSize;
//            var hits = collector.TopDocs(start, limit).ScoreDocs;
//            var totalCount = collector.TotalHits;

//            var docList = hits.Select(sd => searcher.Doc(sd.Doc)).ToList();

//            //反射赋值
//            var list = ConvertDocToObj<T>(docList);

//            searcher.Dispose();
//            directory.Dispose();

//            return new PagedResult<List<T>>
//            {
//                Total = totalCount,
//                Result = list
//            };
//        }

//        /// <summary>
//        /// 查询分页数据（默认排序方式）
//        /// </summary>
//        /// <param name="fields">条件字段</param>
//        /// <param name="keywords">关键词组</param>
//        /// <param name="index">索引类型</param>
//        /// <returns>结果集</returns>
//        public int GetTotla(string[] fields, string[] keywords, IndexType index)
//        {
//            if (fields == null || fields.Length == 0)
//                return 0;
//            if (keywords == null || keywords.Length == 0)
//                return 0;

//            //索引目录
//            var directory = GetLuceneDirectory(index);

//            //查询条件
//            var boolQuery = GetQuery(fields, keywords);

//            var collector = TopScoreDocCollector.Create(20, false);
//            var searcher = new IndexSearcher(directory, true);

//            searcher.Search(boolQuery, collector);

//            if (collector == null || collector.TotalHits == 0)
//                return 0;

//            searcher.Dispose();
//            directory.Dispose();

//            return collector.TotalHits;
//        }

//        /// <summary>
//        /// 文档转换为对象
//        /// </summary>
//        /// <typeparam name="T">实体类型</typeparam>
//        /// <param name="docList">文档集合</param>
//        /// <returns>对象集合</returns>
//        private List<T> ConvertDocToObj<T>(List<Document> docList) where T : new()
//        {
//            var type = typeof(T);
//            var propertyList = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

//            var list = new List<T>();
//            var firstDoc = docList.First();
//            var fieldNames = firstDoc.GetFields().Select(x => x.Name).ToList();

//            foreach (var doc in docList)
//            {
//                var tObj = new T();
//                foreach (var pInfo in propertyList)
//                {
//                    var name = pInfo.Name;
//                    if (fieldNames.Any(x => x.ToLower() == name.ToLower()))
//                    {
//                        SetValue<T>(pInfo, tObj, doc, name);
//                    }
//                }

//                list.Add(tObj);
//            }
//            return list;
//        }

//        /// <summary>
//        /// 获取查询条件
//        /// </summary>
//        /// <param name="fields">条件字段</param>
//        /// <param name="keywords">关键词组</param>
//        /// <returns></returns>
//        private BooleanQuery GetQuery(string[] fields, string[] keywords)
//        {
//            var boolQuery = new BooleanQuery();
//            foreach (var field in fields)
//            {
//                foreach (var keyword in keywords)
//                {
//                    var t = new TermQuery(new Term(field, keyword));
//                    boolQuery.Add(t, Occur.SHOULD);
//                }
//            }
//            return boolQuery;
//        }

//        #endregion 查询

//        private void SetValue<T>(PropertyInfo pInfo, T tObj, Document doc, string name)
//        {
//            var pType = pInfo.PropertyType.Name;
//            switch (pType)
//            {
//                case "String":
//                    pInfo.SetValue(tObj, doc.Get(name), null);
//                    break;

//                case "Int32":
//                    pInfo.SetValue(tObj, GetInt(doc.Get(name)), null);
//                    break;

//                case "Boolean":
//                    pInfo.SetValue(tObj, GetBool(doc.Get(name)), null);
//                    break;

//                case "DateTime":
//                    pInfo.SetValue(tObj, GetDate(doc.Get(name)), null);
//                    break;

//                case "Double":
//                    pInfo.SetValue(tObj, GetDouble(doc.Get(name)), null);
//                    break;

//                case "Single":
//                    pInfo.SetValue(tObj, GetFloat(doc.Get(name)), null);
//                    break;

//                case "Decimal":
//                    pInfo.SetValue(tObj, GetDecimal(doc.Get(name)), null);
//                    break;
//            }
//        }

//        private int GetInt(string value)
//        {
//            var result = 0;
//            int.TryParse(value, out result);
//            return result;
//        }

//        private DateTime GetDate(string value)
//        {
//            DateTime result;
//            DateTime.TryParse(value, out result);
//            return result;
//        }

//        private bool GetBool(string value)
//        {
//            bool result;
//            bool.TryParse(value, out result);
//            return result;
//        }

//        private double GetDouble(string value)
//        {
//            double result;
//            double.TryParse(value, out result);
//            return result;
//        }

//        private float GetFloat(string value)
//        {
//            float result;
//            float.TryParse(value, out result);
//            return result;
//        }

//        private decimal GetDecimal(string value)
//        {
//            decimal result;
//            decimal.TryParse(value, out result);
//            return result;
//        }
//    }
//}
