using System;
using System.Linq;
using Moq;
using Org.BouncyCastle.Crypto;
using U2F.Core.Crypto;
using U2F.Core.Exceptions;
using Xunit;

namespace U2F.Core.UnitTests
{
    public class CryptoServiceTests
    {
        [Fact]
        public void CryptoServiceThrowsExceptionForFakeCert()
        {
            Mock<ICipherParameters> cert = new Mock<ICipherParameters>();
            CryptoService cryptoService = new CryptoService();

            Exception ex = Assert.Throws<U2fException>(() => cryptoService.CheckSignature(cert.Object, new byte[1], new byte[1]));

            Assert.Contains(Resources.SignatureError, ex.Message);
        }

        [Fact]
        public void CryptoServiceThrowsExceptionForEmptySignedBytes()
        {
            Mock<ICipherParameters> cert = new Mock<ICipherParameters>();
            CryptoService cryptoService = new CryptoService();

            Exception ex = Assert.Throws<ArgumentException>(() => cryptoService.CheckSignature(cert.Object, new byte[0], new byte[1]));

            Assert.Equal(Resources.InvalidArguments, ex.Message);
        }

        [Fact]
        public void CryptoServiceThrowsExceptionForEmptySignature()
        {
            Mock<ICipherParameters> cert = new Mock<ICipherParameters>();
            CryptoService cryptoService = new CryptoService();

            Exception ex = Assert.Throws<ArgumentException>(() => cryptoService.CheckSignature(cert.Object, new byte[1], new byte[0]));

            Assert.Equal(Resources.InvalidArguments, ex.Message);
        }

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
