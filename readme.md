# 项目日志

### 数据访问层
  
1. 数据的爬取与基本清理  
   **day-2025-02-25  **
   基本实现了爬虫，进行了简单的数据清洗  
   进行了测试：受无头浏览器爬取影响，爬取1000条数据约半分钟  

   day-2025-02-26  
   对数据访问层进行进一步分层处理

   - DAO           直接进行数据库交互
   - DataAttribute 数据实体的特征修饰，用于反射自动生成相应的表
   - Entity        存放数据实体，每一个数据实体对应一张表
   - service       存放具体业务逻辑的代码

   bug：

   - .net core 版本太低，单元测试框架XUnite无法使用
   - 爬虫在爬取最后一个页面时会崩溃，但是能存储数据

   **day-2025-02-27  **

   链接了SQlite数据库，一个轻量级的数据库  

   通过特征(Attribute)和反射(Reflection)实现了一个简单的ORM框架，通过给实体类加上特定的特征(Attribute)，能够自动生成表。同时给实体类的属性添加属性Attribute，可以根据Attribute确定表中的列的类型和添加主键，外键，默认值等  

   添加了CommonTool层作为公共层为每一层提供自定义工具方法  

   完善了对数据的处理过程，流程如下图  

   ~~~mermaid
   flowchart TB
     A["爬取的数据(嵌套的json格式)"]
     B["正则提取中包含招聘信息的部分(一个List)"]
     C["对List中数据进行处理，选出需要的部分，减少数据量"]
     D["将List映射成某个实体列列表"]
     E["将实体类列表利用上面的ORM框架存入SQlite数据库"]
     A --> B
     B --> C
     C --> D
     D --> E
   ~~~
   bug:  
   - orm框架无法处理循环外键问题  
     
   day-2025-02-28   
   为orm框架添加插入数据功能，能够通过Attribute自动构造插入语句  
   确定了表的主键采用UUID算法生成  
   修复了bug：
   - 爬虫在爬取最后一个页面时会崩溃
   - orm框架无法处理循环外键问题(添加限制不允许添加会导致循环外键Attribute)  
     
    现有bug：  
   - .net core 版本太低，单元测试框架XUnite无法使用
   - 插入语句无法处理Entity中的List类型的成员  

  ** day-2025-03-02**  
   升级了项目的.NET版本，能够使用XUnit进行测试  
   添加了EF core框架，个人写的ORM框架不好用又耗费时间  
   添加了EntityContex层用于使用EF core    
   优化了代码，添加了注释  
   对爬虫功能进行了单元测试，暂无bug    
   已有bug：    
   - 无
     
   修复bug：  
   - .net core 版本太低
   - 插入语句无法处理Entity中的List类型的成员，对于有这List类型的成员的类，映射表时，手动为这个List类型的成员建表，并删除，手动编写代码进行插入
