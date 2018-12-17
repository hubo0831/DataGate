<template>
  <a href="/">
    <div class="topbar-logo topbar-btn">
      <img :src="appConfig.logo" style="padding-left:8px;">
    </div>
    <div class="topbar-title">
      <span style="font-size: 19px;color: #fff;" v-html="appConfig.titleHtml"></span>
    </div>
  </a>
</template>
<script>
export default {
};
</script>
