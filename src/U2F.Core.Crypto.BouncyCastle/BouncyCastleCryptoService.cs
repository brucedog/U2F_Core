using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using U2F.Core.Exceptions;
using U2F.Core.Utils;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace U2F.Core.Crypto.BouncyCastle
{
    public class BouncyCastleCryptoService : ICryptoService, IDisposable
    {
        private SHA256 _sha256 = SHA256.Create();
        private RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

        public BouncyCastleCryptoService()
        {
            _sha256.Initialize();
        }

        private byte[] RawPubKeyFromCertificate(X509Certificate2 cert)
        {
            try
            {
                var pubKey = new X509CertificateParser().ReadCertificate(cert.RawData).GetPublicKey();
                return ((ECPublicKeyParameters)pubKey).Q.GetEncoded();
            }
            catch (Exception e)
            {
                throw new U2fException(U2fException.ErrorDecodingPublicKey, e);
            }
        }

        private ICipherParameters CipherParamsFromBytes(byte[] rawPublicKey)
        {
            try
            {
                var curve = SecNamedCurves.GetByOid(SecObjectIdentifiers.SecP256r1);
                var point = curve.Curve.DecodePoint(rawPublicKey);
                var ecP = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

                return new ECPublicKeyParameters(point, ecP);
            }
            catch (Exception exception)
            {
                throw new U2fException(U2fException.ErrorDecodingPublicKey, exception);
            }
        }

        private bool CheckSignature(ICipherParameters certificate, byte[] signedbytes, byte[] signature)
        {
            try
            {
                if (certificate == null || signedbytes == null || signedbytes.Length == 0
                    || signature == null || signature.Length == 0)
                    throw new U2fException(U2fException.InvalidArguments);

                var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
                signer.Init(false, certificate);
                signer.BlockUpdate(signedbytes, 0, signedbytes.Length);

                if (!signer.VerifySignature(signature))
                    throw new U2fException(U2fException.SignatureError);

                return true;
            }
            catch (Exception e)
            {
                throw new U2fException(U2fException.SignatureError, e);
            }
        }

        public bool CheckSignature(X509Certificate2 certificate, byte[] signedBytes, byte[] signature)
        {
            var rawPublicKey = RawPubKeyFromCertificate(certificate);
            var cipherParams = CipherParamsFromBytes(rawPublicKey);
            return CheckSignature(cipherParams, signedBytes, signature);
        }

        public bool CheckSignature(byte[] publicKey, byte[] signedBytes, byte[] signature)
        {
            var cipherParams = CipherParamsFromBytes(publicKey);
            return CheckSignature(cipherParams, signedBytes, signature);
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

        public byte[] GenerateChallenge()
        {
            byte[] randomBytes = new byte[32];
            _randomNumberGenerator.GetBytes(randomBytes);

            return randomBytes;
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
