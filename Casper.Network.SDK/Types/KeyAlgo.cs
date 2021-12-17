using System;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Enumeration of the different key algorithms supported
    /// </summary>
    public enum KeyAlgo
    {
        /// <summary>
        /// Belonging to Curve25519 elliptic curve.
        /// </summary>
        ED25519 = 1,
        /// <summary>
        /// Belonging to secp256k1 elliptic curve.
        /// </summary>
        SECP256K1 = 2
    }

    public static class KeyAlgoExtensions
    {
        /// <summary>
        /// Returns the expected size in bytes of a key depending on the algorithm used
        /// </summary>
        public static int GetKeySizeInBytes(this KeyAlgo ka)
        {
            return ka switch
            {
                KeyAlgo.ED25519 => 33,
                KeyAlgo.SECP256K1 => 34,
                _ => throw new Exception($"Wrong key algoirthm type {ka}")
            };
        }
    }
}