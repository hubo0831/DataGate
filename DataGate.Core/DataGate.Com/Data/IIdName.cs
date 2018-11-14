namespace DataGate.Com
{
    /// <summary>
    /// 有Id和Name的实体类接口
    /// </summary>
    public interface IIdName<T> : IId<T>
    {
        string Name { get; set; }
    }
}