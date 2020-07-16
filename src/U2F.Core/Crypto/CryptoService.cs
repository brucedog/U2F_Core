using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using U2F.Core.Exceptions;
using U2F.Core.Utils;

namespace U2F.Core.Crypto
{
    public sealed class CryptoService : IDisposable, ICryptoService
    {
        private SHA256 _sha256 = SHA256.Create();
        private RandomNumberGenerator _randomNumberGenerator;

        public CryptoService()
        {
            _sha256.Initialize();
            _randomNumberGenerator = RandomNumberGenerator.Create();
        }

        public bool CheckSignature(byte[] publicKey, byte[] signedBytes, byte[] signature)
        {
            try
            {
                var cngPubKey = ConvertPublicKey(publicKey);

                if (cngPubKey == null
                    || signedBytes == null || signedBytes.Length == 0
                    || signature == null || signature.Length == 0)
                    throw new U2fException(U2fException.InvalidArguments);

                bool result = VerifySignedBytesAgainstSignature(cngPubKey, signedBytes, signature);

                if (!result)
                    throw new U2fException(U2fException.SignatureError);

                return true;
            }
            catch (Exception exception)
            {
                throw new U2fException(U2fException.SignatureError, exception);
            }
        }

        public bool CheckSignature(X509Certificate2 certificate, byte[] signedBytes, byte[] signature)
        {
            try
            {
                if (certificate == null
                    || signedBytes == null || signedBytes.Length == 0
                    || signature == null || signature.Length == 0)
                    throw new U2fException(U2fException.InvalidArguments);
                
                bool result = VerifySignedBytesAgainstSignature(certificate.GetECDsaPublicKey(), signedBytes, signature);

                if (!result)
                    throw new U2fException(U2fException.SignatureError);

                return true;
            }
            catch (Exception exception)
            {
                throw new U2fException(U2fException.SignatureError, exception);
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
                throw new UnsupportedOperationException(UnsupportedOperationException.Sha256Exception, exception);
            }
        }

        private bool VerifySignedBytesAgainstSignature(ECDsa publicKey, byte[] signedBytes, byte[] signature)
        {
            bool result = publicKey.VerifyData(signedBytes, signature.FromAsn1Signature(), HashAlgorithmName.SHA256);
            return result;
        }

        /// <summary>
        /// Coverts byte array into a P256 EC Public key
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns>NIST P256 Public key</returns>
        private ECDsa ConvertPublicKey(byte[] rawData)
        {
            if (rawData == null || rawData.Length != 65)
                throw new U2fException(U2fException.InvalidArguments);
            
            var pubKeyX = rawData.Skip(1).Take(32).ToArray();
            var pubKeyY = rawData.Skip(33).ToArray();

            return ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint
                {
                    X = pubKeyX,
                    Y = pubKeyY
                }
            });
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