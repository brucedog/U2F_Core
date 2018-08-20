using System;
using System.IO;

namespace U2F.Core.Utils
{
    /// <summary>
    /// ASN1 Integer helper class.
    /// </summary>
    internal class Asn1Helper
    {
        /// <summary>
        /// The ASN1 integer tag
        /// </summary>
        private const byte IntegerTag = 0x02;

        /// <summary>
        /// Converts the DER encoded ASN1 integer from the binary binaryReader.
        /// Note: the actual data should be either 33 or 32 bytes long.
        /// </summary>
        /// <param name="binaryReader">The binary reader.</param>
        /// <returns>A ANS1 integer.</returns>
        internal static byte[] ConvertDerToAsn1(BinaryReader binaryReader)
        {
            if (binaryReader == null)
            {
                throw new ArgumentNullException(nameof(binaryReader));
            }

            byte tag = binaryReader.ReadByte();
            if (tag != IntegerTag)
            {
                throw new ArgumentOutOfRangeException(nameof(binaryReader), "byte does not match the integer tag.");
            }

            byte dataLength = binaryReader.ReadByte();
            if (dataLength < 32 || dataLength > 33)
            {
                throw new ArgumentOutOfRangeException(nameof(binaryReader), "The data is not 32 or 33 bytes long.");
            }

            byte[] data;
            if (dataLength == 33)
            {
                byte zero = binaryReader.ReadByte();
                if (zero != 0x00)
                {
                    throw new ArgumentOutOfRangeException(nameof(binaryReader), "The first byte should be zero.");
                }
                data = binaryReader.ReadBytes(32);
            }
            else
            {
                data = binaryReader.ReadBytes(32);
            }

            return data;
        }
    }

    /// <summary>
    /// Extension method for converting a DER encoded ASN1 Signature.
    /// The DER encoded ASN1 signature should be formatted as the following:
    /// 0x30: The DER sequence tag.
    /// xx : The length of the rest of the data: 68, 69 or 70 [2*(32{+1}+2)]
    /// 0x02: The ASN1 integer tag.
    /// yy : The length of the first object, either 32 or 33
    ///      The length should be 32 unless the first byte is greater than 0x80 when a leading 0x00 will be added.
    /// 0x02: Again the ASN1 integer tag.
    /// zz : The length of the second bit of data should be either 32 or 33.
    ///      The length should be 32 unless the first byte is greater than 0x80 when a leading 0x00 will be added.
    /// </summary>
    public static class Asn1Extensions
    {
        private const byte SequenceTag = 0x30;

        /// <summary>
        /// Convert from a DER encoded ASN1 signature, which is composed of two ASN1 integers.
        /// </summary>
        /// <param name="signature">The DER encoded ASN1 signature.</param>
        /// <returns>The converted signature.</returns>
        public static byte[] FromAsn1Signature(this byte[] signature)
        {
            if (signature.Length < 70 || signature.Length > 72)
            {
                throw new ArgumentOutOfRangeException(nameof(signature), "The signature should be 70 to 70 bytes long.");
            }

            using (MemoryStream stream = new MemoryStream(signature))
            using (BinaryReader reader = new BinaryReader(stream))
            {                
                byte tag = reader.ReadByte();
                if (tag != SequenceTag)
                {
                    throw new ArgumentOutOfRangeException(nameof(signature), "Invalid sequence tag.");
                }

                reader.ReadByte();

                byte[] first = Asn1Helper.ConvertDerToAsn1(reader);
                byte[] second = Asn1Helper.ConvertDerToAsn1(reader);

                byte[] convertedSignature = new byte[64];
                Array.Copy(first, 0, convertedSignature, 0, 32);
                Array.Copy(second, 0, convertedSignature, 32, 32);

                return convertedSignature;                
            }
        }
    }
}