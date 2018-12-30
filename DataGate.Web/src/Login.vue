<template>
  <div class="container">
    <div class="topbar-wrap">
      <div class="login-center" style="margin:20px;padding-left:460px;">
        <site-title></site-title>
      </div>
    </div>
    <div class="login-middle">
      <div class="login-center">
        <div style="height:30px"></div>
        <div class="login-container">
          <el-form
            ref="AccountForm"
            :model="loginModel"
            :rules="rules"
            label-position="left"
            label-width="0px"
          >
            <h3 class="title">系统登录</h3>
            <el-form-item prop="account">
              <el-input
                type="text"
                v-model="loginModel.account"
                autofocus
                autoselect
                placeholder="账号"
              ></el-input>
            </el-form-item>
            <el-form-item prop="password">
              <el-input type="password" v-model="loginModel.password" placeholder="密码"></el-input>
            </el-form-item>
            <el-checkbox
              v-model="loginModel.remember"
              true-label="1"
              false-label="0"
              class="remember"
              style="color:#409EFF"
            >记住我</el-checkbox>
            <el-form-item style="width:100%;">
              <el-button
                type="primary"
                style="width:100%;"
                @click.native.prevent="doLogin"
                native-type="submit"
                :loading="loading"
              >登录</el-button>
            </el-form-item>
          </el-form>
        </div>
      </div>
    </div>
    <div class="login-bottom">
      <div class="login-center">
        <login-bottom/>
      </div>
    </div>
  </div>
</template>

<script>
import API from "./api/api_user";
import UserAPI from "./api/api_user.js";
import pubmixin from "./pages/pubmixin.js";
import appConfig from "./appConfig";
import bus from "./bus";
import "./assets/styles/login.scss";
export default {
  mixins: [pubmixin],
  data() {
    return {
      loginModel: {
        account: "",
        password: "",
        //TODO:记住我的实现：需要提交到服务器，服务器返回加密后的用户标识到cookie中保存起来
        remember: 0
      },
      rules: {
        account: [
          {
            required: true,
            message: "请输入账号",
            trigger: "blur"
          }
        ],
        password: [
          {
            required: true,
            message: "请输入密码",
            trigger: "blur"
          }
        ]
      }
    };
  },
  created() {
    //如果上次勾了记住我，则尝试用记住我的信息自动登录
    API.rememberLogin().then(remember => {
      if (remember == 0 || remember == 1) this.loginModel.remember = remember;
    });
  },
  methods: {
    doLogin() {
      this.$refs.AccountForm.validate(valid => {
        if (!valid) return;
        UserAPI.login(this.loginModel).catch(result => {
          // for (var i in result) {
          //   console.log("result[" + i + "]=" + result[i]);
          // }
          that.$message.error(result.$message || "登录失败");
        });
      });
    }
  }
};
</script>
