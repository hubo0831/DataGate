<template>
  <!-- 用作单纯显示一个字段值的模板 -->
  <div>
    <template v-if="meta.uitype=='TextBox'">{{ value }}</template>
    <template v-else-if="meta.uitype=='TextArea'">{{ value }}</template>
    <template v-else-if="meta.uitype=='CheckBox' || meta.uitype=='Switch'">
      <i class="fa fa-check" aria-hidden="true" v-if="value"></i>
    </template>
    <template v-else-if="meta.uitype=='DropdownList'">{{ dropDownValue(value) }}</template>
    <template v-else-if="meta.uitype=='List'">{{dropDownMultiValue(value)}}</template>
    <template v-else-if="meta.uitype=='Date'">{{value | formatDate}}</template>
    <template v-else-if="meta.uitype=='DateTime'">{{value | formatDateTime}}</template>
    <template v-else-if="meta.uitype=='Custom'"><slot :meta="meta" :value="value" :obj="obj">{{value}}</slot></template>
    <!-- 数组类型 -->
    <template v-else-if="(meta.datatype||'').startsWith('[')">
      <slot :meta="meta" :value="value"></slot>
    </template>
    <!-- 定制的显示组件 -->
    <component
      v-else-if="meta.uitype"
      :in-edit="false"
      :is="meta.uitype"
      v-model="value"
      :meta="meta"
      :obj="obj"
      :in-form="inForm"
      v-bind="meta.attr"
    ></component>
    <template v-else>{{ value }}</template>
  </div>
</template>
<script>
export default {
  //meta-元数据 value-列的值 obj-整行的数据对象
  props: ["meta", "value", "obj", "inForm"],
  filters: {},
  methods: {
    //显示单选下拉列表的值
    dropDownValue(val) {
      for (var i in this.meta.options) {
        if (this.meta.options[i].value == val) return this.meta.options[i].text;
      }
    },
    //显示多选下拉列表的值，这里的val可能是，分隔的值或一个数组
    dropDownMultiValue(val) {
      var vk = this.meta.valuekey;
      if (typeof val == "string") {
        val = val.split(",");
      }
      else{
        val = val.map(v => v[vk]);
      }
      var options = this.meta.options.filter(
        op => val.indexOf(vk ? op.value[vk] : op.value) >= 0
      );
      return options.map(m => m.text).join(",");
    }
  }
};
</script>