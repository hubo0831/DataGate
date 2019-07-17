<template>
  <div v-on:keyup.enter="search" v-if="metadata.length">
    <!-- @*筛选条件组件*@ -->
    <el-form :inline="true">
      <el-form-item v-for="meta in metaFilter" :key="meta.name" :label="meta.title">
        <template v-if="meta.uitype=='Date'|| meta.uitype=='DateTime'">
          <div v-if="meta.operator=='bt'">
            <span>
              <el-date-picker
                v-model="meta.value"
                type="date"
                @change="onChange(meta)"
                clearable
                placeholder="起始日期"
              ></el-date-picker>
            </span> ~
            <span>
              <el-date-picker
                v-model="meta.value1"
                type="date"
                @change="onChange(meta)"
                clearable
                placeholder="结束日期"
              ></el-date-picker>
            </span>
          </div>
          <span v-else>
            <el-date-picker
              v-model="meta.value"
              type="date"
              @change="onChange(meta)"
              clearable
              :placeholder="getPlaceholder(meta)"
            ></el-date-picker>
          </span>
        </template>
        <div :style="{width:itemWidth+'px'}" v-else>
          <template v-if="meta.uitype=='List'">
            <el-select
              v-model="meta.value"
              multiple
              filterable
              @change="onChange"
              allow-create
              style="width:100%"
              :placeholder="getPlaceholder(meta)"
            >
              <el-option
                v-for="sel in meta.options"
                :key="sel.value"
                :label="sel.text"
                :value="sel.value"
              ></el-option>
            </el-select>
          </template>
          <template v-else-if="meta.uitype=='DropdownList'">
            <el-select
              v-model="meta.value"
              style="width:100%"
              clearable
              filterable
              allow-create
              @change="onChange(meta)"
              :placeholder="getPlaceholder(meta)"
            >
              <el-option
                v-for="sel in meta.options"
                :key="sel.value"
                :label="sel.text"
                :value="sel.value"
              ></el-option>
            </el-select>
          </template>
          <el-checkbox
            v-else-if="meta.uitype=='CheckBox'"
            v-model="meta.value"
            :true-label="'1'"
            @change="onChange(meta)"
            :indeterminate="meta.value !=1 && meta.value!=0"
            :false-label="'0'"
          ></el-checkbox>
          <template v-else-if="meta.uitype=='TextBox'">
            <el-input
              v-model="meta.value"
              style="width:100%"
              clearable
              :placeholder="getPlaceholder(meta)"
              @input="onInput(meta)"
            ></el-input>
          </template>
          <template v-else-if="meta.uitype=='TextArea'">
            <el-input
              v-model="meta.value"
              style="width:100%"
              clearable
              :placeholder="getPlaceholder(meta)"
              @change="onChange(meta)"
            ></el-input>
          </template>
          <!-- Custom自定义组件暂时用文本框 -->
          <template v-else-if="meta.uitype=='Custom'">
            <el-input
              @change="onChange(meta)"
              style="width:100%"
              v-model="meta.value"
              clearable
              :placeholder="getPlaceholder(meta)"
            ></el-input>
          </template>
          <!-- 自定义输入组件 -->
          <component
            v-else-if="meta.uitype"
            :is="meta.uitype"
            v-model="meta.value"
            :meta="meta"
            :obj="meta"
            :in-edit="true"
            @change="onChange(meta)"
            :placeholder="getPlaceholder(meta)"
            v-bind="meta.attr"
          ></component>
          <!-- 没有明确定义的组件 -->
          <template v-else>
            <el-input
              v-model="meta.value"
              clearable
              :placeholder="meta.title"
              @input="onInput(meta)"
            ></el-input>
          </template>
        </div>
      </el-form-item>
      <el-form-item>
        <el-button-group>
          <el-button
            type="primary"
            icon="fa fa-search"
            title="搜索"
            v-if="metaFilter.length>1"
            native-type="submit"
            @click.prevent="search"
          ></el-button>
          <el-button icon="fa fa-rotate-right" title="重置" @click="reset"></el-button>
        </el-button-group>
        <slot></slot>
      </el-form-item>
    </el-form>
  </div>
</template>
<script>
import util from "../common/util.js";
import editTask from "../common/editTask.js";
var task = new editTask();
var timeOut = 0;

export default {
  props: {
    //传入的待搜索的元数据定义
    metadata: {
      type: Array,
      default: function() {
        return [];
      }
    },
    itemWidth: {
      type: Number,
      default: 150
    }
  },
  data() {
    return {
      r: 0 //确保地址栏会更新，在空按搜索按钮时仍然会搜索
    };
  },
  inject: ["urlQuery"],
  computed: {
    metaFilter() {
      //按照原order顺序排序，如果order是负数则取绝对值
      //因为原不显示出来的字段可能也要参加搜索
      return util.sort(this.metadata, m => Math.abs(m.order || 0));
    }
  },
  watch: {
    metadata(val) {
      let { r } = this;
      val.forEach(meta => {
        if (!meta.operator) {
          meta.operator = this.getOperators(meta)[0].value;
        }
        if (!("value" in meta) || r == 0) {
          this.$set(meta, "value", null); //去掉meta的默认值
          this.$set(meta, "value1", null);
        }
      });
      r++;
      task.updateAllOptions(val);
      this.restoreFormValue();
    }
  },

  methods: {
    onChange(meta) {
      //如果只有一个框就立即搜
      if (
        this.metaFilter.length == 1 ||
        (meta.column && meta.column.fastsearch)
      ) {
        this.search();
      }
    },
    onInput(meta) {
      //如果只有一个框，文本框，值输入就开始搜
      if (
        this.metaFilter.length == 1 ||
        (meta.column && meta.column.fastsearch)
      ) {
        clearTimeout(timeOut);
        timeOut = setTimeout(this.search, 500);
      }
    },
    restoreFormValue() {
      var query = this.urlQuery._filter;
      if (query) query = JSON.parse(query);
      if (!query) return;
      for (var i in query) {
        var meta = this.metaFilter.find(f => f.name == query[i].n);
        if (!meta) continue;
        meta.value = query[i].v;
        if (query[i].v1) meta.value1 = query[i].v1;
      }
    },
    //计算比较表达式运算符
    getOperators: function(meta) {
      switch (meta.uitype) {
        case "Date":
        case "DateTime":
          return [
            {
              text: "介于",
              value: "bt"
            },
            {
              text: "等于",
              value: "de"
            },
            {
              text: "早于",
              value: "lte"
            },
            {
              text: "晚于",
              value: "gte"
            }
          ];
        case "List":
          return [
            {
              text: "包含",
              value: "in"
            },
            {
              text: "不包含",
              value: "nin"
            }
          ];
        default:
          return [
            {
              text: "包含",
              value: "i"
            },
            {
              text: "不包含",
              value: "ni"
            },
            {
              text: "等于",
              value: "e"
            },
            {
              text: "为空",
              value: "n"
            },
            {
              text: "不为空",
              value: "nn"
            }
          ];
      }
    },
    //TODO：数据检验
    validate: function(callback) {
      return callback(true);
    },
    getPlaceholder(meta) {
      if (meta.attr && meta.attr.placeholder) return meta.attr.placeholder;
      return meta.title || meta.name;
    },
    search: function() {
      var filter = [];
      this.metadata.forEach(meta => {
        if (
          !(
            meta.value ||
            meta.value === 0 ||
            meta.operator == "n" ||
            meta.operator == "nn"
          )
        ) {
          return;
        }
        var wp = {
          o: meta.operator,
          n: meta.name,
          v: meta.value
        };
        if (meta.value1) wp.v1 = meta.value1;
        filter.push(wp);
      });
      // filter = filter
      //   .map(f => {
      //     var str = `${f.n}|${f.o}|${f.v}`;
      //     if (f.v1) str += "|" + f.v1;
      //     return str;
      //   })
      //   .join("`");
      this.validate(valid => {
        if (valid && this.$emitPass("search", filter).passed) {
          if (filter.length) {
            var f = JSON.stringify(filter);
            if (f.length >= 2000) {
              this.$message.error("您输入的查询条件过长，请适当减少一些条件。");
              return;
            }
            this.urlQuery._filter = f;
          } else {
            this.$delete(this.urlQuery, "_filter");
          }
          this.urlQuery._r = this.r++; //强行修改url让数据能在点按钮时刷新
          this.$router.replace({
            path: this.$route.path,
            query: this.urlQuery
          });
        }
      });
    },
    reset: function() {
      this.metadata.forEach(meta => {
        meta.value = null;
        meta.value1 = null;
      });
      this.search();
    }
  }
};
</script>
<style scoped lang="scss">
.el-form-item {
  margin-bottom: 10px;
}
.search-input {
  width: 150px;
}
</style>