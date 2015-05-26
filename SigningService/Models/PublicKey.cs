using SigningService.Extensions;
using System;

namespace SigningService.Models
{
    internal class PublicKey : IEquatable<PublicKey>
    {
        /// <summary>
        /// Constructs a public key from exponent and modulus pair
        /// </summary>
        /// <param name="exponent">Length of the exponent should be less or equal to 4</param>
        /// <param name="modulus"></param>
        public PublicKey(byte[] exponent, byte[] modulus)
        {
            if (exponent == null)
            {
                throw new ArgumentNullException("exponent");
            }

            if (modulus == null)
            {
                throw new ArgumentNullException("modulus");
            }

            if (exponent.Length > 4)
            {
                throw new ArgumentException("exponent Length should be less or equal to 4.");
            }

            Exponent = new byte[4];
            exponent.CopyTo(Exponent, 0);

            Modulus = new byte[modulus.Length];
            modulus.CopyTo(Modulus, 0);
        }

        // Exponent should be of length 4
        public byte[] Exponent { get; private set; }
        public byte[] Modulus { get; private set; }

        public bool Equals(PublicKey other)
        {
            if (other == null)
            {
                return false;
            }

            return Exponent.IsEquivalentTo(other.Exponent) && Modulus.IsEquivalentTo(other.Modulus);
        }

        // First and last two characters of modulus
        // are enough to uniquely identify public key
        public override int GetHashCode()
        {
            if (Modulus == null)
            {
                return 0;
            }

            uint hash;
            if (Modulus.Length >= 4)
            {
                uint a = Modulus[0];
                uint b = Modulus[1];
                uint c = Modulus[Modulus.Length - 2];
                uint d = Modulus[Modulus.Length - 1];

                a <<= 0;
                b <<= 8;
                c <<= 16;
                d <<= 24;
                hash = a | b | c | d;
                return unchecked((int)hash);
            }

            hash = 0;
            for (int i = 0; i < Modulus.Length; i++)
            {
                uint val = Modulus[i];
                hash |= val << (i * 8);
            }
            return unchecked((int)hash);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PublicKey pk = (PublicKey)obj;

            return Equals(pk);
        }

        public override string ToString()
        {
            return string.Format("PublicKey(Exponent: {0}, Modulus: {1})", Exponent.ToHex(), Modulus.ToHex());
        }
    }
}