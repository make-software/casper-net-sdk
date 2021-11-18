using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest
{
    public class TransferDeployTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Transfer1()
        {
            var header = new DeployHeader()
            {
                Account = PublicKey.FromHexString("01027c04a0210afdf4a83328d57e8c2a12247a86d872fb53367f22a84b1b53d2a9"),
                Timestamp = DateUtils.ToEpochTime("2021-09-25T17:01:24.399Z"),
                Ttl = 1800*1000, //"30m",
                ChainName = "casper-test",
                GasPrice = 1
            };
            
            var bigInt1000 = new BigInteger(1000);
            var payment = new ModuleBytesDeployItem(bigInt1000);
            var session = new TransferDeployItem(
                15000000000,
                PublicKey.FromHexString("01027c04a0210afdf4a83328d57e8c2a12247a86d872fb53367f22a84b1b53d2a9"),
                null,
                123456789012345);

            var deploy = new Deploy(header, payment, session);
            
            deploy.AddApproval(new DeployApproval()
            {
                Signature = Signature.FromHexString("012dbf03817a51794a8e19e0724884075e6d1fbec326b766ecfa6658b41f81290da85e23b24e88b1c8d9761185c961daee1adab0649912a6477bcd2e69bd91bd08"),
                Signer = PublicKey.FromHexString("01027c04a0210afdf4a83328d57e8c2a12247a86d872fb53367f22a84b1b53d2a9")
            });
            
            var options = new JsonSerializerOptions()
            {
                Converters =
                {
                    new PublicKey.PublicKeyConverter(),
                    new ExecutableDeployItemConverter()
                }
            };
            var json = JsonSerializer.Serialize(deploy, options);
            Assert.IsNotNull(json);
        }
    }
}