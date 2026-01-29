using System.Net.Mail;

namespace NightBlade.MMO
{
    public static partial class EmailExtensions
    {
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                email.Trim().EndsWith("."))
            {
                return false;
            }

            try
            {
                MailAddress addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}







