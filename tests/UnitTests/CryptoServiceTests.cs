using System.Linq;
using U2F.Core.Crypto;
using Xunit;

namespace U2F.Core.UnitTests
{
    public class CryptoServiceTests
    {
        [Fact]
        public void CryptoServiceConstructsProperly()
        {
            CryptoService cryptoService = new CryptoService();

            Assert.NotNull(cryptoService);
        }

        [Fact]
        public void CryptoService2ChallengesShouldBeDifferent()
        {
            CryptoService generator = new CryptoService();
            byte[] firstChallenge = generator.GenerateChallenge();
            byte[] secondChallenge = generator.GenerateChallenge();

            Assert.True(firstChallenge.Length == secondChallenge.Length);
            Assert.False(firstChallenge.SequenceEqual(secondChallenge));
        }
    }
}
