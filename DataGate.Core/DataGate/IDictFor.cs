namespace DataGate
{
    /// <summary>
    /// 各版本数据字典提供程序接口
    /// </summary>
    public interface IDictFor
    {
        /// <summary>
        /// 数据字典查询SQL语句，必须对照ColumnMeta类定义返回统一格式的列
        /// </summary>
        string DictSql { get; }
    }
}