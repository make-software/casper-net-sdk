using System.Collections.Generic;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Identifier of a package by its contract package hash or its addressable entity package address.
    /// </summary>
    public interface IPackageIdentifier
    {
        public Dictionary<string, object> GetPackageIdentifier();
    }
    
    /// <summary>
    /// Identifier of a package by its contract package hash or its addressable entity package address.
    /// </summary>
    public class PackageIdentifier : IPackageIdentifier
    {
        private readonly string? _contractPackageHash;
        private readonly string? _packageAddr;

        private PackageIdentifier(string? contractPackageHash = null, string? packageAddr = null)
        {
            _contractPackageHash = contractPackageHash;
            _packageAddr = packageAddr;
        }

        public static PackageIdentifier FromContractPackageHash(string contractPackageHash) =>
            new(contractPackageHash: contractPackageHash);

        public static PackageIdentifier FromPackageAddr(string packageAddr) =>
            new(packageAddr: packageAddr);

        /// <summary>
        /// Returns a PackageIdentifier object as defined in the RPC schema.
        /// </summary>
        public Dictionary<string, object> GetPackageIdentifier()
        {
            return !string.IsNullOrWhiteSpace(_contractPackageHash)
                ? new Dictionary<string, object> { { "ContractPackageHash", _contractPackageHash! } }
                : new Dictionary<string, object> { { "PackageAddr", _packageAddr! } };
        }
    }
}