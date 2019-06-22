using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataGate.Com
{
    /// <summary>
    /// DataTable的帮助类
    /// </summary>
    public static class DataTableHelper
    {
        /// <summary>
        /// copy content from source to destination row 
        /// they must be of the same type 
        /// </summary>
        /// <param name="objS">source reference to a row</param>
        /// <param name="objD">reference to a destination row</param>
        public static void CopyRow(this DataRow objS, DataRow objD)
        {
            int intCounter = 0;
            // Starts an edit operation on objD.
            objD.BeginEdit();

            // set destination row with the SourceRow.
            for (intCounter = 0; intCounter <= objS.ItemArray.GetUpperBound(0); intCounter++)
            {
                objD[intCounter] = objS[intCounter];
            }

            // Ends the edit occurring on destination row.
            objD.EndEdit();
        }

        /// <summary>
        /// copy content from source to destination row 
        /// they must be of the same type 
        /// </summary>
        /// <param name="objS">source reference to a row</param>
        /// <param name="objD">reference to a destination row</param>
        public static void CopyRowByFieldName(this DataRow objS, DataRow objD)
        {
            int intCounter = 0;
            int intCounter2 = 0;
            string strFieldName = "";

            // Starts an edit operation on objD.
            objD.BeginEdit();

            // set destination row with the SourceRow. 
            for (intCounter = 0; intCounter <= objS.ItemArray.GetUpperBound(0); intCounter++)
            {
                // it no need to check upper or lower 
                strFieldName = objS.Table.Columns[intCounter].ColumnName;

                //for each objD 
                for (intCounter2 = 0; intCounter2 <= objD.ItemArray.GetUpperBound(0); intCounter2++)
                {
                    if (objD.Table.Columns[intCounter2].ColumnName == strFieldName)
                    {
                        objD[intCounter2] = objS[strFieldName];
                        break;
                    }
                }
            }

            // Ends the edit occurring on destination row.
            objD.EndEdit();
        }

        /// <summary>
        /// copy content from souece datatable to destination data.
        /// they must be of the same type
        /// </summary>
        /// <param name="objS">source table</param>
        /// <param name="objD">destination table</param>
        public static void CopyDataTable(this DataTable objS, DataTable objD)
        {
            DataRow objRowD = null;

            foreach (DataRow objRowS in objS.Rows)
            {
                if (objRowS.RowState != DataRowState.Deleted && objRowS.RowState != DataRowState.Detached)
                {
                    objRowD = objD.NewRow();
                    DataTableHelper.CopyRow(objRowS, objRowD);

                    // add new row
                    objD.Rows.Add(objRowD);
                }
            }
        }

        /// <summary>
        /// copy content from souece datatable to destination data.
        /// they must be of the same type
        /// </summary>
        /// <param name="objSource">source table</param>
        /// <param name="objDestination">destination table</param>
        /// <param name="sourceFilter">filter string for source table</param>
        public static void CopyDataTable(this DataTable objSource, DataTable objDestination, string sourceFilter)
        {
            DataView objS = new DataView(objSource);
            DataTable objD = (DataTable)objDestination;
            DataRow objRowD = null;

            objS.RowFilter = sourceFilter;
            foreach (DataRowView objRowS in objS)
            {
                if (objRowS.Row.RowState != DataRowState.Deleted && objRowS.Row.RowState != DataRowState.Detached)
                {
                    objRowD = objD.NewRow();
                    DataTableHelper.CopyRow(objRowS.Row, objRowD);

                    // add new row
                    objD.Rows.Add(objRowD);
                }
            }
        }

        /// <summary>
        /// copy content from souece datatable to destination data.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="objDestination"></param>
        public static void CopyDataTableByFieldName(DataTable sourceTable, DataTable destTable)
        {
            DataRow objRowD = null;

            foreach (DataRow objRowS in sourceTable.Rows)
            {
                if (objRowS.RowState != DataRowState.Deleted && objRowS.RowState != DataRowState.Detached)
                {
                    objRowD = destTable.NewRow();
                    DataTableHelper.CopyRowByFieldName(objRowS, objRowD);

                    // add new row
                    destTable.Rows.Add(objRowD);
                }
            }
        }

        /// <summary>
        /// 将源记录合并到目标行上。（适用于将多条记录合并到一条记录上）
        /// </summary>
        /// <param name="sourceRow">源记录</param>
        /// <param name="mergeRow">目标行</param>
        public static void MergeRecord(DataRow sourceRow, DataRow mergeRow)
        {
            if (sourceRow == null)
                return;

            // 行是否可用
            if (DataTableHelper.RowIsAvailable(sourceRow) && DataTableHelper.RowIsAvailable(mergeRow))
            {
                // 得到记录的表信息。
                DataTable sourceTable = sourceRow.Table;
                DataTable mergeTable = mergeRow.Table;

                // 将数据源的信息拷贝到目标行上。
                foreach (DataColumn col in sourceTable.Columns)
                {
                    // 判断列是否存在。
                    if (mergeTable.Columns.Contains(col.ColumnName))
                    {
                        // 赋值
                        mergeRow[col.ColumnName] = sourceRow[col.ColumnName];
                    }
                }
            }
        }
        /// <summary>
        /// 将源表的数据合并到目标表的指定行上。（适用于将多条记录合并到一条记录上）
        /// </summary>
        /// <param name="sourceTable">源表</param>
        /// <param name="mergeTable">目标表</param>
        /// <param name="rowIndex">需合并的行索引（从0开始）</param>
        public static void MergeRecord(DataTable sourceTable, DataTable mergeTable, int rowIndex)
        {
            // 判断传入的行索引是否有效。
            if (sourceTable.Rows.Count >= (rowIndex + 1) && mergeTable.Rows.Count >= (rowIndex + 1))
            {
                // 得到待合并的行
                DataRow sourceRow = sourceTable.Rows[rowIndex];   // 源记录
                DataRow mergeRow = mergeTable.Rows[rowIndex];    // 目标行

                // 执行行合并操作。
                DataTableHelper.MergeRecord(sourceRow, mergeRow);
            }
        }
        /// <summary>
        /// 将源表的结构合并到目标表上。（适用于将多表结构合并到一个表中）
        /// </summary>
        /// <param name="sourceTable">源表</param>
        /// <param name="mergeTable">目标表</param>
        public static void MergeTableStructure(DataTable sourceTable, DataTable mergeTable)
        {
            // 配置返回行的表结构
            foreach (DataColumn col in sourceTable.Columns)
            {
                // 判断此列是否已存在.
                if (mergeTable.Columns.Contains(col.ColumnName))
                {
                    continue;
                }

                // 将新列插入到表中。
                DataColumn newCol = mergeTable.Columns.Add(col.ColumnName, col.DataType);
                newCol.MaxLength = col.MaxLength;
            }
        }
        /// <summary>
        /// 得到行集合中指定字段的值。
        /// </summary>
        /// <param name="sourceRow">待取值的集合</param>
        /// <param name="fieldName">目标字段名</param>
        /// <returns>返回字段值的拼接串，以逗号分隔</returns>
        public static string GetRecordValues(DataRow[] sourceRow, string fieldName)
        {
            string fieldValues = "";

            foreach (DataRow rowItem in sourceRow)
            {
                if (!DataTableHelper.RowIsAvailable(rowItem))
                    continue;

                if (rowItem[fieldName] == DBNull.Value)
                    continue;

                // 将需要取出的字段值拼接起来。
                fieldValues += rowItem[fieldName] + ",";
            }

            // 返回
            return fieldValues == "" ? "" : fieldValues.Substring(0, fieldValues.Length - 1);
        }
        /// <summary>
        /// 得到行集合中指定字段的值（以逗号分隔）。
        /// </summary>
        /// <param name="sourceRow">待取值的集合</param>
        /// <param name="fieldName">目标字段名</param>
        /// <returns>返回字段值的拼接串，以逗号分隔</returns>
        public static string GetRecordValues(DataRowCollection sourceRow, string fieldName)
        {
            string fieldValues = "";

            foreach (DataRow rowItem in sourceRow)
            {
                if (!DataTableHelper.RowIsAvailable(rowItem))
                    continue;

                if (rowItem[fieldName] == DBNull.Value)
                    continue;

                // 将需要取出的字段值拼接起来。
                fieldValues += rowItem[fieldName] + ",";
            }

            // 返回
            return fieldValues == "" ? "" : fieldValues.Substring(0, fieldValues.Length - 1);
        }
        /// <summary>
        /// 得到行集合中指定字段的值。
        /// </summary>
        /// <param name="sourceTable">待取值的集合</param>
        /// <param name="fieldName">目标字段名</param>
        /// <param name="splitChar">分隔符</param>
        /// <returns>返回字段值的拼接串</returns>
        public static string GetRecordValues(DataTable sourceTable, string fieldName, string splitChar)
        {
            string fieldValues = "";

            foreach (DataRow rowItem in sourceTable.Rows)
            {
                if (!DataTableHelper.RowIsAvailable(rowItem))
                    continue;

                if (rowItem[fieldName] == DBNull.Value)
                    continue;

                // 将需要取出的字段值拼接起来。
                fieldValues += rowItem[fieldName] + splitChar;
            }

            // 返回
            return fieldValues == "" ? "" : fieldValues.Substring(0, fieldValues.Length - 1);
        }

        /// <summary>
        /// find row in the datatable
        /// </summary>
        /// <param name="table">table</param>
        /// <param name="filter">filter</param>
        /// <returns></returns>
        public static DataRow FindRow(this System.Data.DataTable table, string filter)
        {
            return table.FindRow(filter, DataViewRowState.CurrentRows);
        }
        /// <summary>
        /// find row in the datatable
        /// </summary>
        /// <param name="table">table</param>
        /// <param name="filter">filter</param>
        /// <param name="rowVersion">row data version</param>
        /// <returns></returns>
        public static DataRow FindRow(this System.Data.DataTable table, string filter, DataViewRowState rowVersion)
        {
            if (table == null || table.Rows.Count <= 0)
            {
                return null;
            }

            System.Data.DataRow objRowReturn = null;
            System.Data.DataRow[] objRow = table.Select(filter, null, rowVersion);

            if (((objRow != null)) && objRow.Length > 0)
            {
                objRowReturn = (System.Data.DataRow)objRow[0];
            }

            return objRowReturn;
        }

        /// <summary>
        /// find row in the dataview
        /// </summary>
        /// <param name="dataView">dataview</param>
        /// <param name="key">key value</param>
        /// <param name="sort">sort</param>
        /// <returns></returns>
        public static DataRowView FindRow(this System.Data.DataView dataView, object key, string sort)
        {
            if (dataView == null || dataView.Count <= 0)
            {
                return null;
            }

            System.Data.DataRowView[] objRow = null;
            System.Data.DataRowView objRowReturn = null;
            if (!string.IsNullOrEmpty(sort.Trim()))
            {
                dataView.Sort = sort;
            }

            // Sort can't is empty 
            if (string.IsNullOrEmpty(dataView.Sort.Trim()))
            {
                return null;
            }

            objRow = dataView.FindRows(key);
            if (((objRow != null)) && objRow.Length > 0)
            {
                objRowReturn = objRow[0];
            }

            return objRowReturn;
        }

        /// <summary>
        /// find rows in the dataview
        /// </summary>
        /// <param name="dataView">dataview</param>
        /// <param name="objKey">key value</param>
        /// <param name="strSort">sort</param>
        /// <returns></returns>
        public static DataRowView FindRow(this System.Data.DataView dataView, object[] objKey, string strSort)
        {
            if (dataView == null || dataView.Count <= 0)
            {
                return null;
            }


            System.Data.DataRowView[] objRow = null;
            System.Data.DataRowView objRowReturn = null;

            if (!string.IsNullOrEmpty(strSort.Trim()))
            {
                dataView.Sort = strSort;
            }

            // Sort can't is empty 
            if (string.IsNullOrEmpty(dataView.Sort.Trim()))
            {
                return null;
            }

            objRow = dataView.FindRows(objKey);
            if (((objRow != null)) && objRow.Length > 0)
            {
                objRowReturn = objRow[0];
            }

            return objRowReturn;
        }

        /// <summary>
        /// find rows in the datatable
        /// </summary>
        /// <param name="objTable">table</param>
        /// <param name="strFilter">filter</param>
        /// <returns></returns>
        public static DataRow[] FindRows(this System.Data.DataTable objTable, string strFilter)
        {
            // Parameters : 
            // 
            // Descriptions : 
            //  find rows in the datatable
            // 
            // Return : 
            // Author/Date : 2008-10-08, Cuttlebone

            return objTable.FindRows(strFilter, DataViewRowState.CurrentRows);
        }

        /// <summary>
        /// find rows in the datatable
        /// </summary>
        /// <param name="objTable">table</param>
        /// <param name="strFilter">filter</param>
        /// <param name="rowState">data row state</param>
        /// <returns></returns>
        public static DataRow[] FindRows(this System.Data.DataTable objTable, string strFilter, DataViewRowState rowState)
        {
            if (objTable == null || objTable.Rows.Count <= 0)
            {
                return null;
            }

            return objTable.Select(strFilter, "", rowState);
        }

        /// <summary>
        /// find rows in the dataview
        /// </summary>
        /// <param name="objDataView">dataview</param>
        /// <param name="objKey">key value</param>
        /// <param name="strSort">sort</param>
        /// <returns></returns>
        public static DataRowView[] FindRows(this System.Data.DataView objDataView, object objKey, string strSort)
        {
            if (objDataView == null || objDataView.Count <= 0)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(strSort.Trim()))
            {
                objDataView.Sort = strSort;
            }

            // Sort can't is empty 
            if (string.IsNullOrEmpty(objDataView.Sort.Trim()))
            {
                return null;
            }

            return objDataView.FindRows(objKey);
        }

        /// <summary>
        /// find rows in the dataview
        /// </summary>
        /// <param name="objDataView">dataview</param>
        /// <param name="objKey">key value</param>
        /// <param name="strSort">sort</param>
        /// <returns></returns>
        public static DataRowView[] FindRows(this System.Data.DataView objDataView, object[] objKey, string strSort)
        {
            if (objDataView == null || objDataView.Count <= 0)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(strSort.Trim()))
            {
                objDataView.Sort = strSort;
            }

            // Sort can't is empty 
            if (string.IsNullOrEmpty(objDataView.Sort.Trim()))
            {
                return null;
            }

            return objDataView.FindRows(objKey);
        }

        /// <summary>
        /// indicate the datarow whether is available. 
        /// </summary>
        /// <param name="objRow"></param>
        /// <returns></returns>
        public static bool RowIsAvailable(this DataRow objRow)
        {
            if (objRow == null)
                return false;

            return (objRow.RowState != DataRowState.Deleted && objRow.RowState != DataRowState.Detached);
        }

        /// <summary>
        /// remove duplicate record for the destination source.
        /// </summary>
        /// <returns></returns>
        public static void RemoveDuplicate(this DataTable objTableS, DataTable objTableD, string[] objPrimaryKey)
        {
            DataView objTableView = new DataView(objTableD);

            if (objTableS != null && objTableD != null && objPrimaryKey.Length > 0)
            {
                object[] objKeyValue = new object[objPrimaryKey.Length];
                DataRowView[] objRowDup = null;

                foreach (DataRow objRowS in objTableS.Rows)
                {
                    // check row state whether the row is available.
                    if (DataTableHelper.RowIsAvailable(objRowS))
                    {
                        // set key value.
                        for (int intIndex = 0; intIndex <= objPrimaryKey.GetUpperBound(0); intIndex++)
                        {
                            objKeyValue[intIndex] = objRowS[objPrimaryKey[intIndex]];
                        }

                        // find destination table.
                        objRowDup = objTableView.FindRows(objKeyValue, objPrimaryKey[0]);
                        if (objRowDup != null)
                        {
                            for (int intIndex = 0; intIndex <= objRowDup.GetUpperBound(0); intIndex++)
                            {
                                // delete duplicate row.
                                objRowDup[intIndex].Delete();
                            }
                        }
                    }
                }
            }

            // submit changes and reutrn the table.
            objTableView.Table.AcceptChanges();
        }

        /// <summary>
        ///  将数据表中的记录全部设置Delete状态。
        /// </summary>
        /// <param name="objTable">目标数据表</param>
        public static int RemoveAll(this DataTable objTable)
        {
            return objTable.RemoveAll("");
        }

        /// <summary>
        /// 将数据表中相匹配的记录全部设置Delete状态。
        /// </summary>
        /// <param name="objTable">目标数据表</param>
        /// <param name="rowFilter">删除条件</param>
        /// <returns></returns>
        public static int RemoveAll(this DataTable objTable, string rowFilter)
        {
            int intCounter = 0;
            if (objTable == null)
            {
                return 0;
            }

            DataRow[] deleteList = objTable.FindRows(rowFilter);
            if (deleteList == null)
                return 0;

            for (int i = 0; i < deleteList.Length; i++)
            {
                deleteList[i].Delete();
                intCounter++;
            }

            return intCounter;
        }

        /// <summary>
        /// 从Xml获取DataTable
        /// </summary>
        /// <param name="xmlFile">Xml文件名</param>
        /// <returns>DataTable</returns>
        public static DataTable XmlToDataTable(string xmlFile)
        {
            DataSet ds = new DataSet();

            ds.ReadXml(xmlFile);

            DataTable dt = ds.Tables.Count == 0 ? null : ds.Tables[0];
            return dt;
        }

        /// <summary>
        /// 将DataTable保存到Xml文件
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="xmlFile">Xml文件名</param>
        public static void ToXml(this DataTable dt, String xmlFile)
        {
            DataSet ds = dt.DataSet;
            if (ds == null)
            {
                ds = new DataSet();
                ds.Tables.Add(dt);
            }
            ds.WriteXml(xmlFile);
        }

        #region datarow和datareader转换成简单类型
        /// <summary>
        /// 将reader[Key]转换为字符串
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ToStr(this IDataReader reader, string key)
        {
            return CommOp.ToStr(reader[key]);
        }

        /// <summary>
        /// 将reader[index]转换为字符串
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string ToStr(this IDataReader reader, int index)
        {
            return CommOp.ToStr(reader[index]);
        }

        /// <summary>
        /// 将reader[Key]转换为Int32
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int ToInt(this IDataReader reader, string key)
        {
            return CommOp.ToInt(reader[key]);
        }

        /// <summary>
        /// 将reader[index]转换为Int32
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int ToInt(this IDataReader reader, int index)
        {
            return CommOp.ToInt(reader[index]);
        }

        /// <summary>
        /// 将DataRow中的值转换成int?
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int? ToIntNull(this DataRow dr, string key)
        {
            return CommOp.ToIntNull(dr[key]);
        }

        /// <summary>
        /// 将DataRow中的值转换成int?
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int? ToIntNull(this DataRow dr, int index)
        {
            return CommOp.ToIntNull(dr[index]);
        }

        /// <summary>
        /// 将DataRow中的值转换成Int32
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int ToInt(this DataRow dr, string key)
        {
            return CommOp.ToInt(dr[key]);
        }

        /// <summary>
        /// 将DataRow中的值转换成Int32
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long ToLong(this DataRow dr, string key)
        {
            return CommOp.ToLong(dr[key]);
        }

        /// <summary>
        /// 将DataRow中的值转换成Int64
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static long ToLong(this DataRow dr, int index)
        {
            return CommOp.ToLong(dr[index]);
        }

        /// <summary>
        /// 将DataRow中的值转换成int?
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static long? ToLongNull(this DataRow dr, int index)
        {
            return CommOp.ToLongNull(dr[index]);
        }

        /// 将DataRow中的值转换成Int32
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long? ToLongNull(this DataRow dr, string key)
        {
            return CommOp.ToLongNull(dr[key]);
        }

        /// <summary>
        /// 将DataRow中的值转换成double
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static double ToDouble(this DataRow dr, int index)
        {
            return CommOp.ToDouble(dr[index]);
        }

        /// <summary>
        /// 将DataRow中的值转换成double
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double ToDouble(this DataRow dr, string key)
        {
            return CommOp.ToDouble(dr[key]);
        }

        /// <summary>
        /// 将DataRow中的值转换成Int32
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int ToInt(this DataRow dr, int index)
        {
            return CommOp.ToInt(dr[index]);
        }

        /// <summary>
        /// 将DataRow中的值转换成去除两边空格后的String,不会返回null
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ToStr(this DataRow dr, string key)
        {
            return CommOp.ToStr(dr[key]);
        }

        public static string ToStr(this DataRow dr, int index)
        {
            return CommOp.ToStr(dr[index]);
        }

        /// <summary>
        /// 将DataRow中的值转换成bool类型
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ToBool(this DataRow dr, string key)
        {
            return CommOp.ToBool(dr[key]);
        }

        /// <summary>
        /// 将DataRow中的值转换成bool类型
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool ToBool(this DataRow dr, int index)
        {
            return CommOp.ToBool(dr[index]);
        }

        /// <summary>
        /// 将DataRow中的值转换成bool?类型
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool? ToBoolNull(this DataRow dr, string key)
        {
            return CommOp.ToBoolNull(dr[key]);
        }

        /// <summary>
        /// 将DataRow中的值转换成bool?类型
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool? ToBoolNull(this DataRow dr, int index)
        {
            return CommOp.ToBoolNull(dr[index]);
        }

        /// <summary>
        /// 将DataRow中的值转换为特定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T ToValue<T>(this DataRow dr, string key)
        {
            return CommOp.HackType<T>(dr[key]);
        }

        /// <summary>
        /// 将DataRow中的值转换为特定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T ToValue<T>(this DataRow dr, int index)
        {
            return CommOp.HackType<T>(dr[index]);
        }

        #endregion

        #region DataTable和实体类互转
        //https://www.cnblogs.com/dyfzwj/archive/2011/04/16/2017916.html
        /// <summary>  
        /// 填充对象列表：用DataTable填充实体类列表
        /// </summary> 
        /// <returns>List[T]</returns>
        public static List<T> ToList<T>(this DataTable dt) where T : new()
        {
            if (dt == null)
            {
                return null;
            }
            List<T> modelList = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                //T model = (T)Activator.CreateInstance(typeof(T));  
                T model = new T();
                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    PropertyInfo propertyInfo = model.GetType().GetProperty(dr.Table.Columns[i].ColumnName);
                    if (propertyInfo != null && dr[i] != DBNull.Value)
                        propertyInfo.SetValue(model, dr[i], null);
                }

                modelList.Add(model);
            }
            return modelList;
        }

        /// <summary>  
        /// 填充对象：用DataRow填充实体类
        /// </summary>  
        /// <returns>T对象</returns>
        public static T ToModel<T>(this DataRow dr) where T : new()
        {
            if (dr == null)
            {
                return default(T);
            }
            //T model = (T)Activator.CreateInstance(typeof(T));  
            T model = new T();
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                PropertyInfo propertyInfo = model.GetType().GetProperty(dr.Table.Columns[i].ColumnName);
                if (propertyInfo != null && dr[i] != DBNull.Value)
                    propertyInfo.SetValue(model, dr[i], null);
            }
            return model;
        }

        /// <summary>
        /// 实体类List转换成DataTable
        /// </summary>
        /// <param name="modelList">实体类列表</param>
        /// <returns>DataTable</returns>
        public static DataTable ToTable<T>(this IList<T> modelList)
        {
            DataTable dt = CreateTable(typeof(T));
            foreach (T model in modelList)
            {
                DataRow dataRow = dt.NewRow();
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    dataRow[propertyInfo.Name] = propertyInfo.GetValue(model, null);
                }
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// 根据实体类得到表结构
        /// </summary>
        /// <param name="type">实体类</param>
        /// <returns>空的DataTable</returns>
        private static DataTable CreateTable(Type type)
        {
            DataTable dataTable = new DataTable(type.Name);
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                dataTable.Columns.Add(new DataColumn(propertyInfo.Name, propertyInfo.PropertyType));
            }
            return dataTable;
        }
        #endregion
    }
}