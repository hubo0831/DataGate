## v0.1.7 2019-02-01 -
1. 文件上传的服务
2. Models.json文件增加 name+的字段组合；
3. 调整NLog日志配置增加缓冲
4. 计时方式调整，在NLog增加缓冲以后很奇怪的是算出的action执行时间都相同，没有深究，换成直接在BaseController中用Stopwatch记时
5. 在*Keys.json配置文件中，OpType=GetPage时也可以配置直接执行的Sql语句，此时的Model只起到给前端Table定义列的作用
6. 增加OpType=NonQuery选项，直接执行非查询语句，返回影响的行数， 路由地址规定为 /dg/n/{key}
7. 修复因为增加Key的正则过滤器而产生的用户管理页面错误。
8. 增加MetaQuery同时返回元数据和数据。格式是{meta, data}
9. 增强多表查询和保存，支持多级的子表。

## v0.1.6 2018-12-22
1. 在FieldMeta中增加Column属性定义多个显示规则以免外面定义的属性过多
2. 增加登录框中记住我的功能

## v0.1.5 1217
1. 提供Mock数据支持; NewtonSoft默认不序列化空值和等于类型默认值的属性（跳过空值或0,但长度0的字符串''还会序列化）
2. DBHelper的autofac自动注入
3. MetaService.CreateDBHelper
4. MetaService.RegisterDBHelper
      
## v0.1.4 
1. 菜单权限控制