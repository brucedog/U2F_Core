using NUnit.Framework;
using U2F.Core;

namespace U2F.Tests
{
    [TestFixture]
    public class CryptoServiceUnitTests
    {
        [TestCase]
        public void CryptoServiceConstructsProperly()
        {
            CryptoService cryptoService = new CryptoService();

            Assert.NotNull(cryptoService);
        }
    }
}
