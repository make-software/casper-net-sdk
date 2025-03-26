using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    internal class PackageCompat : RpcResult
    {
        [JsonPropertyName("Package")]
        public Package Package { get; init; }
        
        [JsonPropertyName("ContractPackage")]
        public ContractPackage ContractPackage { get; init; }
    }
    
    internal class GetPackageResultCompat : RpcResult
    {
        [JsonPropertyName("package")]
        public PackageCompat Package { get; init; }
        
        [JsonPropertyName("merkle_proof")]
        public string MerkleProof { get; init; }        
    }
    
    [JsonConverter(typeof(GetPackageResultConverter))]
    public class GetPackageResult : RpcResult
    {
        /// <summary>
        /// A legacy Contract Package.
        /// </summary>
        public ContractPackage ContractPackage { get; init; }
        
        /// <summary>
        /// An Addressable Entity package.
        /// </summary>
        public Package Package { get; init; }
        
        /// <summary>
        /// The merkle proof.
        /// </summary>
        public string MerkleProof { get; init; }
        
        public class GetPackageResultConverter : JsonConverter<GetPackageResult>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(GetPackageResult);
            }

            public override GetPackageResult Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    var resultCompat = JsonSerializer.Deserialize<GetPackageResultCompat>(ref reader, options);

                    return new GetPackageResult()
                    {
                        ApiVersion = resultCompat.ApiVersion,
                        ContractPackage = resultCompat.Package.ContractPackage,
                        Package = resultCompat.Package.Package,
                        MerkleProof = resultCompat.MerkleProof,
                    };
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                GetPackageResult block,
                JsonSerializerOptions options)
            {
                throw new JsonException($"not implemented");
            }
        }
    }
}