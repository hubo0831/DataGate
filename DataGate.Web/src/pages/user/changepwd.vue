<template>
  <el-row class="warp">
    <el-col :span="12" class="warp-main">
      <el-form ref="form" status-icon :model="form" label-width="100px" :rules="rules" size="mini">
        <el-form-item label="账号">
          <label>{{userProfile.currentUser.account}}</label>
        </el-form-item>
        <el-form-item label="原密码" prop="oldPwd">
          <el-input type="password" v-model="form.oldPwd"></el-input>
        </el-form-item>
        <el-form-item label="新密码" prop="newPwd">
          <el-input type="password" v-model="form.newPwd"></el-input>
        </el-form-item>
        <el-form-item label="确认新密码" prop="confirmPwd">
          <el-input type="password" v-model="form.confirmPwd"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="default" @click="doChangepwd">提交</el-button>
        </el-form-item>
      </el-form>
    </el-col>
  </el-row>
</template>
<script>
import * as API from "../../api";
export default {
  data() {
    var validatePass0 = (rule, value, callback) => {
      if (value === "") {
        callback(new Error("请输入原密码"));
      } else {
        API.POST("/api/check/Password", { p: value }).done(r => {
          if (!r) {
            callback(new Error("原密码不正确"));
          }
          callback();
        });
      }
    };
    var validatePass = (rule, value, callback) => {
      if (value === "") {
        callback(new Error("请输入密码"));
      } else {
        if (this.form.oldPwd == value) {
          callback(new Error("不能和原密码相同"));
        } else if (this.form.confirmPwd !== "") {
          this.$refs.form.validateField("confirmPwd");
        }
        callback();
      }
    };
    var validatePass2 = (rule, value, callback) => {
      if (value === "") {
        callback(new Error("请再次输入密码"));
      } else if (value !== this.form.newPwd) {
        callback(new Error("两次输入密码不一致!"));
      } else {
        callback();
      }
    };

    return {
      form: {
        oldPwd: "",
        newPwd: "",
        confirmPwd: ""
      },
      rules: {
        oldPwd: [{ validator: validatePass0, trigger: "blur" }],
        newPwd: [{ validator: validatePass, trigger: "blur" }],
        confirmPwd: [{ validator: validatePass2, trigger: "blur" }]
      }
    };
  },

  methods: {
    doChangepwd() {
      this.$refs.form.validate(v => {
        if (!v) return;
        API.POST("/api/check/ChangePassword", { p: this.form.newPwd }).done(
          r => {
            this.$message.success("密码修改成功！");
          }
        );
      });
    }
  }
};
</script>
