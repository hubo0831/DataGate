#要将node_modules排除在svn之外，否则svn非常慢
#此包是配置服务端DataGate.Api一起使用，请在nuget上安装DataGate.Api以搭建服务器环境。它是一个.netcore 2.1的API站点。

#用vue-cli来建立一个vue项目：
vue init webpack

#安装环境：
npm install

#安装datagate.web
npm install datagate.web

#修改main.js, 用下面的代码替换原有的：

import {Run} from "datagate.web"
Run();

#运行调试：
npm run dev

#打包生成：
npm run build 

#如果运行出错，报以下错：
These dependencies were not found:

* !!vue-style-loader!css-loader?{"sourceMap":true}!../../vue-loader/lib/style-compiler/index?{ ............

#则运行
npm install sass-loader --save
npm install node-sass --save