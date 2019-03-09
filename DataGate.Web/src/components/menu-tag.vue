<template>
  <!-- 个性化菜单项显示 -->
  <div class="tagmenu">
    <i :class="menu.iconCls || 'fa fa-file-o'"></i>
    {{menu.name}}
    <sup v-if="menu.tag">{{menu.tag}}</sup>
  </div>
</template>

<script>
export default {
  props: ["menu"]
};
</script>

<style lang="scss" scoped>
.tagmenu {
  sup {
    position: relative;
    right: 5px;
    top: -5px;
    font-size: 9px;
    background:#f56c6c;
    
    border-radius: 10px;
    padding:2px 5px;

  }
}
</style>
