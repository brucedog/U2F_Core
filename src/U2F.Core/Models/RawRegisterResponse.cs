using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using U2F.Core.Exceptions;
using U2F.Core.Utils;

namespace U2F.Core.Models
{
    public class RawRegisterResponse
    {
        private const byte RegistrationReservedByteValue = 0x05;
        private const byte RegistrationSignedReservedByteValue = 0x00;
        
        // The (uncompressed) x,y-representation of a curve point on the P-256 NIST elliptic curve.
        private readonly byte[] _userPublicKey;

        // A handle that allows the U2F token to identify the generated key pair.
        private readonly byte[] _keyHandle;

        private readonly X509Certificate _attestationCertificate;

        // A ECDSA signature (on P-256) 
        private readonly byte[] _signature;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawRegisterResponse"/> class.
        /// </summary>
        /// <param name="userPublicKey">The user public key.</param>
        /// <param name="keyHandle">The key handle.</param>
        /// <param name="attestationCertificate">The attestation certificate.</param>
        /// <param name="signature">The signature.</param>
        public RawRegisterResponse(byte[] userPublicKey, byte[] keyHandle,
                                   X509Certificate attestationCertificate, byte[] signature)
        {
            _userPublicKey = userPublicKey;
            _keyHandle = keyHandle;
            _attestationCertificate = attestationCertificate;
            _signature = signature;
        }

        /// <summary>
        /// Converts string response into RawRegisterResponse object
        /// </summary>
        /// <param name="rawDataBase64">raw string from client</param>
        /// <returns>RawRegisterResponse object</returns>
        public static RawRegisterResponse FromBase64(String rawDataBase64)
        {
            if(String.IsNullOrWhiteSpace(rawDataBase64))
                throw new ArgumentException("Invalid argument were being passed.");

            byte[] bytes = rawDataBase64.Base64StringToByteArray();

            Stream stream = new MemoryStream(bytes);
            BinaryReader binaryReader = new BinaryReader(stream);

            try
            {
                byte reservedByte = binaryReader.ReadByte();
                if (reservedByte != RegistrationReservedByteValue)
                {
                    throw new U2fException(String.Format("Incorrect value of reserved byte. Expected: {0}. Was: {1}",
                        RegistrationReservedByteValue, reservedByte));
                }

                byte[] publicKey = binaryReader.ReadBytes(65);
                byte[] keyHandle = binaryReader.ReadBytes(binaryReader.ReadByte());
                // TODO need to determine actual length of cert
                X509Certificate x509Certificate = new X509Certificate(binaryReader.ReadBytes(320));
                
                int size = (int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position);

                byte[] signature = binaryReader.ReadBytes(size);

                RawRegisterResponse rawRegisterResponse = new RawRegisterResponse(
                    publicKey,
                    keyHandle,
                    x509Certificate,
                    signature);

                return rawRegisterResponse;
            }
            catch (Exception e)
            {
                throw new U2fException("Error when parsing attestation certificate", e);
            }
            finally
            {
                stream.Dispose();
                binaryReader.Dispose();
            }
        }

        public void CheckSignature(String appId, String clientData)
        {
            if (String.IsNullOrWhiteSpace(appId) || String.IsNullOrWhiteSpace(clientData))
                throw new ArgumentException("Invalid argument(s) were being passed.");

            byte[] signedBytes = PackBytesToSign(
                U2F.Crypto.Hash(appId),
                U2F.Crypto.Hash(clientData),
                _keyHandle,
                _userPublicKey);

            U2F.Crypto.CheckSignature(_attestationCertificate, signedBytes, _signature);
        }

        public DeviceRegistration CreateDevice()
        {
            return new DeviceRegistration(
                _keyHandle,
                _userPublicKey,
                // TODO investigate that this is correct
                _attestationCertificate.GetPublicKey(),
                DeviceRegistration.InitialCounterValue);
            return null;
        }

        public byte[] PackBytesToSign(byte[] appIdHash,
            byte[] clientDataHash, byte[] keyHandle, byte[] userPublicKey)
        {
            List<byte> someBytes = new List<byte>();
            someBytes.Add(RegistrationSignedReservedByteValue);
            someBytes.AddRange(appIdHash);
            someBytes.AddRange(clientDataHash);
            someBytes.AddRange(keyHandle);
            someBytes.AddRange(userPublicKey);

            return someBytes.ToArray();
        }

        public override int GetHashCode()
        {
            int hash = 23 + _userPublicKey.Sum(b => b + 31);
            hash += _keyHandle.Sum(b => b + 31);
            hash += _signature.Sum(b => b + 31);

            return hash;
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is RawRegisterResponse))
                return false;
            if (this == obj)
                return true;
            if (GetType() != obj.GetType())
                return false;
            RawRegisterResponse other = (RawRegisterResponse)obj;
            if (!_attestationCertificate.Equals(other._attestationCertificate))
                return false;
            if (!_keyHandle.SequenceEqual(other._keyHandle))
                return false;
            if (!_signature.SequenceEqual(other._signature))
                return false;
            return _userPublicKey.SequenceEqual(other._userPublicKey);
        }
    }
}