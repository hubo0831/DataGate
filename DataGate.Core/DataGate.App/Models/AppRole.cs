using DataGate.Com;

namespace DataGate.App.Models
{
    public class AppRole: IdName<string>
    {
        public string Remark { get; set; }

        public int Ord { get; set; }
    }
}
