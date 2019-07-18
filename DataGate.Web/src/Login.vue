<template>
  <div class="container">
    <div class="login-top">
      <site-title></site-title>
    </div>
    <div class="login-middle">
      <div class="login-container" v-loading="loading">
        <el-form
          ref="AccountForm"
          :model="loginModel"
          :rules="rules"
          label-position="left"
          label-width="0px"
          v-if="!forgot"
        >
          <h3 class="title">系统登录</h3>
          <el-form-item prop="account">
            <el-input
              type="text"
              v-model="loginModel.account"
              autofocus
              autoselect
              placeholder="账号/手机/邮箱"
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
          <!-- <a href="#" style="float:right" @click="forgot=true">
              忘记密码
              <i class="fa fa-question"></i>
          </a>-->
          <el-form-item style="width:100%;">
            <el-button
              type="primary"
              style="width:100%;"
              @click.native.prevent="doLogin"
              native-type="submit"
            >登录</el-button>
          </el-form-item>
        </el-form>
        <el-form ref="ForgotForm" label-width="80px" :rules="forgotRules" v-else>
          <h3 class="title">请输入要找回的账号信息</h3>
          <el-form-item prop="account" label="注册账号">
            <el-input
              type="text"
              v-model="forgotModel.account"
              autofocus
              autoselect
              placeholder="账号"
            ></el-input>
          </el-form-item>
          <el-form-item prop="email" label="注册邮箱">
            <el-input type="email" v-model="forgotModel.email" placeholder="邮箱"></el-input>
          </el-form-item>
          <div style="text-align:center">
            <el-button
              type="primary"
              @click.native.prevent="doForgot"
              native-type="submit"
              :loading="loading"
            >发送密码重置邮件</el-button>
            <el-button @click="forgot=false">返回登录</el-button>
          </div>
        </el-form>
      </div>
      <div style="clear:both"></div>
    </div>
    <div class="login-bottom">
      <login-bottom/>
    </div>
  </div>
</template>

<script>
import API from "./api/api_user";
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
      forgotModel: {
        account: "",
        email: ""
      },
      forgot: false,
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
      },
      forgotRules: {
        account: [
          {
            required: true,
            message: "请输入账号",
            trigger: "blur"
          }
        ],
        email: [
          {
            required: true,
            message: "请输入邮箱",
            trigger: "blur"
          }
        ]
      }
    };
  },
  created() {
    var args = { passed: false };
    //如果有第三方登录,
    bus.$emitPass("custom-login", args, null, () =>
      //如果上次勾了记住我，则尝试用记住我的信息自动登录
      API.rememberLogin().catch(remember => {
        this.loginModel.remember = remember;
      })
    );
  },
  methods: {
    doLogin() {
      this.$refs.AccountForm.validate(valid => {
        if (!valid) return;
        API.login(this.loginModel).catch(result => {
          // for (var i in result) {
          //   console.log("result[" + i + "]=" + result[i]);
          // }
          this.$message.error(result.$message || "登录失败");
        });
      });
    },
    doForgot() {
      API.forgot(this.forgotModel)
        .then(result => {
          this.$message.success("密码重置邮件发送成功！请查收。");
        })
        .catch(result => {
          this.$message.error(result.$message || "密码重置失败");
        });
    }
  }
};
</script>
