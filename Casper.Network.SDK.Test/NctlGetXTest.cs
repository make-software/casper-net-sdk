using System.Text.Json;
using System.Threading.Tasks;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NUnit.Framework;

namespace NetCasperTest
{
    [Category("NCTL")]
    public class NctlGetXTest : NctlBase
    {
        [Test]
        public async Task GetNodeStatusTest()
        {
            var rpcResponse = await _client.GetNodeStatus();
            Assert.IsNotEmpty(rpcResponse.Result.GetRawText());
            Assert.AreEqual("2.0", rpcResponse.JsonRpc);
            Assert.AreNotEqual(0, rpcResponse.Id);
            
            var nodeStatus = rpcResponse.Parse();
            Assert.IsNotEmpty(nodeStatus.ApiVersion);

            // only put deploy returns a deploy hash. catch exception for other cases
            //
            var ex = Assert.Catch<RpcClientException>(() => rpcResponse.GetDeployHash());
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains("deploy_hash property not found."));
        }
        
        [Test]
        public async Task GetNodePeersTest()
        {
            var rpcResponse = await _client.GetNodePeers();
            Assert.IsNotEmpty(rpcResponse.Result.GetRawText());

            var nodePeers = rpcResponse.Parse();
            Assert.IsNotEmpty(nodePeers.ApiVersion);
            Assert.IsTrue(nodePeers.Peers.Count > 0);
            Assert.IsNotEmpty(nodePeers.Peers[0].Address);
            Assert.IsNotEmpty(nodePeers.Peers[0].NodeId);
        }

        [Test]
        public async Task GetAccountTest()
        {
            try
            {
                var block = (await _client.GetBlock()).Parse().Block;
                var blockHeight = (int) block.Header.Height;
                var blockHash = block.Hash;
                var stateRootHash = await _client.GetStateRootHash(blockHash);

                var response = await _client.GetAccountInfo(_faucetKey.PublicKey, blockHeight);
                var accountInfo = response.Parse();
                Assert.IsNotEmpty(accountInfo.Account.AccountHash.ToString());

                var response2 = await _client.GetAccountInfo(_faucetKey.PublicKey.ToAccountHex(), blockHeight);
                var accountInfo2 = response2.Parse();
                Assert.AreEqual(accountInfo.Account.AccountHash.ToString(), accountInfo2.Account.AccountHash.ToString());
                
                var response3 = await _client.GetAccountInfo(new AccountHashKey(_faucetKey.PublicKey), blockHeight);
                var accountInfo3 = response3.Parse();
                Assert.AreEqual(accountInfo.Account.AccountHash.ToString(), accountInfo3.Account.AccountHash.ToString());

                var response4 = await _client.GetAccountInfo(_faucetKey.PublicKey, blockHash);
                var accountInfo4 = response4.Parse();
                Assert.AreEqual(accountInfo.Account.AccountHash.ToString(), accountInfo4.Account.AccountHash.ToString());

                var response5 = await _client.GetAccountInfo(_faucetKey.PublicKey.ToAccountHex(), blockHash);
                var accountInfo5 = response5.Parse();
                Assert.AreEqual(accountInfo.Account.AccountHash.ToString(), accountInfo5.Account.AccountHash.ToString());

                var response6 = await _client.GetAccountInfo(new AccountHashKey(_faucetKey.PublicKey), blockHash);
                var accountInfo6 = response6.Parse();
                Assert.AreEqual(accountInfo.Account.AccountHash.ToString(), accountInfo6.Account.AccountHash.ToString());
                
                var resp = await _client.GetAccountBalance(_faucetKey.PublicKey, stateRootHash);
                var accountBalance = resp.Parse();
                Assert.IsTrue(accountBalance.BalanceValue > 0);

                resp = await _client.GetAccountBalance(accountInfo.Account.MainPurse, stateRootHash);
                var accountBalanceCompare = resp.Parse();
                Assert.AreEqual(accountBalance.BalanceValue, accountBalanceCompare.BalanceValue);
                
                resp = await _client.GetAccountBalance(accountInfo.Account.MainPurse.ToString(), stateRootHash);
                accountBalanceCompare = resp.Parse();
                Assert.AreEqual(accountBalance.BalanceValue, accountBalanceCompare.BalanceValue);
                
                resp = await _client.GetAccountBalance(new AccountHashKey(_faucetKey.PublicKey), stateRootHash);
                accountBalanceCompare = resp.Parse();
                Assert.AreEqual(accountBalance.BalanceValue, accountBalanceCompare.BalanceValue);
                
                
                resp = await _client.GetAccountBalance(_faucetKey.PublicKey, blockHeight);
                accountBalanceCompare = resp.Parse();
                Assert.AreEqual(accountBalance.BalanceValue, accountBalanceCompare.BalanceValue);

                resp = await _client.GetAccountBalance(accountInfo.Account.MainPurse, blockHeight);
                accountBalanceCompare = resp.Parse();
                Assert.AreEqual(accountBalance.BalanceValue, accountBalanceCompare.BalanceValue);
                
                resp = await _client.GetAccountBalance(new AccountHashKey(_faucetKey.PublicKey), blockHeight);
                accountBalanceCompare = resp.Parse();
                Assert.AreEqual(accountBalance.BalanceValue, accountBalanceCompare.BalanceValue);

                
                resp = await _client.GetAccountBalanceWithBlockHash(_faucetKey.PublicKey, blockHash);
                accountBalanceCompare = resp.Parse();
                Assert.AreEqual(accountBalance.BalanceValue, accountBalanceCompare.BalanceValue);

                resp = await _client.GetAccountBalanceWithBlockHash(accountInfo.Account.MainPurse, blockHash);
                accountBalanceCompare = resp.Parse();
                Assert.AreEqual(accountBalance.BalanceValue, accountBalanceCompare.BalanceValue);
                
                resp = await _client.GetAccountBalanceWithBlockHash(new AccountHashKey(_faucetKey.PublicKey), blockHash);
                accountBalanceCompare = resp.Parse();
                Assert.AreEqual(accountBalance.BalanceValue, accountBalanceCompare.BalanceValue);
            }
            catch (RpcClientException e)
            {
                Assert.Fail(e.RpcError.Message);
            }

            try
            {
                var key1 = KeyPair.CreateNew(KeyAlgo.ED25519);

                await _client.GetAccountInfo(key1.PublicKey);
                Assert.Fail("Exception expected");
            }
            catch (RpcClientException e)
            {
                Assert.IsNotNull(e.RpcError);
                Assert.IsNotNull(e.RpcError.Message);
            }
            
            try
            {
                var key1 = KeyPair.CreateNew(KeyAlgo.ED25519);

                await _client.GetAccountBalance(key1.PublicKey);
                Assert.Fail("Exception expected");
            }
            catch (RpcClientException e)
            {
                Assert.IsNotNull(e.RpcError);
                Assert.IsNotNull(e.RpcError.Message);
            }
        }

        [Test]
        public async Task GetBlockTest()
        {
            try
            {
                var response = await _client.GetBlock();
                var result = response.Parse();
                Assert.IsNotNull(result.Block.Hash);

                var response2 = await _client.GetBlock(1);
                var result2 = response2.Parse();
                Assert.IsNotNull(result2.Block.Hash);
                
                Assert.AreEqual(result2.Block.Body.Proposer.IsSystem, result2.Block.Body.Proposer.isSystem);
                
                var response3 = await _client.GetBlock(result2.Block.Hash);
                var result3 = response3.Parse();
                Assert.AreEqual(result2.Block.Hash, result3.Block.Hash);

                var response4 = await _client.GetBlock((int)result2.Block.Header.Height);
                var result4 = response4.Parse();
                Assert.AreEqual(result2.Block.Hash, result4.Block.Hash);

                var response5 = await _client.GetBlockTransfers(result2.Block.Hash);
                var result5 = response5.Parse();
                Assert.AreEqual(0, result5.Transfers.Count);
                
                var response6 = await _client.GetBlockTransfers((int)result2.Block.Header.Height);
                var result6 = response6.Parse();
                Assert.AreEqual(0, result6.Transfers.Count);
                
                var hash1 = await _client.GetStateRootHash(result2.Block.Hash);
                Assert.AreEqual(32*2, hash1.Length);

                var hash2 = await _client.GetStateRootHash((int)result2.Block.Header.Height);
                Assert.AreEqual(hash1, hash2);
                
                var hash3 = await _client.GetStateRootHash();
                Assert.AreEqual(32*2, hash3.Length);
            }
            catch (RpcClientException e)
            {
                Assert.Fail(e.RpcError.Message);
            }
            
            try
            {
                await _client.GetBlock(100000);
                Assert.Fail("Exception expected");
            }
            catch (RpcClientException e)
            {
                Assert.IsNotNull(e.RpcError);
                Assert.IsNotNull(e.RpcError.Message);
                Assert.AreNotEqual(0, e.RpcError.Code);
                Assert.IsNotNull(e.RpcError.Data);
            }
        }

        [Test]
        public async Task GetSystemBlockProposerTest()
        {
            try
            {
                var response = await _client.GetBlock(0);
                var result = response.Parse();
                Assert.IsNotNull(result.Block.Hash);
                Assert.IsTrue(result.Block.Body.Proposer.IsSystem);
                Assert.AreEqual(result.Block.Body.Proposer.IsSystem, result.Block.Body.Proposer.isSystem);
            }
            catch (RpcClientException e)
            {
                Assert.IsNotNull(e.RpcError);
                Assert.IsNotNull(e.RpcError.Message);
                Assert.AreNotEqual(0, e.RpcError.Code);
                Assert.IsNotNull(e.RpcError.Data);
            }
        }

        [Test]
        public async Task GetAuctionInfoTest()
        {
            try
            {
                var response = await _client.GetAuctionInfo();
                var auctionInfo = response.Parse();
                Assert.IsTrue(auctionInfo.AuctionState.Bids.Count > 0);

                var response2 = await _client.GetAuctionInfo(1);
                var auctionInfo2 = response2.Parse();
                Assert.IsTrue(auctionInfo2.AuctionState.Bids.Count > 0);
                
                var response3 = await _client.GetEraInfoBySwitchBlock(2);
                var eraInfo3 = response3.Parse();
                Assert.IsNotNull(eraInfo3);

                var response4 = await _client.GetEraInfoBySwitchBlock();
                var eraInfo4 = response4.Parse();
                Assert.IsNotNull(eraInfo4);
            }
            catch (RpcClientException e)
            {
                Assert.Fail(e.RpcError.Message);
            }
        }

        [Test]
        public async Task GetEraSummaryTest()
        {
            try
            {
                var response = await _client.GetEraSummary();
                var result = response.Parse();
                Assert.IsTrue(result.EraSummary.EraId > 0);
                Assert.IsNotNull(result.EraSummary.StoredValue.EraInfo);
                Assert.IsTrue(result.EraSummary.StoredValue.EraInfo.SeigniorageAllocations.Count > 0);
                
                var response2 = await _client.GetEraSummary(result.EraSummary.BlockHash);
                var result2 = response2.Parse();
                Assert.IsTrue(result2.EraSummary.EraId > 0);
                Assert.IsNotNull(result2.EraSummary.StoredValue.EraInfo);
                Assert.IsTrue(result2.EraSummary.StoredValue.EraInfo.SeigniorageAllocations.Count > 0);

                var response3 = await _client.GetBlock(result.EraSummary.BlockHash);
                var result3 = response3.Parse();
                Assert.IsNotNull(result3.Block.Hash);
                
                var response4 = await _client.GetEraSummary((int)result3.Block.Header.Height);
                var result4 = response4.Parse();
                Assert.IsTrue(result4.EraSummary.EraId > 0);
                Assert.IsNotNull(result4.EraSummary.StoredValue.EraInfo);
                Assert.IsTrue(result4.EraSummary.StoredValue.EraInfo.SeigniorageAllocations.Count > 0);
            }
            catch (RpcClientException e)
            {
                Assert.Fail(e.RpcError.Message);
            }
        }
        
        [Test]
        public async Task GetValidatorChangesTest()
        {
            try
            {
                var response = await _client.GetValidatorChanges();
                var changes = response.Parse();
                Assert.IsNotNull(changes.Changes);
            }
            catch (RpcClientException e)
            {
                Assert.Fail(e.RpcError.Message);
            }
        }
        
        [Test]
        public async Task GetRpcSchemaTest()
        {
            try
            {
                var schema = await _client.GetRpcSchema();
                Assert.IsNotEmpty(schema);

                var doc = JsonDocument.Parse(schema);
                Assert.IsNotNull(doc);
            }
            catch (RpcClientException e)
            {
                Assert.Fail(e.RpcError.Message);
            }
        }
        
        [Test]
        public async Task GetChainspecTest()
        {
            try
            {
                var response = await _client.GetChainspec();
                Assert.IsNotNull(response);

                var result = response.Parse();
                Assert.IsNotNull(result.ChainspecBytes.ChainspecBytes);
                Assert.IsNotNull(result.ChainspecBytes.ChainspecAsString);
            }
            catch (RpcClientException e)
            {
                Assert.Fail(e.RpcError.Message);
            }
        }
    }
}
