using System;
using System.Linq;
using Moq;
using U2F.Core.Crypto;
using U2F.Core.Exceptions;
using Xunit;

namespace U2F.Core.UnitTests
{
    public class CryptoServiceTests
    {
        [Theory, ClassData(typeof(CryptoServices))]
        public void CryptoService2ChallengesShouldBeDifferent(ICryptoService generator)
        {
            byte[] firstChallenge = generator.GenerateChallenge();
            byte[] secondChallenge = generator.GenerateChallenge();

            Assert.True(firstChallenge.Length == secondChallenge.Length);
            Assert.False(firstChallenge.SequenceEqual(secondChallenge));
        }
    }
}
