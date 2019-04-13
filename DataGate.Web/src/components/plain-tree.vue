<template>
<!-- 对有id和parentId的平面结构列表生成树 -->
<div>
<el-tree show-checkbox
    draggable
    :data="treeData"
    @current-change="doCurrentChange" 
    @check-change="doCheckChange"
    @node-drop="doNodeDrop"
    :expand-on-click-node="false"
    default-expand-all
    node-key="id" 
    ref="tree"
    highlight-current
    :props="defaultProps">
<div slot-scope="scope" style="width:100%">
<slot :node="scope.node" :data="scope.data">{{scope.node.label}}</slot>
</div>
</el-tree>
</div>
</template>
<script>
import util from "../common/util";
export default {
  props: {
    task: Object //任务对象
  },
  data() {
    return {
      current: null, //当前节点
      // filterText: null,
      selection: [], //勾选的节点
      defaultProps: {
        children: "children",
        label: "name"
      },
      raiseCheckChange: true //视图更新时tree会清除勾选，再此作一判断标志以恢复
    };
  },
  computed: {
    treeData() {
      var nestData = util.buildNestData(util.sort(this.task.products, "ord"));
      return nestData;
    }
  },
  methods: {
    doCurrentChange(data, node) {
      if (this.current) {
        this.task.acceptChanges();
      }
      this.current = data;
      this.task.setSelection([data]);
    },
    restoreSelection() {
      this.$nextTick(() => {
        this.$refs.tree.setCheckedKeys(this.selection.map(sel => sel.id));
        if (this.current) this.$refs.tree.setCurrentKey(this.current.id);
        this.raiseCheckChange = true;
      });
    },
    doCheckChange(data, isChecked, isCheckedChildren) {
      if (!this.raiseCheckChange) {
        return;
      }
      this.selection = this.$refs.tree.getCheckedNodes();
      if (isChecked) {
        this.$refs.tree.setCurrentKey(data.id);
        this.doCurrentChange(data); //setCurrentKey不会触发current-change事件
      }
    },
    addBrother() {
      var menu = this.task.createProduct();
      menu.parentId = this.current ? this.current.parentId : null;
      this.task.add(menu);
      this.restoreSelection();
      this.doCurrentChange(menu);
    },
    addChild() {
      if (!this.current) {
        this.$message("请先激活父节点");
        return;
      }
      this.raiseCheckChange = false;
      var menu = this.task.createProduct();
      menu.parentId = this.current.id;
      this.task.add(menu);
      this.restoreSelection();
      this.doCurrentChange(menu);
    },
    clone() {
      if (!this.current) {
        this.$message(`请先激活要克隆的${this.task.productName}`);
        return;
      }
      this.raiseCheckChange = false;
      var menu = $.extend({}, this.current);
      menu.id = util.guid();
      menu.ord = this.task.getMaxOrder();
      this.task.add(menu);
      this.restoreSelection();
      this.doCurrentChange(menu);
    },
    doUp() {
      if (!this.current) {
        this.$message(`请先激活要上移的${this.task.productName}`);
        return;
      }
      this.raiseCheckChange = false;
      var brothers = this.task.products.filter(
        m => (m.parentId || "") == (this.current.parentId || "")
      );
      var idx = brothers.indexOf(this.current);
      if (idx > 0) {
        var prev = brothers[idx - 1];
        var ord = prev.ord;
        prev.ord = this.current.ord;
        this.current.ord = ord;
        this.task.editBuffer.ord = ord;
        this.task.change(prev);
        this.task.change(this.current);
        this.restoreSelection();
      }
    },
    doDown() {
      if (!this.current) {
        this.$message(`请先激活要下移的${this.task.productName}`);
        return;
      }
      this.raiseCheckChange = false;
      var borthers = this.task.products.filter(
        m => (m.parentId || "") == (this.current.parentId || "")
      );

      var idx = borthers.indexOf(this.current);
      if (idx < borthers.length - 1) {
        var next = borthers[idx + 1];
        var ord = next.ord;
        next.ord = this.current.ord;
        this.task.editBuffer.ord = ord;
        this.current.ord = ord;
        this.task.change(next);
        this.task.change(this.current);
        this.restoreSelection();
      }
    },
    //拖拽完成：
    //共四个参数，依次为：
    //被拖拽节点对应的 Node、
    //结束拖拽时最后进入的节点、
    //被拖拽节点的放置位置（before、after、inner）、
    //event
    doNodeDrop(node, nodeEnter, position, evt) {
      switch (position) {
        case "before":
          this.moveBefore(node.data, nodeEnter.data);
          break;
        case "after":
          this.moveAfter(node.data, nodeEnter.data);
          break;
        case "inner":
          this.moveInner(node.data, nodeEnter.data);
          break;
      }
      this.task.change(node.data);
    },
    moveBefore(data, toData) {
      data.parentId = toData.parentId;
      var borthers = this.task.products.filter(
        m => (m.parentId || "") == (toData.parentId || "")
      );
      var idx = borthers.indexOf(toData);
      if (idx == 0) {
        data.ord = toData.ord - 5 - Math.random();
      } else {
        var d = 2 + Math.random() - Math.random();
        data.ord = (toData.ord + borthers[idx - 1].ord) / d;
      }
    },
    moveAfter(data, toData) {
      data.parentId = toData.parentId;
      var borthers = this.task.products.filter(
        m => (m.parentId || "") == (toData.parentId || "")
      );
      var idx = borthers.indexOf(toData);
      if (idx == borthers.length - 1) {
        data.ord = toData.ord + 5 + Math.random();
      } else {
        var d = 2 + Math.random() - Math.random();
        data.ord = (toData.ord + borthers[idx + 1].ord) / d;
      }
    },
    moveInner(data, toData) {
      data.parentId = toData.id;
      data.ord = this.task.getMaxOrder();
    },
    doDel() {
      if (!this.selection.length) {
        this.$message.warning(
          `您还没有勾选要删除的${this.task.productName},请先勾选要删除的${
            this.task.productName
          }。`
        );
        return;
      }
      this.raiseCheckChange = false;
      this.selection.forEach(item => {
        if (item == this.current) {
          this.current = null;
          this.selection = [];
          this.task.setSelection([]);
        }
        this.task.remove(item);
      });
      this.restoreSelection();
    },
    //保存前的验证
    validate() {
      this.task.acceptChanges();
      return this.task.validate().catch((r) => {
        this.$refs.tree.setCurrentKey(r.product.id);
        this.$message.error(r.err);
        this.doCurrentChange(r.product);
        throw Error; //如果不throw一下的话，下一步会是接受状态
      });
    }
  }
};
</script>
