using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using U2F.Core.Exceptions;
using U2F.Core.Utils;

namespace U2F.Core.Crypto
{
    public sealed class CryptoService : IDisposable, ICrytoService
    {
        private SHA256 _sha256 = SHA256.Create();
        private RandomNumberGenerator _randomNumberGenerator;
        private const string SignatureError = "Error when verifying signature";
        private const string ErrorDecodingPublicKey = "Error when decoding public key";
        private const string InvalidArgumentException = "The arguments passed the were not valid";

        public CryptoService()
        {
            _sha256.Initialize();
            // TODO i should be able to get to the sha256 
            //_randomNumberGenerator = RandomNumberGenerator.Create("Sha256"); 
            _randomNumberGenerator = RandomNumberGenerator.Create();
        }

        public byte[] GenerateChallenge()
        {
            byte[] randomBytes = new byte[32];
            _randomNumberGenerator.GetBytes(randomBytes);

            return randomBytes;
        }

        public X509Certificate DecodePublicKey(byte[] encodedPublicKey)
        {
            try
            {
                // TODO determine the friendly name
                ECCurve thing = ECCurve.CreateFromOid(Oid.FromFriendlyName("SecP256r1", OidGroup.EncryptionAlgorithm));

                ECDsa signer = ECDsa.Create();

                ECParameters param = new ECParameters();
                param.Curve = ECCurve.NamedCurves.nistP256;
                param.Q = new ECPoint();
            }
            catch (InvalidKeySpecException exception)
            {
                throw new U2fException(SignatureError, exception);
            }
            catch (Exception exception)
            {
                throw new U2fException(ErrorDecodingPublicKey, exception);
            }

            return null;
        }

        //TODO compare with unit test results
        public bool CheckSignature(X509Certificate certificate, byte[] signedbytes, byte[] signature)
        {
            try
            {
                if (certificate == null || signedbytes== null || signedbytes.Length == 0 
                    || signature == null || signature.Length == 0)
                    throw new ArgumentException(InvalidArgumentException);
                
                CngKey cngKey = CngKey.Create(CngAlgorithm.ECDsaP256);                
                ECDsaCng ecDsaCng = new ECDsaCng(cngKey);
                byte[] signedHash = ecDsaCng.SignData(signedbytes, 0, signedbytes.Length, HashAlgorithmName.SHA256);

                if (!ecDsaCng.VerifyHash(signedHash, signature))
                    throw new U2fException(SignatureError);

                return true;
            }
            catch (InvalidKeySpecException e)
            {
                throw new U2fException(SignatureError, e);
            }
            catch (Exception e)
            {
                throw new U2fException(SignatureError, e);
            }
        }

        public byte[] Hash(string str)
        {
            return str.GetBytes();
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
                throw new UnsupportedOperationException("Error when computing SHA-256", exception);
            }
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