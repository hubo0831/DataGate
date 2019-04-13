### 0.3.3
1. 优化EditTask的增删改状态，无论对象是添加或修改的都可以调用add或change方法
2. EditTask增加total属性，移除TaskMixin中的total。
3. 优化EditGrid的列排序，当数据只有一页时本地排序。
4. 增加Metadata中的col属性以绑定el-col中的span, 以定制Form列宽。
5. 移除EditTask.changeStatus, 增加add, change remove三个独立的方法。

### 0.3.2 - 2019-04-07
1. 增加deferred/blob操作等方法
2. editGrid/editTask行内编辑优化

### 0.3.0 - 0.3.1 2019-03-06-2019-03-28
1. 补充数据验证
2. EditForm增加数字输入type=number
3. fileupload优化
4. edit-grid select事件精简

### v0.2.9 - 0.2.9.1 2019-03-03 ~ 03-05
1. inEdit作为属性名时只能写成:in-edit='true/false'
2. editGrid增加sort-change事件

### v0.2.8 2019-03-03
1. 增加formEdit自定义输入控件的inEdit=true
2. UserProfie增加电话号码修改。

### v0.2.7 2019-02-26
1.增加UserAPI.loginUrl()

### v0.2.6 2019-02-19
1. 增加服务端的匿名登录用户

### v0.2.4 - v0.2.5
1. 配合服务端调整FileUpload
2. 基类的userProfile重命名为userState

### v0.2.3
1. formatDate增加毫秒
2. export + UserAPI in index and index.base

### v0.2.1 - v 0.2.2 2019-1-23
1. 升级vue-template-compiler 2.5.22
2. 去掉throw jquery.ajax不是promise
3. $('.dg-scr').slimscroll

### V0.2.0 2019-1-2 - 2019-1-22
1. 增加日志
2. 增加API.NONQUERY接口以执行非查询命令
3. 增加API.EXPORT接口以导出Excel
4. Util.download(file{url, name})下载文件API
5. Vue升到2.5.22 vue-router升到3.0.2

### V0.1.13 2018-12-22 - 12-30
1. 服务端自动判断外键的排序位
2. EditGrid和SearchForm增加和模型对接的自定义组件定义
3. 配合服务端增加meta的column属性以批量定义列显示规则
4. 进入带search-Form的页面时从地址栏恢复searchForm中的值
5. 增加：taskpagemixin.apiDataFilter(key, data)ajax请求后数据预处理
6. 增加前时后退时保留上一次滚动位置，使用window.popstate事件。
7. 删除测试用的system账号，增加记住我功能
8. 增加pubmixin中的fitHeight(selector)方法，自动调整editGrid的高度到页面底部
9. 增加dg-fit和dg-scr伪样式名用于自动调整高度和自动生成滚动条

### V0.1.12 2018-12-17 - 
1. Fileupload=>FileUpload
2. tree-select=>hidden样式
3. webuploader样式集成
4. 增加align对齐属性

### V0.1.9 2018-12-10
1. 菜单menu加上route属性以定义路由信息，和url区别，url是实际的菜单导航路径。route是路由模板定义。
   该方法不是最佳，但找不到更好的办法。
2. 增加appConfig.titleHtml以自定义系统标题显示样式
3. 增加基类中的userProfile用户信息，用this.userProfile.currentUser获取信息
4. 改善用户信息的获取，在logout再login以后能在返回的子页面或组件中正确获取用户信息
5. 增加bus事件register用以注册组件，因Vue.components()方法在项目中无效只在框架中有效，原因不明。

### V0.1.8 2018-11-22
1. EditGrid.showIndex默认为true
2. 增加appConfig.elSize取值1，2，3代表控件大小, 1=mini 2=small 3=medium
3. 在metadata中加入任意自定义属性？配合table-column中的属性定义


###v0.1.7  2018-11-13
1. 菜单权限判定
2. +unauth和notfound页面

###v0.1.6  2018-11-12
1. 将login和app的css样式分离到单独的文件以便于项目自由改写。
2. 将pubmixin和taskmixin导出
3. 增加appConfig的Vue全局属性appConfig.logo systemName copyright

###v0.1.3
1.防止刷新时登录框一闪而过不好看。
2.混用require和import导致的页面不加载问题。
3.将页面图标等个性化信息组件化为site-title和login-bottom以供应用自己改写