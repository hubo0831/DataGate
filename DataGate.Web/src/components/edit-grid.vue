<!--将element-ui的table加上编辑功能-->
<template>
  <div @keyup.enter="submitRow()" @keyup.escape="cancelEdit()">
    <el-form
      :inline="true"
      :model="task.editBuffer"
      ref="gridForm"
      :rules="task.rules"
      status-icon
      :show-message="false"
    >
      <el-table
        v-bind:data="dataList"
        ref="dataGrid"
        border
        fit
        highlight-current-row
        @sort-change="doSortChange"
        :stripe="stripe"
        :height="height"
        @selection-change="doSelectionChange"
        @row-dblclick="doRowDblClick"
        @row-click="doRowClick"
        tooltip-effect="light"
        style="width: 100%;"
      >
        <el-table-column type="selection" v-if="multiSelect"></el-table-column>
        <el-table-column type="index" v-if="showIndex"></el-table-column>
        <el-table-column
          :show-overflow-tooltip="meta.column.minWidth>120"
          :prop="meta.name"
          :label="meta.title"
          v-for="meta in metaFilter"
          :key="meta.name"
          header-align="center"
          :class-name="getColumnClass(meta)"
          v-bind="meta.column"
        >
          <template slot-scope="scope">
            <!-- 非当前编辑行或只读字段只显示 -->
            <template v-if="(scope.row != editingRow) || scope.row.readonly || editMode !='inline'">
              <slot
                :name="meta.name"
                :in-edit="false"
                v-if="meta.uitype=='Custom'"
                :meta="meta"
                :obj="scope.row"
              >
                <!-- 自定义显示状态的数据列内容 -->
                {{scope.row[meta.name]}}
              </slot>
              <display-item
                v-else
                :meta="meta"
                v-model="scope.row[meta.name]"
                :obj="scope.row"
              >{{showArray(scope.row, meta)}}</display-item>
            </template>
            <!-- 下面是编辑状态时的编辑界面 -->
            <!-- 显示时是一个字段，编辑时又是另一个字段的情况 -->
            <el-form-item v-else-if="meta.linkto && meta.uitype=='TextBox'" :prop="meta.linkto">
              <edit-item :obj="task.editBuffer" :meta="getMeta(meta.linkto)"></edit-item>
            </el-form-item>
            <!-- 自定义编辑列的内容 -->
            <slot
              :name="meta.name"
              :in-edit="true"
              :obj="scope.row"
              :meta="meta"
              v-else-if="meta.uitype=='Custom'"
            >{{scope.row[meta.name]}}</slot>
            <!-- 常规编辑 -->
            <el-form-item v-else :prop="meta.name">
              <edit-item :obj="task.editBuffer" :meta="meta"></edit-item>
            </el-form-item>
          </template>
        </el-table-column>
      </el-table>
    </el-form>
    <template v-if="editMode=='popup'">
      <!-- 不能用v-show -->
      <el-dialog
        :visible.sync="showEdit"
        :close-on-click-modal="false"
        top="10vh"
        @closed="cancelEdit"
      >
        <template slot="title">
          <slot name="editer-title" :new-item="newItem">
            <template v-if="newItem">
              <i class="fa fa-plus"></i>
              新增{{task.productName}}
            </template>
            <template v-else>修改{{task.productName}}</template>
          </slot>
        </template>
        <slot name="editer-header"></slot>
        <edit-form
          ref="editorForm"
          :task="task"
          style="margin-right:30px"
          :height="height?height - 90:0"
          :label-width="labelWidth"
        >
          <slot name="edit-form-item">
            <!-- UIType='Custome'的组件在edit-from内的插槽 -->
          </slot>
        </edit-form>
        <span slot="footer" style="margin-right:30px" class="dialog-footer">
          <el-button type="primary" @click="doSave">保存</el-button>
          <el-button @click="cancelEdit">取消</el-button>
        </span>
      </el-dialog>
    </template>
    <template v-else-if="editMode=='side'">
      <div v-show="showEdit" class="card side-edit">
        <div class="clearfix card-header">
          <slot name="editer-title" :new-item="newItem"></slot>
        </div>
        <div class="card-content">
          <edit-form ref="editorForm" :task="task"></edit-form>
        </div>
      </div>
    </template>
  </div>
</template>
<script>
import { Util, Bus } from "../";
export default {
  props: {
    task: Object, //包含数据，元数据的对象
    data: Array, //单独指定数据，如果不指定则使用task.products
    metadata: Array, //单独指定元数据，如果不指定则使用task.metadata
    height: Number, //高度(px)
    multiSelect: Boolean, //多选,默认false
    multiEdit: Boolean, //是否支持同时编辑多项,默认false
    showIndex: {
      type: Boolean, //显示行号
      default: true
    },
    stripe: {
      type: Boolean, //显示斑马线
      default: true
    },
    editMode: {
      //编辑模式：inline-行内编辑, side-侧边栏编辑, popup-弹出窗, none-不能编辑
      //如果有其他自定义模式，响应事件 show-edit 来自定义编辑行为，在事件中，通过task.editBuffer来得到当前要编辑的行信息
      type: String,
      default: "inline"
    },
    labelWidth: {
      type: Number,
      default: 120
    }
  },
  data: function() {
    return {
      current: null,
      editingRow: null, //当前正在编辑的行
      showEdit: false,
      newItem: null //点击新增按钮时的新增对象暂存
    };
  },
  inject: ["urlQuery"],
  watch: {
    //切换任务（如分页时）重置标志
    task() {
      this.current = null;
      this.editingRow = null;
      this.showEdit = false;
    }
  },
  created() {
    // if (!this.dataList) {
    //   this.dataList = this.task.products;
    // }
    // if (!this.metadata) {
    //   this.metadata = this.task.metadata;
    // }
  },
  // mounted(){
  //   if (this.editMode == "popup")
  //   $('.editer-container').silmScroll();
  // },
  directives: {
    focus: {
      // 指令的定义
      inserted: function(el) {
        el.focus();
      }
    }
  },
  computed: {
    dataList() {
      return this.data || this.task.products;
    },
    metaFilter() {
      var metas = this.metadata || this.task.metadata;
      return Util.sort(metas.filter(m => m.order >= 0), m => m.order);
    }
  },
  methods: {
    getColumnClass(meta) {
      var args = { meta, passed: true, className: "" };
      this.$emitPass("get-column-class", args);
      return args.className;
    },
    getMeta(name) {
      var metas = this.metadata || this.task.metadata;
      var meta = metas.find(m => m.name == name);
      if (!meta) throw `没有找到${name}对应的元数据。`;
      return meta;
    },
    showArray(row, meta) {
      var args = {
        row,
        meta,
        value: row[meta.name]
      };
      this.$emit("format-array", args);
      return args.value;
    },
    //用户点击列标题排序的事件
    doSortChange(cpo) {
      if (!this.$emitPass("sort-change", cpo).passed) return;
      if (!cpo.prop) this.$delete(this.urlQuery, "_sort");
      else
        this.urlQuery._sort =
          cpo.prop + " " + ((cpo.order || "").startsWith("d") ? "d" : "a");

      this.$router.replace({
        path: this.$route.path,
        query: this.urlQuery
      });
    },
    //勾选事件
    doSelectionChange(items) {
      this.submitRow(true).then(() => this.task.setSelection(items));
      if (items.length == 1) {
        //刚开始时，鼠标如果正好点到复选框，将不会有当前行， 在此处强行指定
        this.changeCurrentRow(items[0]);
      } else if (items.length == 0) {
        this.changeCurrentRow(null);
      }
    },
    //为免与selectionChange和rowclick事件冲突，不触发，只调用此方法
    changeCurrentRow(item) {
      this.$refs.dataGrid.setCurrentRow(item);
      if (this.current != item) {
        this.current = item;
        this.$emit("current-change", item);
      }
    },
    //统一处理table的行点击事件，当行点击时自动选择
    doRowClick: function(row, event, column) {
      if (row == this.current) return;
      this.newItem = null;
      if (column.type != "selection") {
        this.$refs.dataGrid.clearSelection();
      }
      this.$refs.dataGrid.toggleRowSelection(row);
    },
    //双击编辑
    doRowDblClick(item) {
      //只有在待编辑的行通过后才能进入新行的编辑
      this.submitRow().then(() => {
        this.$emitPass(
          "row-dblclick",
          {
            item,
            passed: this.editMode != "none"
          },
          this.doEdit
        );
      });
    },
    doEdit() {
      if (!this.multiEdit && this.task.selection.length > 1) {
        this.$message.warning("不能同时编辑多行。");
        return;
      }
      if (!this.current) {
        this.$message.warning("您还没有选择要编辑的行。");
        return;
      }
      this.submitRow().then(() => {
        this.editingRow = this.current;
        //  this.task.editBuffer = $.extend({}, this.current);
        this.showEdit = true;
        this.$emit("show-edit", this.newItem);
      });
    },
    //光标自动到指定行 https://www.jianshu.com/p/e2710643370d
    gotoRow(row) {
      if (!this.dataList) return;

      // 获取行高
      let height =
        parseInt(
          $(this.$refs.dataGrid.$el)
            .find(".el-table__body tbody td:first-child")
            .height()
        ) || 32;

      // 遍历表格数据，获取查询的数据
      for (let i = 0; i < this.dataList.length; i++) {
        const item = this.dataList[i];
        // 判断查询的数据是否存在，存在则进行定位操作
        if (item == row) {
          this.$refs.dataGrid.bodyWrapper.scrollTop = height * (i - 1) + 10;
          break;
        }
      }
    },
    getMaxOrder() {
      var sortField = this.task.getSortField();
      if (!sortField) return null;
      var max = 0;
      this.dataList.forEach(row => {
        if (max < row[sortField]) max = row[sortField];
      });
      return max;
    },
    setOrder(rowData) {
      var sortField = this.task.getSortField();
      if (!sortField) return;
      rowData[sortField] = this.getMaxOrder() + 1;
    },
    doAdd() {
      this.submitRow().then(() => {
        var newItem = this.task.createProduct();
        this.setOrder(newItem);
        this.$emit("new-row", newItem);
        this.$refs.dataGrid.clearSelection();
        this.task.changeStatus(newItem, "added");
        this.newItem = newItem;
        this.$nextTick(() => {
          this.$refs.dataGrid.toggleRowSelection(newItem);
          if (this.editMode == "inline") this.gotoRow(this.current);
          this.doEdit();
        });
      });
    },
    doSave() {
      var saveTask = () => {
        this.task.acceptChanges();
        this.showEdit = false;
        this.editingRow = null;
        this.newItem = null;
        this.$emit("save-task", this.task);
      };

      if (this.editMode == "popup" || this.editMode == "side") {
        //只有在表单编辑时才会触发
        this.$refs.editorForm.validate(v => {
          if (!v) return;
          saveTask();
        });
      } else {
        this.$emitPass("validate", this.task, saveTask);
      }
    },
    cancelEdit() {
      if (this.newItem) {
        this.task.changeStatus(this.newItem, "removed");
        this.newItem = null;
      }
      this.showEdit = false;
      this.editingRow = null;
    },
    //inline编辑时的提交动作
    submitRow(silence) {
      var dfd = Util.deferred();
      if (this.editingRow) {
        this.$refs.gridForm.validate(v => {
          if (v) {
            //应该是只有在行内编辑时才会触发的分支
            this.task.acceptChanges();
            this.editingRow = null;
            dfd.resolve();
          } else if (!silence) {
            this.$message.warning("请先完善正在编辑的行。");
          }
        });
      } else {
        dfd.resolve();
      }
      return dfd.promise;
    },
    //确认删除后执行删除
    performDelRow(item) {
      if (item == this.editingRow) this.editingRow = null;
      if (item == this.current) {
        var idx = this.dataList.indexOf(this.current);
        if (idx == this.dataList.length - 1) idx--;
        this.$nextTick(() => {
          if (idx >= 0 && idx < this.dataList.length)
            this.$refs.dataGrid.setCurrentRow(this.dataList[idx]);
        });
      }
      this.task.changeStatus(item, "removed");
    },
    //确认删除
    confirmDelRow(item) {
      this.$emitPass("before-del-row", item, this.performDelRow);
    },
    doDel() {
      var rowsForDel = this.task.selection;
      if (rowsForDel.length == 0 && this.current) {
        rowsForDel = [this.current];
      }
      if (!rowsForDel.length) {
        this.$message.warning("您还没有选择要删除的行。");
        return;
      }
      this.$emitPass("before-del-rows", rowsForDel, sel => {
        sel.forEach(this.performDelRow);
        //在inline模式下，删除后不立即保存，
        //在其他模式下，删除后要立即提交到服务器删除，这项工作由父组件来做
        this.$emit("after-del-rows");
      });
    },
    //交换上下排序位
    swapOrder(curr, slibing) {
      var sortField = this.task.getSortField();
      if (!sortField) {
        this.$message.warning("没有定义排序位[datatype=SortOrder]");
        return;
      }

      var ord = slibing[sortField];
      this.task.changeStatus(slibing, "changed");
      this.task.changeStatus(curr, "changed");
      slibing[sortField] = curr[sortField];
      curr[sortField] = ord;
      this.task.editBuffer[sortField] = ord;
      this.$nextTick(() => this.$refs.dataGrid.toggleRowSelection(curr));
    },
    //上移
    doUp() {
      if (!this.current) {
        this.$message.warning("请先选择要上移的行");
        return;
      }
      var idx = this.dataList.indexOf(this.current);
      if (idx > 0) {
        this.swapOrder(this.current, this.dataList[idx - 1]);
      }
    },
    //下移
    doDown() {
      if (!this.current) {
        this.$message.warning("请先选择要下移的行");
        return;
      }
      var idx = this.dataList.indexOf(this.current);
      if (idx < this.dataList.length - 1) {
        this.swapOrder(this.current, this.dataList[idx + 1]);
      }
    }
  }
};
</script>
<style scoped lang="scss">
.dg-toolbar {
  margin: 5px 0;
}

.el-form-item {
  margin: 0;
  padding: 0;
}
.editer-container {
  overflow: auto;
}
.side-edit {
  position: fixed;
  top: 80px;
  right: 0px;
  width: 400px;
  display: block;
}
</style>
