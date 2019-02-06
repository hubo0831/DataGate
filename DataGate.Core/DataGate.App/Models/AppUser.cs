using DataGate.Com;

namespace DataGate.App.Models
{
    public class AppUser : IdName<string>
    {
        public string Account { get; set; }
        public string Email { get; set; }

        public string Tel { get; set; }

        public string Password { get; set; }

        public string PasswordSalt { get; set; }

        public int FailedTry { get; set; }
    }
}
