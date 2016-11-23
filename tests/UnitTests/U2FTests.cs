using U2F.Core.Models;
using U2F.Core.Utils;
using Xunit;

namespace U2F.Core.UnitTests
{    
    public class U2FTests
    {
        [Fact]
        public void U2F_FinishRegistration()
        {
            StartedRegistration startedRegistration = new StartedRegistration(TestConts.SERVER_CHALLENGE_REGISTER_BASE64, TestConts.APP_ID_ENROLL);
            RegisterResponse registerResponse = new RegisterResponse(TestConts.REGISTRATION_RESPONSE_DATA_BASE64, TestConts.CLIENT_DATA_REGISTER_BASE64);
            
            DeviceRegistration results = U2F.Core.Crypto.U2F.FinishRegistration(startedRegistration, registerResponse, TestConts.TRUSTED_DOMAINS);

            Assert.NotNull(results);
            Assert.NotNull(results.KeyHandle);
            Assert.NotNull(results.PublicKey);
            Assert.NotNull(results.GetAttestationCertificate());
        }

        [Fact]
        public void U2F_FinishRegistrationNoFacets()
        {
            StartedRegistration startedRegistration = new StartedRegistration(TestConts.SERVER_CHALLENGE_REGISTER_BASE64, TestConts.APP_ID_ENROLL);
            RegisterResponse registerResponse = new RegisterResponse(TestConts.REGISTRATION_RESPONSE_DATA_BASE64, TestConts.CLIENT_DATA_REGISTER_BASE64);

            var results = U2F.Core.Crypto.U2F.FinishRegistration(startedRegistration, registerResponse);

            Assert.NotNull(results);
            Assert.NotNull(results.KeyHandle);
            Assert.NotNull(results.PublicKey);
            Assert.NotNull(results.GetAttestationCertificate());
        }

        [Fact]
        public void U2F_StartRegistration()
        {
            var results = U2F.Core.Crypto.U2F.StartRegistration(TestConts.APP_ID_ENROLL);

            Assert.NotNull(results);
            Assert.NotNull(results.Challenge);
            Assert.NotNull(results.Version);
            Assert.Equal(results.AppId, TestConts.APP_ID_ENROLL);
        }

        [Fact]
        public void U2F_StartAuthentication()
        {
            RegisterResponse registerResponse = new RegisterResponse(TestConts.REGISTRATION_RESPONSE_DATA_BASE64, TestConts.CLIENT_DATA_REGISTER_BASE64);
            RawRegisterResponse rawAuthenticateResponse = RawRegisterResponse.FromBase64(registerResponse.RegistrationData);
            DeviceRegistration deviceRegistration = rawAuthenticateResponse.CreateDevice();

            var results = U2F.Core.Crypto.U2F.StartAuthentication(TestConts.APP_ID_ENROLL, deviceRegistration);

            Assert.NotNull(results);
            Assert.NotNull(results.AppId);
            Assert.NotNull(results.Challenge);
            Assert.NotNull(results.KeyHandle);
            Assert.NotNull(results.Version);
        }

        [Fact]
        public void U2F_FinishAuthentication()
        {
            StartedAuthentication startedAuthentication = new StartedAuthentication(
                TestConts.SERVER_CHALLENGE_SIGN_BASE64,
                TestConts.APP_SIGN_ID,
                TestConts.KEY_HANDLE_BASE64);

            AuthenticateResponse authenticateResponse = new AuthenticateResponse(
                TestConts.CLIENT_DATA_AUTHENTICATE_BASE64,
                TestConts.SIGN_RESPONSE_DATA_BASE64,
                TestConts.KEY_HANDLE_BASE64);


            DeviceRegistration deviceRegistration = new DeviceRegistration(TestConts.KEY_HANDLE_BASE64_BYTE, 
                TestConts.USER_PUBLIC_KEY_AUTHENTICATE_HEX,
                TestConts.ATTESTATION_CERTIFICATE.Base64StringToByteArray(), 
                0);

            uint orginalValue = deviceRegistration.Counter;

            U2F.Core.Crypto.U2F.FinishAuthentication(startedAuthentication, authenticateResponse, deviceRegistration);

            Assert.True(deviceRegistration.Counter != 0);
            Assert.NotEqual(orginalValue, deviceRegistration.Counter);
            Assert.Equal(orginalValue + 1, deviceRegistration.Counter);
        }
    }
}