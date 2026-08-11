using System.IO;
using System.Text.Json;
using Casper.Network.SDK.Types;
using NetCasperTest;
using NUnit.Framework;

namespace Casper.Network.SDK.Test
{
    public class BlockTest
    {
        [Test]
        public void BlockHeaderV1Test()
        {
            string testFile = TestContext.CurrentContext.TestDirectory + "/TestData/block_header_v1.json";
            var json = File.ReadAllText(testFile);

            var _options = new JsonSerializerOptions()
            {
                WriteIndented = false,
                Converters = { new BlockHeader.BlockHeaderConverter() }
            };

            var block = JsonSerializer.Deserialize<BlockHeader>(json, _options);

            Assert.IsNotNull(block);
            Assert.AreEqual("a11fbc52f7ace34fb3b84af0756462691af64f6e8ecf562171b8a61281dc1f00", block.StateRootHash);
            Assert.AreEqual(1000, block.Height);
            Assert.AreEqual(10, block.EraId);
            
            var blockV1 = (BlockHeaderV1)block;
            Assert.IsNotNull(blockV1);
            Assert.AreEqual(1000, blockV1.Height);
            Assert.AreEqual(10, blockV1.EraId);
            Assert.AreEqual("a11fbc52f7ace34fb3b84af0756462691af64f6e8ecf562171b8a61281dc1f00", blockV1.StateRootHash);
        }
        

        [Test]
        public void BlockHeaderV2Test()
        {
            string testFile = TestContext.CurrentContext.TestDirectory + "/TestData/block_header_v2.json";
            var json = File.ReadAllText(testFile);
            
        var _options = new JsonSerializerOptions()
        {
            WriteIndented = false,
            Converters = { new BlockHeader.BlockHeaderConverter() }
        };
        
            var block = JsonSerializer.Deserialize<BlockHeader>(json,  _options);
            
            Assert.IsNotNull(block);
            Assert.AreEqual("1bcde15749ac85e56f76d7428fc5c7e2a83b5e968dfe3845b0c46e929913c992", block.StateRootHash);
            Assert.AreEqual(8803828, block.Height);
            
            var blockV2 = (BlockHeaderV2)block;
            Assert.IsNotNull(blockV2);
            Assert.AreEqual("1bcde15749ac85e56f76d7428fc5c7e2a83b5e968dfe3845b0c46e929913c992", blockV2.StateRootHash);
            Assert.AreEqual("fd4cd1c938d4d8c4876940f2514af3251f127b20203fa8850ee57504579fe438", blockV2.LastSwitchBlockHash);
            Assert.AreEqual(8803828, blockV2.Height);
        }
    }
}