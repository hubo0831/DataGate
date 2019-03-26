<template>
<el-col :span="16">
    Sorry, 您没有权限访问此资源。<br />
    资源地址：{{path}} <br />
    请联系管理员。
</el-col>
</template>
<script>
export default {
  props:["path"]
};
</script>