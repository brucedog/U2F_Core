using System;
using System.Security.Cryptography.X509Certificates;

namespace U2F.Core
{
    public interface ICrytoService
    {
        /// <summary>
        /// Checks the signature.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="src">The source.</param>
        /// <param name="signature">The signature.</param>
        /// <returns></returns>
       // bool CheckSignature(ICipherParameters key, byte[] src, byte[] signature);

        /// <summary>
        /// Checks the signature.
        /// </summary>
        /// <param name="attestationCertificate">The attestation certificate.</param>
        /// <param name="signedBytes">The signed bytes.</param>
        /// <param name="signature">The signature.</param>
        /// <returns></returns>
        bool CheckSignature(X509Certificate attestationCertificate, byte[] signedBytes, byte[] signature);

        /// <summary>
        /// Decodes the public key.
        /// </summary>
        /// <param name="encodedPublicKey">The encoded public key.</param>
        /// <returns></returns>
        X509Certificate DecodePublicKey(byte[] encodedPublicKey);

        /// <summary>
        /// Hashes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        byte[] Hash(byte[] bytes);

        /// <summary>
        /// Hashes the specified string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        byte[] Hash(String str);
        
    }
}