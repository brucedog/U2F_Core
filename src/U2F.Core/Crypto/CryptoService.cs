using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using U2F.Core.Exceptions;
using U2F.Core.Utils;

namespace U2F.Core.Crypto
{
    public sealed class CryptoService : IDisposable, ICryptoService
    {
        //private readonly ECCurve _elipiticCurve = ECCurve.CreateFromFriendlyName("secP256r1");
        private SHA256 _sha256 = SHA256.Create();
        private RandomNumberGenerator _randomNumberGenerator;
        private const string SignatureError = "Error when verifying signature";
        private const string ErrorDecodingPublicKey = "Error when decoding public key";
        private const string InvalidArgumentException = "The arguments passed the were not valid";
        private const string Sha256Exception = "Error when computing SHA-256";

        public CryptoService()
        {
            _sha256.Initialize();
            // TODO i should be able to get to the sha256 
            //_randomNumberGenerator = RandomNumberGenerator.Create("Sha256"); 
            _randomNumberGenerator = RandomNumberGenerator.Create();
        }

        public CngKey EncodePublicKey(byte[] rawKey)
        {
            try
            {
                return ConvertPublicKey(rawKey);
            }
            catch (Exception exception)
            {
                throw new U2fException(ErrorDecodingPublicKey, exception);
            }
        }

        public CngKey DecodePublicKey(X509Certificate2 certificate)
        {
            try
            {
                byte[] rawData = certificate.PublicKey.EncodedKeyValue.RawData;

                return ConvertPublicKey(rawData);
            }
            catch (InvalidKeySpecException exception)
            {
                throw new U2fException(Resources.SignatureError, exception);
            }
            catch (Exception exception)
            {
                throw new U2fException(ErrorDecodingPublicKey, exception);
            }
        }

        public bool CheckSignature(CngKey certificate, byte[] signedBytes, byte[] signature)
        {
            try
            {
                if (certificate == null
                    || signedBytes == null || signedBytes.Length == 0
                    || signature == null || signature.Length == 0)
                    throw new ArgumentException(InvalidArgumentException);

                bool result = VerifySignedBytesAgainstSignature(certificate, signedBytes, signature);

                if(!result)
                    throw new U2fException(SignatureError);

                return true;
            }
            catch (InvalidKeySpecException exception)
            {
                throw new U2fException(SignatureError, exception);
            }
            catch (Exception exception)
            {
                throw new U2fException(SignatureError, exception);
            }
        }

        public bool CheckSignature(X509Certificate2 attestationCertificate, byte[] signedBytes, byte[] signature)
        {
            try
            {
                if (attestationCertificate == null
                    || signedBytes == null || signedBytes.Length == 0
                    || signature == null || signature.Length == 0)
                    throw new ArgumentException(InvalidArgumentException);

                CngKey publicKey = DecodePublicKey(attestationCertificate);

                bool result = VerifySignedBytesAgainstSignature(publicKey, signedBytes, signature);

                if (!result)
                    throw new U2fException(SignatureError);

                return true;
            }
            catch (InvalidKeySpecException exception)
            {
                throw new U2fException(SignatureError, exception);
            }
            catch (Exception exception)
            {
                throw new U2fException(SignatureError, exception);
            }
        }

        public byte[] GenerateChallenge()
        {
            byte[] randomBytes = new byte[32];
            _randomNumberGenerator.GetBytes(randomBytes);

            return randomBytes;
        }

        public byte[] Hash(string stringToHash)
        {
            return Hash(stringToHash.GetBytes());
        }

        public byte[] Hash(byte[] bytes)
        {
            try
            {
                byte[] hash = _sha256.ComputeHash(bytes);

                return hash;
            }
            catch (Exception exception)
            {
                throw new UnsupportedOperationException(Sha256Exception, exception);
            }
        }

        private bool VerifySignedBytesAgainstSignature(CngKey publicKey, byte[] signedBytes, byte[] signature)
        {
            using (ECDsaCng verifier = new ECDsaCng(publicKey))
            {
                verifier.HashAlgorithm = CngAlgorithm.Sha256;
                bool result = verifier.VerifyData(signedBytes, signature.FromAsn1Signature());
                return result;
            }
        }

        private CngKey ConvertPublicKey(byte[] rawData)
        {
            if (rawData == null || rawData.Length == 0 || rawData.Length != 65)
                throw new ArgumentException(InvalidArgumentException);

            var header = new byte[] { 0x45, 0x43, 0x53, 0x31, 0x20, 0x00, 0x00, 0x00 };
            var eccPublicKeyBlob = new byte[72];

            Array.Copy(header, 0, eccPublicKeyBlob, 0, 8);
            Array.Copy(rawData, 1, eccPublicKeyBlob, 8, 64);

            CngKey key = CngKey.Import(eccPublicKeyBlob, CngKeyBlobFormat.EccPublicBlob);

            return key;
        }

        public void Dispose()
        {
            _sha256.Dispose();
            _sha256 = null;
            _randomNumberGenerator.Dispose();
            _randomNumberGenerator = null;
        }
    }
}