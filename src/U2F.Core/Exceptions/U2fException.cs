using System;

namespace U2F.Core.Exceptions
{
    public class U2fException : Exception
    {
        public const string SignatureError = "Error when verifying signature";
        public const string ErrorDecodingPublicKey = "Error when decoding public key";
        public const string InvalidArguments = "The arguments passed the were not valid";

        public U2fException(string message, Exception innerException = null) : base(message, innerException)
        { }
    }
}
