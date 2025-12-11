## Common.Db.Core

一个数据库操作的核心库，包含一些公用类等

### 公共类

* GetPageRequest：分页请求
* GetPageSortRequest：分页排序请求
* GetQueryPageResult：分页返回类

### 版本更新记录

* 1.2.0
  * 更新文件命名空间
* 1.1.0
  * 增加分页请求类初始化
* 1.0.0
  * 增加包文档
* 0.1.0
    * 命名空间迁移到Common.Core
    * 增加表达式树帮助类Expressionable
    * 增加RefAsync用于分页查询

* 0.0.3
    * 增加NotNull静态分析
    * 移除过期的方法
    * 更新dbcore类库MarkEqual方法为扩展方法
* 0.0.2
    * 迁移Common.EfCore的类到DBCore中
* 0.0.1
    * 请求类和响应类处理