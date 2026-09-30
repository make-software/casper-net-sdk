using System.IO;
using System.Numerics;
using System.Text.Json;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest.RPCResponses
{
    public class SpeculativeExecutionResultTest
    {
        private static string Fixture(string name) =>
            File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", name));

        [Test]
        public void ParsesCasperV1Success()
        {
            var result = RpcResult.Parse<SpeculativeExecutionResult>(Fixture("specexec_transfer_success_v158.json"));
            var execution = (ExecutionResult)result.ExecutionResult;

            Assert.AreEqual("1.0.0", result.ApiVersion);
            Assert.AreEqual("c07bd513629c39fec57260b4e94feded8150182aacf24769c98eaac97a537cc4", result.BlockHash);
            Assert.AreEqual(1, execution.Version);
            Assert.IsTrue(execution.IsSuccess);
            Assert.AreEqual(new BigInteger(100000000), execution.Cost);
            Assert.AreEqual(1, execution.Transfers.Count);
            Assert.AreEqual(5, execution.Effect.Count);
            Assert.IsNull(result.Messages);
        }

        [Test]
        public void ParsesCasperV1Failure()
        {
            var result = RpcResult.Parse<SpeculativeExecutionResult>(Fixture("specexec_transfer_failure_v158.json"));
            var execution = (ExecutionResult)result.ExecutionResult;

            Assert.AreEqual("eda864c7da3f5765a7027e3aa234b05c4b6c33efae51adacd224dc5f3c1c7958", result.BlockHash);
            Assert.AreEqual(1, execution.Version);
            Assert.IsFalse(execution.IsSuccess);
            Assert.AreEqual("Insufficient payment", execution.ErrorMessage);
            Assert.AreEqual(0, execution.Transfers.Count);
            Assert.AreEqual(2, execution.Effect.Count);
            Assert.IsNull(result.Messages);
        }

        [Test]
        public void ParsesCasperV2Success()
        {
            var result = RpcResult.Parse<SpeculativeExecutionResult>(Fixture("specexec_cep18transfer_success_v200.json"));

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual("entity-contract-eece73f6a210f5d08f8a9da3348ab3c6f65d42eb0df9938f324940fb5422c360",
                result.Messages[0].EntityAddr);
            Assert.IsTrue(result.Messages[0].MessagePayload.String.Contains("recipient"));
            Assert.AreEqual("events", result.Messages[0].TopicName);
            Assert.AreEqual("5721a6d9d7a9afe5dfdb35276fb823bed0f825350e4d865a5ec0110c380de4e1", result.Messages[0].TopicNameHash);
            Assert.AreEqual(1, result.Messages[0].TopicIndex);
            Assert.AreEqual(2, result.Messages[0].BlockIndex);
        }

        [Test]
        public void ParsesCasperV2FailureThroughRpcResponse()
        {
            var json = "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":" + Fixture("specexec_transfer_failure_v200.json") + "}";
            var response = JsonSerializer.Deserialize<RpcResponse<SpeculativeExecutionResult>>(json);
            var result = response.Parse();
            var execution = (ExecutionResult)result.ExecutionResult;

            Assert.AreEqual("f563d1435b3c707c85a2ead4a9524022f241bee8076775d8999102e5b9cf93e8", result.BlockHash);
            Assert.AreEqual(2, execution.Version);
            Assert.IsFalse(execution.IsSuccess);
            Assert.AreEqual("Mint(InsufficientFunds)", execution.ErrorMessage);
            Assert.AreEqual(0, execution.Transfers.Count);
            Assert.AreEqual(0, execution.Effect.Count);
        }
    }
}
