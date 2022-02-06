using System.Threading.Tasks;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    [Category("NCTL"), NonParallelizable]
    public class NctlQueryGlobalStateTest : NctlBase
    {
        private URef _mainPurse = null;
        
        [Test, Order(1)]
        public async Task QueryAccountHash()
        {
            var accountHash = GlobalStateKey.FromString(_faucetKey.PublicKey.GetAccountHash());

            var rpcResponse = await _client.QueryGlobalState(accountHash);
            var account = rpcResponse.Parse().StoredValue.Account;
            
            Assert.AreEqual(accountHash.ToHexString(), account.AccountHash.ToHexString());

            _mainPurse = account.MainPurse;
        }
        
        [Test, Order(2)]
        public async Task QueryURef()
        {
            Assert.IsNotNull(_mainPurse,  "This test must run after QueryAccountHash");
            
            var rpcResponse = await _client.QueryGlobalState(_mainPurse);
            var clValue = rpcResponse.Parse().StoredValue.CLValue;
            Assert.IsNotNull(clValue);
        }

        [Test]
        public async Task QueryWrongKey()
        {
            var key = GlobalStateKey.FromString(
                "uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007");

            try
            {
                await _client.QueryGlobalState(key);
                Assert.Fail("Exception expected");
            }
            catch (RpcClientException e)
            {
                Assert.IsNotNull(e.RpcError);
                Assert.IsNotNull(e.RpcError.Message);
            }
        }
    }
}