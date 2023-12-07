namespace Izm.Rumis.Infrastructure.Identity
{
    public class PasswordSettings
    {
        public const string DefaultSpecialChars = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        public bool RequireUpper { get; set; }
        public bool RequireLower { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireSpecial { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string SpecialChars { get; set; } = DefaultSpecialChars;
    }
}
