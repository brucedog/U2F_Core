using System;

namespace U2F.Core.Exceptions
{
    public class U2fException : Exception
    {
        public U2fException(string message)
        {
            Console.WriteLine("U2f exception:{0}", message);
        }

        public U2fException(string errorWhenVerifyingSignature, InvalidKeySpecException invalidKeyException)
        {
            Console.WriteLine("Error verifying signature:{0} invalid key exception:{1}", errorWhenVerifyingSignature, invalidKeyException);
        }

        public U2fException(string couldNotParseUserPublicKey, Exception invalidKeyException)
        {
            Console.WriteLine("Could not parse:{0} invalid key exception:{1}", couldNotParseUserPublicKey,
                              invalidKeyException);
        }
    }
}
