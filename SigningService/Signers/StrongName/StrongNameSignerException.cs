using System;

namespace SigningService.Signers.StrongName
{
    public class StrongNameSignerException : Exception
    {
        public StrongNameSignerException(string message) : base(message) { }
        public StrongNameSignerException(string format, params object[] args) : base(string.Format(format, args)) { }
    }
}
