using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Izm.Rumis.Infrastructure.Identity
{
    public interface IPasswordValidator
    {
        bool Validate(string password);
    }

    public class PasswordValidator : IPasswordValidator
    {
        public PasswordValidator(PasswordSettings settings)
        {
            this.settings = settings;
        }

        private readonly PasswordSettings settings;

        public bool Validate(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (password.Length < settings.MinLength || password.Length > settings.MaxLength)
                return false;

            if (settings.RequireUpper && !Regex.IsMatch(password, "[A-Z]"))
                return false;

            if (settings.RequireLower && !Regex.IsMatch(password, "[a-z]"))
                return false;

            if (settings.RequireDigit && !Regex.IsMatch(password, "[0-9]"))
                return false;

            if (settings.RequireSpecial)
            {
                var chars = settings.SpecialChars.ToCharArray();

                if (!password.Any(c => chars.Contains(c)))
                    return false;
            }

            return true;
        }
    }
}
