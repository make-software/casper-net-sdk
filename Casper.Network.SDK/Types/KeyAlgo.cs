using System;

namespace Casper.Network.SDK.Types
{
    public enum KeyAlgo
    {
        ED25519 = 1,
        SECP256K1 = 2
    }

    public static class KeyAlgoExtensions
    {
        public static bool MatchesKey(this KeyAlgo ka, string key)
        {
            if (key == null || key.Length < 2)
                return false;

            var fb = key.Substring(0, 2);
            
            return ka == KeyAlgo.ED25519 && fb == "01" ||
                   ka == KeyAlgo.SECP256K1 && fb == "02";
        }

        public static KeyAlgo GetAlgorithm(this KeyAlgo ka, string key)
        {
            return key.Substring(0, 2) switch
            {
                "01" => KeyAlgo.ED25519,
                "02" => KeyAlgo.SECP256K1,
                _ => throw new ArgumentOutOfRangeException("key", "Wrong first byte in public key value")
            };
        }

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