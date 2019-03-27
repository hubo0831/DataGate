import util from "../common/util"
import * as API from "../api"
import appConfig from "../appConfig"

//用于批量编辑和批量保存
export default function editTask() {
  this.metadata = [];
  this.name = ""; //任务名称
  this.productName = ""; //任务对应的成果名称
  this.key = ""; //在提交服务器时的数据修改的key
  this.rules = null; //表单验证规则集合

  //清除元数据以外的数据
  //此方法运用不当可能会造成页面闪动
  this.clearData = (paged) => {
    this.products = []; //当前的主表
    this.addedProducts = []; //新增的记录
    this.changedProducts = []; //修改过的记录
    this.removedProducts = []; //已删除的记录
    this.changed = 0;
    this.selection = []; //勾选中的成果列表，应该是products的子集
    this.editBuffer = {}; //根据selection合并后组成的单个成果，用以绑定form表单的值
    this.details = {}; //主从表中的子表

    //如果是分页查询，则设置排序为custom
    if (paged) {
      this.metadata.forEach(meta => meta.column.sortable && (meta.column.sortable = "custom"));
    }
  }
  this.clearData();

  //生成明细表数据的子任务
  //propName - 主表中存明细表数据的集合属性，
  //key - 提交更改时的明细表数据key
  this.createDetails = function (propName, key) {
    if (!this.products) {
      throw '必须先有主集合才能有子集合';
    }
    var detailTask = new editTask();
    detailTask.key = key;
    detailTask.clearData();
    this.products.forEach(p => {
      detailTask.products = detailTask.products.concat(p[propName]);
    });
    this.details[propName] = detailTask;
  };

  //////////////////////////////元数据定义//////////////////////////////////////

  //不能直接给metadta赋值,因担心和vue起冲突，所以没有用get set访问器
  this.setMetadata = meta => {
    var mtemp = meta || [];

    var pkey = mtemp.find(p => p.primarykey);
    if (!pkey) {
      pkey = mtemp.find(p => p.name == "id");
      if (pkey) pkey.primarykey = true;
    }
    if (!pkey) {
      //  throw "表没有定义主键";
    }

    mtemp.forEach(m => {
      //在进vue响应式之前先加点料
      m.multiValue = false;
      if (!m.column) m.column = {};
      //没有声明显示顺序的主键都不显示
      if (m.primarykey && !m.order && m.order != 0) m.order = -1;
      else if (!m.order) m.order = 0;

      //外表字段不参与编辑
      if (m.foreignfield && !m.formorder) m.formorder = -1;

      //没有声明编辑顺序则编辑顺序等于列表顺序
      if (!m.formorder && m.formorder != 0) m.formorder = m.order;

      //没有声明datatype则推测
      if (!m.datatype) m.datatype = guessDataType(m);

      //没有声明uitype则推测
      if (!m.uitype) m.uitype = guessUIType(m);

      //没有声明显示标题则默认为字段名
      if (!m.title) m.title = m.name;

      //没有声明显示宽度则默认100
      if (!m.column.minWidth && !m.column.width) m.column.minWidth = (appConfig.elSize > 1 ? 120 : 100);

      //没有明确定义可排序的都认为可以排序
      if (typeof m.column.sortable == "undefined") {
        m.column.sortable = true;
      }

      if (!m.align) {
        //数字默认右对齐
        if (m.datatype == "Number") m.column.align = "right";
        //日期默认居中对齐
        else if (m.datatype == "Date") m.column.align = "center";
        else if (m.datatype == "Boolean") m.column.align = "center";
      }

      //将表单控件绑定对象attr加入，如果没有定义的话
      if (!('attr' in m)) m.attr = {};
      //value默认值加入响应式
      if (!('value' in m)) m.value = null;
    });

    this.metadata = mtemp;
    this.updateAllOptions();
    this.rules = this._initRules();
  };

  this._initRules = function () {
    var rules = {};
    this.metadata.forEach(meta => {
      rules[meta.name] = [];
      if (meta.required) {
        var requiredRule = {
          required: true,
          message: '请输入' + meta.title,
          trigger: 'blur'
        }
        rules[meta.name].push(requiredRule);
      }
      if (meta.datatype == "Number") {
        var minMaxRule = {
          validator: validateNumber,
          min: meta.attr.min,
          max: meta.attr.max,
          trigger: 'blur'
        }
        rules[meta.name].push(minMaxRule);
      }
      if (meta.attr.pattern) {
        var patternRule = {
          validator: validateReg,
          pattern: meta.attr.pattern,
          trigger: 'blur'
        };
        rules[meta.name].push(patternRule);
      }
      rules[meta.name].forEach(rule => rule.title = meta.title);
      //todo:其他各类标准验证...
    });

    return rules;
  };

  function validateReg(rule, value, callback) {
    var reg = new RegExp(rule.pattern);
    if (reg.test(value)) {
      callback();
    } else {
      callback(new Error(rule.title + ":格式不对,应满足：" + rule.pattern));
    }
  }

  function validateNumber(rule, value, callback) {
    if (!value) {
      callback();
      return;
    }
    var num = parseFloat(value);
    if (num == NaN) {
      callback(new Error("必须为数字"));
      return;
    }
    if (!rule.min) {
      rule.min = 0;
    }
    if (!rule.max) {
      rule.max = 99999999999999999999;
    }
    if (value < rule.min) {
      callback(new Error(rule.title + ":数字太小"));
    } else if (value > rule.max) {
      callback(new Error(rule.title + ":数字太大"));
    } else {
      callback();
    }
  }

  //设置自定义检验规则，validateFunc的签名是(rule, value, callback)
  //验证不通过时是callback(new Error('错误信息'));通过时空的callback()
  this.setRule = function (name, validateFunc) {
    var meta = this.metadta.find(m => m.name == name);
    if (!meta) return;
    if (!this.rules[meta.name]) {
      this.rules[meta.name] = [];
    }
    this.rules[meta.name].push({
      validator: validateFunc,
      trigger: 'blur'
    })
  }

  function guessDataType(meta) {
    switch (meta.uitype) {
      case "Date":
        return "Date";
      case "DateTime":
        return "DateTime";
      case "CheckBox":
      case "Switch":
        return "Boolean";
      case "CheckBoxList":
      case "TreeList":
      case "List":
        return "[]";
      case "TextArea":
        return "Text";
      case "Html":
        return "Html";
      case "File":
        return "File";
      case "Files":
        return "Files";
      default:
        return "String";
    }
  }

  function guessUIType(meta) {
    switch (meta.datatype) {
      case "Date":
        return "Date";
      case "DateTime":
        return "DateTime";
      case "Boolean":
        return "CheckBox";
      case "Text":
        return "TextArea";
      case "Html":
        return "Html";
      case "File":
        return "File";
      case "Files":
        return "Files";
      default:
        if (meta.datatype.startsWith('[')) {
          return "List";
        } else {
          return "TextBox";
        }
    }
  }

  //重新定义元数据属性, 并从原有元数据定义抽取未定义属性
  // overwrite决定是否无视重新定义, 仍然用原来的的属性
  this.reDefineMetadata = function (newdef, overwrite) {
    if (typeof newdef == "string") {
      var ord = 0;
      newdef = newdef.split(",").map(m => ({
        name: m,
        order: ord++
      }));
    }
    for (var i in newdef) {
      var ele = newdef[i];
      if (typeof ele == "string") {
        ele = {
          name: ele,
          order: i
        }
      }
      for (var j in this.metadata) {
        var def = this.metadata[j];
        if (ele.name == def.name) {
          for (var p in def) {
            if (!(p in ele) || overwrite) {
              ele[p] = def[p];
            }
          }
          break;
        }
      }
    }
    return newdef;
  };

  //纵向合并不同的元数据集合，去重并与初始定义横向合并
  //如果两个定义name相同，则排在后面的元素优先
  //最后按itemorder进行排序
  this.concatDef = function () {
    var formMetadata = this.metadata;
    for (var i in arguments) {
      var arg = arguments[i];
      formMetadata = formMetadata.concat(arg);
    }
    formMetadata = formMetadata.filter(function (m) {
      return m.name;
    }); //排除name为空的异常配置项
    formMetadata = this.mergeDef(util.distinct(formMetadata, "name"));
    formMetadata.sort((m1, m2) => m1.order - m2order);
    this.metadata = this.formMetadata;
    return this;
  };
  //获取主键字段
  this.getPrimaryKeys = () => {
    var pkeys = this.metadata.filter(p => p.primarykey);
    return pkeys;
  };

  //获取排序位字段
  this.getSortOrders = () => {
    var sortOrders = this.metadata.filter(p => p.datatype == "SortOrder");
    if (!sortOrders.length) {
      sortOrders = this.metadata.filter(p => p.name == "ord");
    }
    return sortOrders;
  };

  this.getSortField = () => {
    var sf = this.getSortOrders();
    return sf.length ? sf[0].name : '';
  };

  //注册生成下拉列表数据的回调
  this.setOptionsCallback = (name, func) => {
    var meta = this.metadata.find(m => m.name == name);
    meta.optionsfunc = func;
  };

  //////////////////////////////选项操作//////////////////////////////////////

  //格式化选项列表
  this.updateOptions = (item) => {
    function createOptions(options) {
      return options.map(function (t) {
        if (typeof t == "string") {
          return {
            text: t,
            value: t
          };
        } else {
          return t;
        }
      });
    }

    var options = item.options;
    if (
      (!(options && options.length) || item.linkto) && //items为空或是需要联动的字段
      item.optionsfunc //求选项列表值的函数不为空
    ) {
      var result = item.optionsfunc(this.editBuffer, item) || [];
      if (result instanceof Array) {
        //直接返回数组
        item.options = createOptions(result);
      } else if (result.then) {
        //返回promise对象
        result.then(function (arr) {
          item.options = createOptions(arr);
        });
      } else {
        item.options = [];
      }
    } else if (options) {
      item.options = createOptions(options);
    }
    //在拥有linkto属性的meta没有options的情况下，直接用linkto指向的meta中的options.text赋值
    else if (item.linkto) {
      this.editBuffer[item.name] = this.getMeta(item.linkto)
        .options.find(opt => opt.value == this.editBuffer[item.linkto])
        .text;
    }

  };

  this.getMeta = name => this.metadata.find(m => m.name == name);
  this.getMetas = names => this.metadata.filter(m => names.includes(m.name));

  ///////////////////////////////////数据修改//////////////////////////////////////
  //获取随机最大排序位
  this.getMaxOrder = () => {
    var sortField = this.getSortField();
    if (!sortField) return null;
    var max = 0;
    this.products.forEach(row => {
      if (max < row[sortField]) max = row[sortField];
    });
    return max + 10 + Math.random() * 10;
  };

  //根据元数据定义创建新记录
  this.createProduct = function () {
    var obj = {};

    for (var i in this.metadata) {
      var d = this.metadata[i];
      if (d.uitype == "Operator") continue;
      if (!obj[d.name]) { //防止files字段被重新赋值
        obj[d.name] = d.value;
      }

      var valuefunc = d.valuefunc || d.name + "_value";
      //求由valuefunc定义的默认值
      var val = getDefaultValue(valuefunc, obj);
      if (val != null) {
        obj[d.name] = val;
      }
      if (!d.value && d.datatype && d.datatype.startsWith('[')) {
        obj[d.name] = []; // elementui的多选框在值不是数组时会出错
      }
    }
    var pk = this.getPrimaryKeys();
    if (pk.length == 1 && !obj[pk[0].name] && pk[0].datatype == "String") obj[pk[0].name] = util.guid();

    var ordField = this.getSortField();
    if (ordField && !obj[ordField]) {
      obj[ordField] = this.getMaxOrder();
    }
    return obj;
  };

  this.updateAllOptions = function (metas) {
    metas = metas || this.metadata;
    //准备下拉列表框的选项,这里大致假定一下uitype中有List的就是带下拉列表的元数据
    // 如DropdownList, List, CheckboxList
    this.metadata.filter(meta => meta.uitype == "DropdownList" || meta.uitype == "List" || meta.uitype == "CheckboxList")
      .forEach(meta => this.updateOptions(meta));
  };

  //根据选中的成果集合，生成对应元数据定义集合
  //合并多个对象为一个编辑缓冲对象,保留相同的公共属性，用以和表单绑定
  this.setSelection = function (list) {
    this.selection = list;
    var buffer = {};
    list = list || [];
    var p0 = (list.length > 0) ? list[0] : {};

    this.metadata.forEach(meta => {
      var multiVal = false;
      //当列表length<=1时不会循环
      for (var i = 1; i < list.length; i++) {
        if (!util.isEqual(p0[meta.name], list[i][meta.name])) {
          multiVal = true;
          break;
        }
      }
      //该属性对应的元数据打上是否有多个值的标记，用以在表单显示时多个值用<<多个值>>提示
      meta.multiValue = multiVal;

      if (!meta.multiValue) {
        buffer[meta.name] = p0[meta.name];
      } else {
        buffer[meta.name] = null;
        // meta.required = false; //避免多个值时，因多个值而留空的文本框验证出错
      }

      if (!buffer[meta.name] && meta.datatype && meta.datatype.startsWith('[')) {
        buffer[meta.name] = []; // elementui的多选框在值不是数组时会出错
      }
    });
    this.editBuffer = buffer;
    this.updateAllOptions();
    return this;
  };

  //对于已有的成果， 检查元数据项,要求非空的字段自动填上元数据中的默认值 
  this.check = function (obj) {
    for (var i in this.metadata) {
      var d = this.metadata[i];
      //对于必填项没有值的或需要每次都改写的，填上元数据定义的默认值 
      if ((!obj[d.name] && d.required) || (d.overwrite && d.innertag)) {
        obj[d.name] = d.value;
        if (d.valuefunc) {
          obj[d.name] = getDefaultValue(d.valuefunc, obj, this);
        }
      }
    }

    //删除元数据定义中没有的元数据
    //for (var i in obj) {
    //    if (!metadatas.find(function (m) { return m.name == i })) {
    //        delete obj[i];
    //    }
    //}
    return obj;
  };

  //计算是否有修改
  this.testChanged = () => {
    var changed = this.addedProducts.length +
      this.removedProducts.length +
      this.changedProducts.length;

    for (var d in this.details) {
      changed += this.details[d].testChanged();
    }
    this.changed = changed;
    return this.changed;
  };

  //维护增删改状态
  this.changeStatus = function (product, status /*removed/changed/added*/ ) {
    if (status == 'changed') {
      var idx = this.addedProducts.indexOf(product);
      //当对象是新增时，忽略修改标记
      if (idx >= 0) return;
      idx = this.changedProducts.indexOf(product);
      if (idx < 0) {
        this.changedProducts.push(product);
      }
    } else if (status == 'removed') {
      this._removeAllDetails(product);
      var idx = this.products.indexOf(product);
      if (idx >= 0) {
        this.products.splice(idx, 1);
      }
      idx = this.addedProducts.indexOf(product);
      if (idx >= 0) {
        this.addedProducts.splice(idx, 1);
        //新增的被删除，成果是新的，不需要再处理
        this.testChanged();
        return;
      }

      idx = this.changedProducts.indexOf(product);
      if (idx >= 0) {
        this.changedProducts.splice(idx, 1);
      }
      this.removedProducts.push(product);
    } else if (status == 'added') {
      var idx = this.products.indexOf(product);
      if (idx < 0) {
        this.products.push(product);
      }
      idx = this.addedProducts.indexOf(product);
      if (idx < 0) {
        this.addedProducts.push(product);
      }
      this._addAllDetails(product);
    }
    this.testChanged();
  };

  //子表数据的增删维护工作
  this.changeDetails = function (meta, oldObj) {
    var oldArr = oldObj[meta.name];
    var newArr = this.editBuffer[meta.name];

    if (meta.foreignkey) {
      var pkey = this.getPrimaryKeys()[0].name;
      newArr.forEach(newd => {
        //在多选编辑时，子表的外键因为合并成单一对象的原因可能为空
        //在此处再补上, 这里可能有问题
        newd[meta.foreignkey] = oldObj[pkey];
      });
    } else {
      throw '集合属性必须声明foreignkey, 以表明子集合中哪个属性代表外键';
    }

    var equal = (o, n) => {
      if (meta.valuekey) {
        return o[meta.valuekey] == n[meta.valuekey];
      } else {
        return o == n;
      }
    };

    //旧集合中在新集合中没有的就删除
    oldArr.filter(old => !newArr.find(n => equal(n, old))).forEach(old => {
      this.details[meta.name].changeStatus(old, 'removed');
    });

    //新集合中在旧集合中没有的就新增
    newArr.filter(newd => !oldArr.find(o => equal(o, newd))).forEach(newd => {
      this.details[meta.name].changeStatus(newd, 'added');
    });

  }

  this._removeAllDetails = product => {
    for (var i in this.details) {
      for (var j in product[i]) {
        this.details[i].changeStatus(product[i][j], 'removed');
      }
    }
  };

  this._addAllDetails = product => {
    for (var i in this.details) {
      for (var j in product[i]) {
        this.details[i].changeStatus(product[i][j], 'added');
      }
    }
  }

  function getDefaultValue(func, obj) {
    func = func.toLowerCase();
    if (appConfig.defaultValueFunc[func]) {
      return appConfig.defaultValueFunc[func](obj, API);
    }
    return null;
  }

  //在切换到其他结点前保存已修改的值
  //如果某属性对应某个子表，则比较子表前后的值，判断新增了哪些项，删除了哪些项
  this.acceptChanges = function () {
    for (var i in this.selection) {
      var changed = false;
      var tObj = this.selection[i];
      for (var j in this.metadata) {
        var d = this.metadata[j];
        //如果是多值保存，则缓冲区值为空时pass掉
        if (d.multiValue && util.isEmpty(this.editBuffer[d.name])) {
          continue;
        }

        //如果有子集合
        if (this.details[d.name]) {
          this.changeDetails(d, tObj);
        } else if (!util.isEqual(tObj[d.name], this.editBuffer[d.name])) {
          changed = true;
        }
        tObj[d.name] = this.editBuffer[d.name];
      }
      if (changed) this.changeStatus(tObj, "changed");
    }
    this.setSelection(this.selection);
  };

  //////////////////////////////数据校验////////////////////////////////////////////

  this.validateProduct = function (product, name, rule) {

    return new Promise((resolve, reject) => {
      if (rule.required && !product[name]) {
        reject({
          product,
          name,
          err: rule.message
        });
        return;
      }
      //todo:其他各类标准验证...
      if (rule.validator) {
        rule.validator(rule, product[name], (err) => {
          if (err) {
            reject({
              product,
              name,
              err
            });
          } else {
            resolve();
          }
        });
      } else {
        resolve();
      }
    });
  }

  //对所有改过的数据进行验证
  this.validate = function () {
    var addChanged = this.changedProducts.concat(this.addedProducts);

    if (addChanged.length == 0) {
      return Promise.resolve();
    }

    var validatePromises = [];

    addChanged.forEach(product => {
      for (var i in product) {
        var rule = this.rules[i];
        if (!rule) continue;
        for (var j in rule)
          validatePromises.push(this.validateProduct(product, i, rule[j]));
      }
    });
    return Promise.all(validatePromises);
  };

  //////////////////////////////数据保存////////////////////////////////////////////

  //生成要提交保存的数据
  this.createSaveData = () => {
    var details = [];
    for (var d in this.details) {
      details.push(this.details[d].createSaveData());
    }

    return {
      key: this.key,
      added: this._clearSaveData(this.addedProducts),
      removed: this._clearRemoveData(this.removedProducts),
      changed: this._clearSaveData(this.changedProducts),
      details
    }
  };

  //对于已删除记录，只传删除的主键的值（如果有的话），以节约带宽
  this._clearRemoveData = function (toRemove) {
    var pkeys = this.getPrimaryKeys();
    if (toRemove.length && pkeys.length) {
      return toRemove.map(p => {
        var r = {};
        pkeys.forEach(pk => r[pk.name] = p[pk.name]);
        return r;
      });
    }
    //找不到主键定义则原样返回
    return toRemove;
  };

  //清理将要保存的数据，只留下元数据明确定义的字段的值
  this._clearSaveData = function (toSave) {
    if (this.metadata.length) {
      return toSave.map(p => {
        var s = {};
        this.metadata.forEach(meta => {
          //将主表的明细数据清除，在details集合中单独处理
          if (meta.datatype.startsWith('[')) return;
          s[meta.name] = p[meta.name];
        });
        return s;
      });
    } else { //没有元数据则只能原样返回
      return toSave;
    }
  };
}
