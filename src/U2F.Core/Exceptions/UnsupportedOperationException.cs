using System;

namespace U2F.Core.Exceptions
{
    public class UnsupportedOperationException : Exception
    {
        public const string Sha256Exception = "Error when computing SHA-256";

        public UnsupportedOperationException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
