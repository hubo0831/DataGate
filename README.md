DataGate.Core --asp.net core api 应用程序
1. 安装数据库：提供了oracle脚本，在DataGate.Core/DataGate.Api/db/oralce下。
2. 修改DataGate.Core/DatagGate.Api/appsettings.json中的Default连接串改为自己的数据库连接串。
3. 在visual studio中启动DataGate.Api.

DataGate.Web -- Vue+Vue-Router+ElementUI的后台管理的前端框架，配合DataGate.Core的API.

第一步：运行，

vue init webpack

npm install datagate.web

第二步： 修改入口文件main.js:删除原有代码，只复制粘贴以下两行
import {Run} from "datagate.web"
Run();
 
然后
npm intall
npm run dev

一个简单的，前后端完全分离式的管理系统类一体化前后端解决方案。

因本系统后端利用三个公共的API（api/dg, api/dg/s, api/dg/m)访问所有的数据，统一数据存取的出入口，所以叫"DataGate". 服务端支持Oracle库,SqlServer和Mysql. 未来还会支持MongoDB.

所以无论是前端js编程还是后端编程都变得极其简单。

系统主要特点：

后端（服务端）：

非EF,非任何已知的笨重的ORM框架，纯ADO.net的数据访问, 自动根据配置自动生成sql代码并查询，并且兼容主流数据库系统，支持多种复杂查询，比如父子表，多表联接等。绝大多数情况不用手写sql;
出现新的数据访问需求，完全不需要动代码，只需要在配置文件加一个配置项就行了；
数据库名称和返回数据属性名的自动映射，解决难看的字段名问题;
配置文件的热更新机制，可以使调试后端配置文件就如同调试前端脚本一样容易；
自带完善的单元测试框架，可以很容易进行单元测试。
前端:

完全基于Vue+Vue-router+ElementUI+少量Jquery的js框架，生成SPA应用程序，用户体验一流，响应超快;
内置了用户身份验证，用户信息修改、密码修改和找回;
内置常用的用户管理，角色管理，功能权限管理和字典管理这些常用模块;
内置了根据服务端元数据自动生成的数据表格，并且支持行内编辑、弹出编辑等常用的管理界面逻辑;
整合了百度的webuploader文件上传，可以灵活上传一个或多个文件，带进度条等状态监控，并具备秒传、断点续传和出错重传功能；
内置了常用的搜索式的表单，自动生成搜索界面和相关逻辑. 所有这些都不需要额外的重复代码，都根据后端的元数据描述配置自动生成。
它不仅是一个框架，而是一个可以直接拿来跑的半成品系统。你只需要加入自己的业务的页面即可。 当你用vue-cli搭建了一个脚手架，只需要两步，即可以集成datagate.web的前端框架： 第一步：

npm install datagate.web

第二步： 修改入口文件main.js:删除原有代码，只复制粘贴以下两行

import {Run} from "datagate.web"

Run();

非常简单！
