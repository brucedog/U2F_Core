using System;
using System.Security.Cryptography;
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

namespace U2F.Core.Crypto
{
    public sealed class CryptoService : IDisposable, ICrytoService
    {
        private readonly DerObjectIdentifier _curve = SecObjectIdentifiers.SecP256r1;
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

        public byte[] GenerateChallenge()
        {
            byte[] randomBytes = new byte[32];
            _randomNumberGenerator.GetBytes(randomBytes);

            return randomBytes;
        }

        public bool CheckSignature(X509Certificate attestationCertificate, byte[] signedBytes, byte[] signature)
        {
            return CheckSignature(attestationCertificate.GetPublicKey(), signedBytes, signature);
        }

        public ICipherParameters DecodePublicKey(byte[] encodedPublicKey)
        {
            try
            {
                X9ECParameters curve = SecNamedCurves.GetByOid(_curve);
                ECPoint point = curve.Curve.DecodePoint(encodedPublicKey);
                ECDomainParameters ecP = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

                return new ECPublicKeyParameters(point, ecP);
            }
            catch (InvalidKeySpecException exception)
            {
                throw new U2fException(SignatureError, exception);
            }
            catch (Exception exception)
            {
                throw new U2fException(ErrorDecodingPublicKey, exception);
            }
        }
        
        public bool CheckSignature(ICipherParameters certificate, byte[] signedbytes, byte[] signature)
        {
            try
            {
                if (certificate == null || signedbytes== null || signedbytes.Length == 0 
                    || signature == null || signature.Length == 0)
                    throw new ArgumentException(InvalidArgumentException);

                ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
                signer.Init(false, certificate);
                signer.BlockUpdate(signedbytes, 0, signedbytes.Length);

                if (!signer.VerifySignature(signature))
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
            return Hash(str.GetBytes());
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

        public void Dispose()
        {
            _sha256.Dispose();
            _sha256 = null;
            _randomNumberGenerator.Dispose();
            _randomNumberGenerator = null;
        }
    }
}