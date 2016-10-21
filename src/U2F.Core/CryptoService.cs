using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using U2F.Core.Exceptions;
using U2F.Core.Utils;

namespace U2F.Core
{
    public sealed class CryptoService : IDisposable, ICrytoService
    {
        private SHA256 _sha256 = SHA256.Create();
        private RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();
        private const string SignatureError = "Error when verifying signature";
        private const string ErrorDecodingPublicKey = "Error when decoding public key";
        private const string InvalidArgumentException = "The arguments passed the were not valid";

        public CryptoService()
        {
            _sha256.Initialize();
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
                CngKey cngKey = CngKey.Create(CngAlgorithm.ECDsaP256);
                
                ECDsaCng ecDsaCng = new ECDsaCng(cngKey);
                //ecDsaCng.HashAlgorithm = CngAlgorithm.ECDsaP256;
                ecDsaCng.KeySize = 256;

                ECDsa signer = ECDsa.Create();

                ECParameters param = new ECParameters();
                param.Curve = ECCurve.NamedCurves.nistP256;
                

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

        public bool CheckSignature(X509Certificate certificate, byte[] signedbytes, byte[] signature)
        {
            try
            {
                if (certificate == null || signedbytes== null || signedbytes.Length == 0 
                    || signature == null || signature.Length == 0)
                    throw new ArgumentException(InvalidArgumentException);

                CngKey cngKey = CngKey.Create(CngAlgorithm.ECDsaP256);
                ECDsaCng ecDsaCng = new ECDsaCng(cngKey);
                CngKey.Import(certificate.GetPublicKey(), CngKeyBlobFormat.EccFullPublicBlob);
                ECDsa signer = ECDsa.Create();
                ECParameters param = new ECParameters();
                
                signer.ImportParameters(param);
                byte[] generatedSignature = signer.SignData(signedbytes, 0, signedbytes.Length, HashAlgorithmName.SHA256);

                if (!signer.VerifyData(generatedSignature, signature, HashAlgorithmName.SHA256))
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