using System;

namespace U2F.Core.Exceptions
{
    public class InvalidKeySpecException : Exception
    {
        public InvalidKeySpecException(string message)
        {
            Console.WriteLine("Key threw exception: {0}", message);
        }
    }
}