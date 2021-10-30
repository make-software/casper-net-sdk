using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetCasperSDK.JsonRpc;
using NetCasperSDK.Types;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperSDK
{
    public class NetCasperClient
    {
        private readonly RpcClient _rpcClient;

        public NetCasperClient(string nodeAddress, RpcLoggingHandler loggingHandler = null)
        {
            _rpcClient = new RpcClient(nodeAddress, loggingHandler);
        }

        private Task<RpcResponse> SendRpcRequestAsync(RpcMethod method)
        {
            return _rpcClient.SendRpcRequestAsync(method);
        }

        public async Task<string> GetStateRootHash()
        {
            var method = new GetStateRootHashMethod();
            var rpcResponse = await SendRpcRequestAsync(method);
            return rpcResponse.Result.GetProperty("state_root_hash").GetString();
        }

        public async Task<string> GetStateRootHash(string blockHash)
        {
            var method = new GetStateRootHashMethod(blockHash);
            var rpcResponse = await SendRpcRequestAsync(method);
            return rpcResponse.Result.GetProperty("state_root_hash").GetString();
        }
        

        public async Task<string> GetStateRootHash(int blockHeight)
        {
            var method = new GetStateRootHashMethod(blockHeight);
            var rpcResponse = await SendRpcRequestAsync(method);
            return rpcResponse.Result.GetProperty("state_root_hash").GetString();
        }

        public async Task<RpcResponse> GetNodeStatus()
        {
            var method = new GetNodeStatus();
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetNodePeers()
        {
            var method = new GetNodePeers();
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetAuctionInfo()
        {
            var method = new GetAuctionInfo();
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetAccountInfo(string accountKey, string stateRootHash=null)
        {
            if (!accountKey.StartsWith("account-hash-"))
                accountKey = "account-hash-" + 
                             Hex.ToHexString(PublicKey.FromHexString(accountKey).GetAccountHash());

            if (stateRootHash == null)
                stateRootHash = await GetStateRootHash();
            
            var method = new GetItemMethod(accountKey, stateRootHash);
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetAccountInfo(PublicKey publicKey, List<string> path=null, string stateRootHash=null)
        {
            var accountHash = "account-hash-" + Hex.ToHexString(publicKey.GetAccountHash());

            if (stateRootHash == null)
                stateRootHash = await GetStateRootHash();
            
            var method = new GetItemMethod(accountHash, stateRootHash, path);
            return await SendRpcRequestAsync(method);
        }
        
        public async Task<RpcResponse> QueryState(string keyHash, List<string> path=null, string stateRootHash=null)
        {
            if (stateRootHash == null)
                stateRootHash = await GetStateRootHash();
            
            var method = new GetItemMethod(keyHash, stateRootHash, path);
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> QueryGlobalState(string key, string stateRootHash,
            List<string> path = null)
        {
            var method = new QueryGlobalState(key, stateRootHash, isBlockHash:false, path);
            return await SendRpcRequestAsync(method);
        }
        
        public async Task<RpcResponse> QueryGlobalStateWithBlockHash(string key, string blockHash,
            List<string> path = null)
        {
            var method = new QueryGlobalState(key, blockHash, isBlockHash:true, path);
            return await SendRpcRequestAsync(method);
        }
        
        public async Task<RpcResponse> GetAccountBalance(string purseURef, string stateRootHash=null)
        {
            if (!purseURef.StartsWith("uref-"))
            {
                var response = await GetAccountInfo(purseURef);
                purseURef = response.Result.GetProperty("stored_value").GetProperty("Account")
                    .GetProperty("main_purse").GetString();   
            }
            
            if (stateRootHash == null)
                stateRootHash = await GetStateRootHash();
            
            var method = new GetBalanceMethod(purseURef, stateRootHash);
            return await SendRpcRequestAsync(method);
        }
        
        public async Task<RpcResponse> GetAccountBalance(PublicKey publicKey)
        {
            var response = await GetAccountInfo(publicKey);
            var purseUref = response.Result.GetProperty("stored_value").GetProperty("Account")
                .GetProperty("main_purse").GetString();
            return GetAccountBalance(purseUref).Result;    
        }

        public async Task<RpcResponse> PutDeploy(Deploy deploy)
        {
            if (deploy.Approvals.Count == 0)
                throw new Exception("Sign the deploy before sending it to the network.");

            var method = new PutDeployMethod(deploy);
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetDeploy(string deployHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = new GetDeployMethod(deployHash);

            while (!cancellationToken.IsCancellationRequested)
            {
                    var response = await SendRpcRequestAsync(method);
                    if (!cancellationToken.CanBeCanceled ||
                        response.Result.GetProperty("execution_results").GetArrayLength() > 0)
                        return response;
                    Thread.Sleep(10000);
            }
            throw new TaskCanceledException("GetDeploy operation canceled");
        }

        public async Task<RpcResponse> GetBlock()
        {
            return await GetBlock(null);
        }
        
        public async Task<RpcResponse> GetBlock(string blockHash)
        {
            var method = new GetBlockMethod(blockHash);
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetBlock(int blockHeight)
        {
            var method = new GetBlockMethod(blockHeight);
            return await SendRpcRequestAsync(method);
        }
        
        public async Task<RpcResponse> GetBlockTransfers()
        {
            return await GetBlockTransfers(null);
        }

        public async Task<RpcResponse> GetBlockTransfers(string blockHash)
        {
            var method = new GetBlockTransfersMethod(blockHash);
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetBlockTransfers(int blockHeight)
        {
            var method = new GetBlockTransfersMethod(blockHeight);
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetEraInfoBySwitchBlock()
        {
            return await GetEraInfoBySwitchBlock(null);
        }

        public async Task<RpcResponse> GetEraInfoBySwitchBlock(string blockHash)
        {
            var method = new GetEraInfoBySwitchBlockMethod(blockHash);
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetEraInfoBySwitchBlock(int blockHeight)
        {
            var method = new GetEraInfoBySwitchBlockMethod(blockHeight);
            return await SendRpcRequestAsync(method);
        }

        public async Task<RpcResponse> GetValidatorChanges()
        {
            var method = new GetValidatorChanges();
            return await SendRpcRequestAsync(method);
        }
        
        public async Task<RpcResponse> GetRpcSchema()
        {
            var method = new GetRpcSchema();
            return await SendRpcRequestAsync(method);
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