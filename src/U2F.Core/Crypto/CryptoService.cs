using System;
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
            // TODO i should be able to get to the sha256 
            //_randomNumberGenerator = RandomNumberGenerator.Create("Sha256"); 
            _randomNumberGenerator = RandomNumberGenerator.Create();
        }

        private CngKey PublicKeyFromBytes(byte[] rawKey)
        {
            try
            {
                return ConvertPublicKey(rawKey);
            }
            catch (Exception exception)
            {
                throw new U2fException(U2fException.ErrorDecodingPublicKey, exception);
            }
        }

        private CngKey PublicKeyFromCertificate(X509Certificate2 certificate)
        {
            try
            {
                byte[] rawData = certificate.PublicKey.EncodedKeyValue.RawData;

                return ConvertPublicKey(rawData);
            }
            catch (Exception exception)
            {
                throw new U2fException(U2fException.ErrorDecodingPublicKey, exception);
            }
        }

        public bool CheckSignature(byte[] publicKey, byte[] signedBytes, byte[] signature)
        {
            try
            {
                var cngPubKey = PublicKeyFromBytes(publicKey);

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

                CngKey publicKey = PublicKeyFromCertificate(certificate);

                bool result = VerifySignedBytesAgainstSignature(publicKey, signedBytes, signature);

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
            if (rawData == null || rawData.Length != 65)
                throw new U2fException(U2fException.InvalidArguments);

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