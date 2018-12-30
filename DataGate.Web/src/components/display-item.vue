<template>
  <!-- 用作单纯显示一个字段值的模板 -->
  <div>
    <template v-if="meta.uitype=='TextBox'">{{ value }}</template>
    <template v-else-if="meta.uitype=='TextArea'">{{ value }}</template>
    <template v-else-if="meta.uitype=='CheckBox' || meta.uitype=='Switch'">
      <i class="fa fa-check" aria-hidden="true" v-if="value"></i>
    </template>
    <template v-else-if="meta.uitype=='DropdownList'">{{ value }}</template>
    <template v-else-if="meta.uitype=='Date'">{{value | formatDate}}</template>
    <template v-else-if="meta.uitype=='DateTime'">{{value | formatDateTime}}</template>
      <!-- 数组类型 -->
    <template v-else-if="(meta.datatype||'').startsWith('[')">
      <slot :meta="meta" :value="value"></slot>
    </template>
  <!-- 自定义显示组件 -->
    <component
      v-else-if="meta.uitype"
      :in-edit="false"
      :is="meta.uitype"
      v-model="value"
      :meta="meta"
      :obj="obj"
      :placeholder="meta.title"
    ></component>
    <template v-else>{{ value }}</template>
  </div>
</template>
<script>
export default {
  //meta-元数据 value-列的值 obj-整行的数据对象
  props: ["meta", "value", "obj"],
  methods: {}
};
</script>