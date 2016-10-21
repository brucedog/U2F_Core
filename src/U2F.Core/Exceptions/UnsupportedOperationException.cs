using System;

namespace U2F.Core.Exceptions
{
    public class UnsupportedOperationException : Exception
    {
        public UnsupportedOperationException(string errorWhenComputingSha, Exception noSuchAlgorithmException)
        {
            Console.WriteLine("Error computing sha:{0} No such algorithem exception:{1}", errorWhenComputingSha, noSuchAlgorithmException);
        }
    }
}
