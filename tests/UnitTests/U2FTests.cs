using U2F.Core.Crypto;
using U2F.Core.Crypto.BouncyCastle;
using U2F.Core.Models;
using U2F.Core.Utils;
using Xunit;

namespace U2F.Core.UnitTests
{
    public class U2FTests
    {
        [Theory, ClassData(typeof(CryptoServices))]
        public void U2F_FinishRegistration(ICryptoService crypto)
        {
            Crypto.U2F.Crypto = crypto;

            StartedRegistration startedRegistration =
                new StartedRegistration(TestConstants.SERVER_CHALLENGE_REGISTER_BASE64, TestConstants.APP_ID_ENROLL);
            RegisterResponse registerResponse = new RegisterResponse(TestConstants.REGISTRATION_RESPONSE_DATA_BASE64,
                TestConstants.CLIENT_DATA_REGISTER_BASE64);

            DeviceRegistration results =
                U2F.Core.Crypto.U2F.FinishRegistration(startedRegistration, registerResponse,
                    TestConstants.TRUSTED_DOMAINS);

            Assert.NotNull(results);
            Assert.NotNull(results.KeyHandle);
            Assert.NotNull(results.PublicKey);
            Assert.NotNull(results.GetAttestationCertificate());
        }
        
        [Theory, ClassData(typeof(CryptoServices))]
        public void U2F_FinishRegistrationNoFacets(ICryptoService crypto)
        {
            Crypto.U2F.Crypto = crypto;

            StartedRegistration startedRegistration =
                new StartedRegistration(TestConstants.SERVER_CHALLENGE_REGISTER_BASE64, TestConstants.APP_ID_ENROLL);
            RegisterResponse registerResponse = new RegisterResponse(TestConstants.REGISTRATION_RESPONSE_DATA_BASE64,
                TestConstants.CLIENT_DATA_REGISTER_BASE64);

            var results = U2F.Core.Crypto.U2F.FinishRegistration(startedRegistration, registerResponse);

            Assert.NotNull(results);
            Assert.NotNull(results.KeyHandle);
            Assert.NotNull(results.PublicKey);
            Assert.NotNull(results.GetAttestationCertificate());
        }
        
        [Theory, ClassData(typeof(CryptoServices))]
        public void U2F_StartRegistration(ICryptoService crypto)
        {
            Crypto.U2F.Crypto = crypto;

            var results = U2F.Core.Crypto.U2F.StartRegistration(TestConstants.APP_ID_ENROLL);

            Assert.NotNull(results);
            Assert.NotNull(results.Challenge);
            Assert.NotNull(results.Version);
            Assert.Equal(results.AppId, TestConstants.APP_ID_ENROLL);
        }
        
        [Theory, ClassData(typeof(CryptoServices))]
        public void U2F_StartAuthentication(ICryptoService crypto)
        {
            Crypto.U2F.Crypto = crypto;

            RegisterResponse registerResponse = new RegisterResponse(TestConstants.REGISTRATION_RESPONSE_DATA_BASE64,
                TestConstants.CLIENT_DATA_REGISTER_BASE64);
            RawRegisterResponse rawAuthenticateResponse =
                RawRegisterResponse.FromBase64(registerResponse.RegistrationData);
            DeviceRegistration deviceRegistration = rawAuthenticateResponse.CreateDevice();

            var results = U2F.Core.Crypto.U2F.StartAuthentication(TestConstants.APP_ID_ENROLL, deviceRegistration);

            Assert.NotNull(results);
            Assert.NotNull(results.AppId);
            Assert.NotNull(results.Challenge);
            Assert.NotNull(results.KeyHandle);
            Assert.NotNull(results.Version);
        }
        
        [Theory, ClassData(typeof(CryptoServices))]
        public void U2F_FinishAuthentication(ICryptoService crypto)
        {
            Crypto.U2F.Crypto = crypto;

            StartedAuthentication startedAuthentication = new StartedAuthentication(
                TestConstants.SERVER_CHALLENGE_SIGN_BASE64,
                TestConstants.APP_SIGN_ID,
                TestConstants.KEY_HANDLE_BASE64);

            AuthenticateResponse authenticateResponse = new AuthenticateResponse(
                TestConstants.CLIENT_DATA_AUTHENTICATE_BASE64,
                TestConstants.SIGN_RESPONSE_DATA_BASE64,
                TestConstants.KEY_HANDLE_BASE64);


            DeviceRegistration deviceRegistration = new DeviceRegistration(TestConstants.KEY_HANDLE_BASE64_BYTE,
                TestConstants.USER_PUBLIC_KEY_AUTHENTICATE_HEX,
                TestConstants.ATTESTATION_CERTIFICATE.Base64StringToByteArray(),
                0);

            uint orginalValue = deviceRegistration.Counter;

            U2F.Core.Crypto.U2F.FinishAuthentication(startedAuthentication, authenticateResponse, deviceRegistration);

            Assert.True(deviceRegistration.Counter != 0);
            Assert.NotEqual(orginalValue, deviceRegistration.Counter);
            Assert.Equal(orginalValue + 1, deviceRegistration.Counter);
        }
    }
}