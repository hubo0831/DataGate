V0.1.9 2018-12-10
1. 菜单menu加上route属性以定义路由信息，和url区别，url是实际的菜单导航路径。route是路由模板定义。
   该方法不是最佳，但找不到更好的办法。
2. 增加appConfig.titleHtml以自定义系统标题显示样式
3. 增加基类中的userProfile用户信息，用this.userProfile.currentUser获取信息
4. 改善用户信息的获取，在logout再login以后能在返回的子页面或组件中正确获取用户信息
5. 增加bus事件register用以注册组件，因Vue.components()方法在项目中无效只在框架中有效，原因不明。

V0.1.8 2018-11-22
1. EditGrid.showIndex默认为true
2. 增加appConfig.elSize取值1，2，3代表控件大小, 1=mini 2=small 3=medium
3. 在metadata中加入任意自定义属性？配合table-column中的属性定义


v0.1.7  2018-11-13
1. 菜单权限判定
2. +unauth和notfound页面

v0.1.6  2018-11-12
1. 将login和app的css样式分离到单独的文件以便于项目自由改写。
2. 将pubmixin和taskmixin导出
3. 增加appConfig的Vue全局属性appConfig.logo systemName copyright

v0.1.3
1.防止刷新时登录框一闪而过不好看。
2.混用require和import导致的页面不加载问题。
3.将页面图标等个性化信息组件化为site-title和login-bottom以供应用自己改写