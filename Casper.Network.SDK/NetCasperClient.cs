using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK
{
    public class NetCasperClient
    {
        private readonly RpcClient _rpcClient;

        public NetCasperClient(string nodeAddress, RpcLoggingHandler loggingHandler = null)
        {
            _rpcClient = new RpcClient(nodeAddress, loggingHandler);
        }

        private Task<RpcResponse<TRpcResult>> SendRpcRequestAsync<TRpcResult>(RpcMethod method)
        {
            return _rpcClient.SendRpcRequestAsync<TRpcResult>(method);
        }

        public async Task<string> GetStateRootHash()
        {
            var method = new GetStateRootHashMethod();
            var rpcResponse = await SendRpcRequestAsync<GetStateRootHashResult>(method);
            return rpcResponse.Result.GetProperty("state_root_hash").GetString();
        }

        public async Task<string> GetStateRootHash(string blockHash)
        {
            var method = new GetStateRootHashMethod(blockHash);
            var rpcResponse = await SendRpcRequestAsync<GetStateRootHashResult>(method);
            return rpcResponse.Result.GetProperty("state_root_hash").GetString();
        }

        public async Task<string> GetStateRootHash(int blockHeight)
        {
            var method = new GetStateRootHashMethod(blockHeight);
            var rpcResponse = await SendRpcRequestAsync<GetStateRootHashResult>(method);
            return rpcResponse.Result.GetProperty("state_root_hash").GetString();
        }

        public async Task<RpcResponse<GetNodeStatusResult>> GetNodeStatus()
        {
            var method = new GetNodeStatus();
            return await SendRpcRequestAsync<GetNodeStatusResult>(method);
        }

        public async Task<RpcResponse<GetNodePeersResult>> GetNodePeers()
        {
            var method = new GetNodePeers();
            return await SendRpcRequestAsync<GetNodePeersResult>(method);
        }

        public async Task<RpcResponse<GetAuctionInfoResult>> GetAuctionInfo()
        {
            var method = new GetAuctionInfo();
            return await SendRpcRequestAsync<GetAuctionInfoResult>(method);
        }

        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(PublicKey publicKey,
            string blockHash = null)
        {
            var method = new GetAccountInfo(publicKey.ToAccountHex(), blockHash);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(string publicKey, string blockHash = null)
        {
            var method = new GetAccountInfo(publicKey, blockHash);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(PublicKey publicKey, int height)
        {
            var method = new GetAccountInfo(publicKey.ToAccountHex(), height);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(string publicKey, int height)
        {
            var method = new GetAccountInfo(publicKey, height);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        public async Task<RpcResponse<GetItemResult>> QueryState(string keyHash, List<string> path = null,
            string stateRootHash = null)
        {
            if (stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetItemMethod(keyHash, stateRootHash, path);
            return await SendRpcRequestAsync<GetItemResult>(method);
        }

        public async Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(string key, string stateRootHash = null,
            string path = null)
        {            
            if (stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new QueryGlobalState(key, stateRootHash, isBlockHash: false, path?.Split(new char[] {'/'}));
            return await SendRpcRequestAsync<QueryGlobalStateResult>(method);
        }

        public async Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(GlobalStateKey key, string stateRootHash = null,
            string path = null)
        {
            return await QueryGlobalState(key.ToString(), stateRootHash, path);
        }
        
        public async Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalStateWithBlockHash(string key, string blockHash,
            string path = null)
        {
            var method = new QueryGlobalState(key, blockHash, isBlockHash: true, path?.Split(new char[] {'/'}));
            return await SendRpcRequestAsync<QueryGlobalStateResult>(method);
        }

        public async Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalStateWithBlockHash(GlobalStateKey key,
            string blockHash, string path = null)
        {
            return await QueryGlobalStateWithBlockHash(key.ToString(), blockHash, path);
        }
        
        public async Task<RpcResponse<GetBalanceResult>> GetAccountBalance(string purseURef,
            string stateRootHash = null)
        {
            if (!purseURef.StartsWith("uref-"))
            {
                var response = await GetAccountInfo(purseURef);
                purseURef = response.Result.GetProperty("account")
                    .GetProperty("main_purse").GetString();
            }

            if (stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetBalanceMethod(purseURef, stateRootHash);
            return await SendRpcRequestAsync<GetBalanceResult>(method);
        }

        public async Task<RpcResponse<GetBalanceResult>> GetAccountBalance(PublicKey publicKey)
        {
            var response = await GetAccountInfo(publicKey);
            var purseUref = response.Result.GetProperty("account")
                .GetProperty("main_purse").GetString();
            return GetAccountBalance(purseUref).Result;
        }

        public async Task<RpcResponse<PutDeployResult>> PutDeploy(Deploy deploy)
        {
            if (deploy.Approvals.Count == 0)
                throw new Exception("Sign the deploy before sending it to the network.");

            var method = new PutDeployMethod(deploy);
            return await SendRpcRequestAsync<PutDeployResult>(method);
        }

        public async Task<RpcResponse<GetDeployResult>> GetDeploy(string deployHash,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = new GetDeployMethod(deployHash);

            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await SendRpcRequestAsync<GetDeployResult>(method);
                if (!cancellationToken.CanBeCanceled ||
                    response.Result.GetProperty("execution_results").GetArrayLength() > 0)
                    return response;
                Thread.Sleep(10000);
            }

            throw new TaskCanceledException("GetDeploy operation canceled");
        }

        public async Task<RpcResponse<GetBlockResult>> GetBlock(string blockHash = null)
        {
            var method = new GetBlockMethod(blockHash);
            return await SendRpcRequestAsync<GetBlockResult>(method);
        }

        public async Task<RpcResponse<GetBlockResult>> GetBlock(int blockHeight)
        {
            var method = new GetBlockMethod(blockHeight);
            return await SendRpcRequestAsync<GetBlockResult>(method);
        }

        public async Task<RpcResponse<GetBlockTransfersResult>> GetBlockTransfers(string blockHash = null)
        {
            var method = new GetBlockTransfersMethod(blockHash);
            return await SendRpcRequestAsync<GetBlockTransfersResult>(method);
        }

        public async Task<RpcResponse<GetBlockTransfersResult>> GetBlockTransfers(int blockHeight)
        {
            var method = new GetBlockTransfersMethod(blockHeight);
            return await SendRpcRequestAsync<GetBlockTransfersResult>(method);
        }

        public async Task<RpcResponse<GetEraInfoBySwitchBlockResult>> GetEraInfoBySwitchBlock(string blockHash = null)
        {
            var method = new GetEraInfoBySwitchBlockMethod(blockHash);
            return await SendRpcRequestAsync<GetEraInfoBySwitchBlockResult>(method);
        }

        public async Task<RpcResponse<GetEraInfoBySwitchBlockResult>> GetEraInfoBySwitchBlock(int blockHeight)
        {
            var method = new GetEraInfoBySwitchBlockMethod(blockHeight);
            return await SendRpcRequestAsync<GetEraInfoBySwitchBlockResult>(method);
        }

        public async Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItem(string dictionaryItem, string stateRootHash = null)
        {
            if(stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetDictionaryItem(dictionaryItem, stateRootHash);
            return await SendRpcRequestAsync<GetDictionaryItemResult>(method);
        }

        public async Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByAccount(string accountKey, string dictionaryName, 
            string dictionaryItem, string stateRootHash = null)
        {
            if(stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetDictionaryItemByAccount(accountKey, dictionaryName, dictionaryItem, stateRootHash);
            return await SendRpcRequestAsync<GetDictionaryItemResult>(method);
        }
        
        public async Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByContract(string contractKey, string dictionaryName, 
            string dictionaryItem, string stateRootHash = null)
        {
            if(stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetDictionaryItemByContract(contractKey, dictionaryName, dictionaryItem, stateRootHash);
            return await SendRpcRequestAsync<GetDictionaryItemResult>(method);
        }
        
        public async Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByURef(string seedURef, 
            string dictionaryItem, string stateRootHash = null)
        {
            if(stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetDictionaryItemByURef(seedURef, dictionaryItem, stateRootHash);
            return await SendRpcRequestAsync<GetDictionaryItemResult>(method);
        }
        
        public async Task<RpcResponse<GetValidatorChangesResult>> GetValidatorChanges()
        {
            var method = new GetValidatorChanges();
            return await SendRpcRequestAsync<GetValidatorChangesResult>(method);
        }

        public async Task<string> GetRpcSchema()
        {
            var method = new GetRpcSchema();
            var response = await SendRpcRequestAsync<RpcResult>(method);
            return response.Result.GetRawText();
        }

        public static async Task<string> GetNodeMetrics(string nodeAddress)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                var metrics = new StringBuilder();

                using (var streamReader =
                    new StreamReader(await client.GetStreamAsync(nodeAddress)))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var message = await streamReader.ReadLineAsync();
                        metrics.AppendLine(message);
                    }
                }

                return metrics.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not retrive metrics from {nodeAddress}", ex);
            }
        }

        public static async Task<string> GetNodeMetrics(string host, int port)
        {
            var uriBuilder = new UriBuilder("http", host, port, "metrics");
            return await GetNodeMetrics(uriBuilder.Uri.ToString());
        }
    }
}