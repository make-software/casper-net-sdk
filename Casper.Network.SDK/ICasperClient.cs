using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK
{
    public interface ICasperClient
    {
        Task<string> GetStateRootHash(string blockHash = null);

        Task<string> GetStateRootHash(int blockHeight);

        Task<RpcResponse<GetNodeStatusResult>> GetNodeStatus();

        Task<RpcResponse<GetNodePeersResult>> GetNodePeers();

        Task<RpcResponse<GetAuctionInfoResult>> GetAuctionInfo(string blockHash = null);

        Task<RpcResponse<GetAuctionInfoResult>> GetAuctionInfo(int blockHeight);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(PublicKey publicKey, string blockHash = null);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(string publicKey, string blockHash = null);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(PublicKey publicKey, int blockHeight);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(string publicKey, int blockHeight);

        Task<RpcResponse<GetEntityResult>> GetEntity(IEntityIdentifier entityIdentifier, string blockHash = null);

        Task<RpcResponse<GetEntityResult>> GetEntity(IEntityIdentifier entityIdentifier, ulong blockHeight);

        Task<RpcResponse<GetEntityResult>> GetEntity(string entityAddr, string blockHash = null);

        Task<RpcResponse<GetEntityResult>> GetEntity(string entityAddr, ulong blockHeight);

        Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(string key, string stateRootHash = null,
            string path = null);
        
        Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(GlobalStateKey key, string stateRootHash = null,
            string path = null);

        Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalStateWithBlockHash(string key, string blockHash,
            string path = null);

        Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalStateWithBlockHash(GlobalStateKey key, string blockHash, 
            string path = null);

        Task<RpcResponse<GetBalanceResult>> GetAccountBalance(string purseURef,
            string stateRootHash = null);

        Task<RpcResponse<GetBalanceResult>> GetAccountBalance(URef purseURef,
            string stateRootHash = null);

        Task<RpcResponse<GetBalanceResult>> GetAccountBalance(PublicKey publicKey,
            string stateRootHash = null);

        Task<RpcResponse<QueryBalanceDetailsResult>> QueryBalanceDetails(IPurseIdentifier purseIdentifier,
            string blockHash = null);

        Task<RpcResponse<QueryBalanceDetailsResult>> QueryBalanceDetails(IPurseIdentifier purseIdentifier,
            ulong blockHeight);

        Task<RpcResponse<QueryBalanceDetailsResult>> QueryBalanceDetailsWithStateRootHash(
            IPurseIdentifier purseIdentifier, string stateRootHash);
        
        Task<RpcResponse<PutDeployResult>> PutDeploy(Deploy deploy);

        Task<RpcResponse<GetDeployResult>> GetDeploy(string deployHash,
            CancellationToken cancellationToken = default(CancellationToken));
        
        Task<RpcResponse<GetTransactionResult>> GetTransaction(TransactionHash transactionHash,
            bool finalizedApprovals = false,
            CancellationToken cancellationToken = default(CancellationToken));
        
        Task<RpcResponse<GetBlockResult>> GetBlock(string blockHash = null);

        Task<RpcResponse<GetBlockResult>> GetBlock(int blockHeight);

        Task<RpcResponse<GetBlockTransfersResult>> GetBlockTransfers(string blockHash = null);
        
        Task<RpcResponse<GetBlockTransfersResult>> GetBlockTransfers(int blockHeight);

        Task<RpcResponse<GetEraInfoBySwitchBlockResult>> GetEraInfoBySwitchBlock(string blockHash = null);

        Task<RpcResponse<GetEraInfoBySwitchBlockResult>> GetEraInfoBySwitchBlock(int blockHeight);

        Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItem(string dictionaryItem, string stateRootHash = null);

        Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByAccount(string accountKey, string dictionaryName,
            string dictionaryItem, string stateRootHash = null);
        
        Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByContract(string contractKey, string dictionaryName, 
            string dictionaryItem, string stateRootHash = null);

        Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByURef(string seedURef,
            string dictionaryItem, string stateRootHash = null);

        Task<RpcResponse<GetValidatorChangesResult>> GetValidatorChanges();

        Task<string> GetRpcSchema();
    }
}
