<template>
  <a href="/">
    <span class="topbar-logo topbar-btn">
      <img :src="appConfig.logo" style="padding-left:8px;">
    </span>
    <span class="topbar-title" v-html="appConfig.titleHtml||appConfig.systemName">
    </span>
  </a>
</template>
<script>
export default {
};
</script>
