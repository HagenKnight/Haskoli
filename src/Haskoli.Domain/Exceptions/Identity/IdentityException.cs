using System.Globalization;

namespace Haskoli.Domain.Exceptions.Identity
{
    public class IdentityException : Exception
    {
        public IdentityException() : base() { }
        public IdentityException(string message) : base(message) { }
        public IdentityException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args)) { }
    }
}
