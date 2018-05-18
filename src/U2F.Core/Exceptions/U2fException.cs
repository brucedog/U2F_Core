using System;

namespace U2F.Core.Exceptions
{
    public class U2fException : Exception
    {
        public U2fException(string message)
        {
            Console.WriteLine("U2f exception:{0}", message);
        }

        public U2fException(string message, InvalidKeySpecException invalidKeyException)
            : base(message, invalidKeyException?.InnerException)
        {
            Console.WriteLine("Error verifying signature:{0} invalid key exception:{1}", message, invalidKeyException);
        }

        public U2fException(string message, Exception invalidKeyException)
        : base(message, invalidKeyException?.InnerException)
        {
            Console.WriteLine("Could not parse:{0} invalid key exception:{1}", message, invalidKeyException);
        }
    }
}
