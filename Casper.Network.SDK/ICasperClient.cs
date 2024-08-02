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

        Task<string> GetStateRootHash(ulong blockHeight);

        Task<RpcResponse<GetNodeStatusResult>> GetNodeStatus();

        Task<RpcResponse<GetNodePeersResult>> GetNodePeers();

        Task<RpcResponse<GetAuctionInfoResult>> GetAuctionInfo(string blockHash = null);

        Task<RpcResponse<GetAuctionInfoResult>> GetAuctionInfo(ulong blockHeight);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(PublicKey publicKey, string blockHash = null);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(string publicKey, string blockHash = null);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(PublicKey publicKey, ulong blockHeight);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(string publicKey, ulong blockHeight);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(AccountHashKey accountHash, string blockHash = null);

        Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(AccountHashKey accountHash, ulong blockHeight);

        Task<RpcResponse<GetEntityResult>> GetEntity(IEntityIdentifier entityIdentifier, string blockHash = null);

        Task<RpcResponse<GetEntityResult>> GetEntity(IEntityIdentifier entityIdentifier, ulong blockHeight);

        Task<RpcResponse<GetEntityResult>> GetEntity(string entityAddr, string blockHash = null);

        Task<RpcResponse<GetEntityResult>> GetEntity(string entityAddr, ulong blockHeight);

        Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(string key, ulong height,
            string path = null);
        Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(string key, string stateRootHash = null,
            string path = null);
        
        Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(GlobalStateKey key, string stateRootHash = null,
            string path = null);

        Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalStateWithBlockHash(string key, string blockHash,
            string path = null);

        Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalStateWithBlockHash(GlobalStateKey key, string blockHash, 
            string path = null);

        Task<RpcResponse<GetBalanceResult>> GetBalance(string purseURef,
            string stateRootHash = null);

        Task<RpcResponse<QueryBalanceResult>> QueryBalance(IPurseIdentifier purseIdentifier,
            string blockHash = null);

        Task<RpcResponse<QueryBalanceResult>> QueryBalance(IPurseIdentifier purseIdentifier,
            ulong blockHeight);

        Task<RpcResponse<QueryBalanceResult>> QueryBalanceWithStateRootHash(
            IPurseIdentifier purseIdentifier, string stateRootHash);
        
        Task<RpcResponse<QueryBalanceDetailsResult>> QueryBalanceDetails(IPurseIdentifier purseIdentifier,
            string blockHash = null);

        Task<RpcResponse<QueryBalanceDetailsResult>> QueryBalanceDetails(IPurseIdentifier purseIdentifier,
            ulong blockHeight);

        Task<RpcResponse<QueryBalanceDetailsResult>> QueryBalanceDetailsWithStateRootHash(
            IPurseIdentifier purseIdentifier, string stateRootHash);
        
        Task<RpcResponse<PutDeployResult>> PutDeploy(Deploy deploy);

        Task<RpcResponse<GetDeployResult>> GetDeploy(string deployHash,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<RpcResponse<GetDeployResult>> GetDeploy(string deployHash,
            bool finalizedApprovals,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<RpcResponse<PutTransactionResult>> PutTransaction(TransactionV1 transaction);
        
        Task<RpcResponse<GetTransactionResult>> GetTransaction(TransactionHash transactionHash,
            bool finalizedApprovals = false,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<RpcResponse<GetTransactionResult>> GetTransaction(string version1Hash,
            bool finalizedApprovals = false,
            CancellationToken cancellationToken = default(CancellationToken));
        
        Task<RpcResponse<GetBlockResult>> GetBlock(string blockHash = null);

        Task<RpcResponse<GetBlockResult>> GetBlock(ulong blockHeight);

        Task<RpcResponse<GetBlockTransfersResult>> GetBlockTransfers(string blockHash = null);
        
        Task<RpcResponse<GetBlockTransfersResult>> GetBlockTransfers(ulong blockHeight);

        Task<RpcResponse<GetEraInfoBySwitchBlockResult>> GetEraInfoBySwitchBlock(string blockHash = null);

        Task<RpcResponse<GetEraInfoBySwitchBlockResult>> GetEraInfoBySwitchBlock(ulong blockHeight);

        Task<RpcResponse<GetEraSummaryResult>> GetEraSummary(string blockHash = null);

        Task<RpcResponse<GetEraSummaryResult>> GetEraSummary(ulong blockHeight);
        
        Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItem(string dictionaryItem, string stateRootHash = null);

        Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByAccount(string accountKey, string dictionaryName,
            string dictionaryItem, string stateRootHash = null);
        
        Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByContract(string contractKey, string dictionaryName, 
            string dictionaryItem, string stateRootHash = null);

        Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByURef(string seedURef,
            string dictionaryItem, string stateRootHash = null);

        Task<RpcResponse<GetValidatorChangesResult>> GetValidatorChanges();

        Task<RpcResponse<GetRewardResult>> GetValidatorReward(PublicKey validator, string blockHash = null);

        Task<RpcResponse<GetRewardResult>> GetValidatorReward(PublicKey validator, ulong blockHeight);

        Task<RpcResponse<GetRewardResult>> GetValidatorRewardWithEraId(PublicKey validator, ulong eraId);

        Task<RpcResponse<GetRewardResult>> GetDelegatorReward(PublicKey validator, PublicKey delegator,
            string blockHash = null);

        Task<RpcResponse<GetRewardResult>> GetDelegatorReward(PublicKey validator, PublicKey delegator,
            ulong blockHeight);

        Task<RpcResponse<GetRewardResult>> GetDelegatorRewardWithEraId(PublicKey validator, PublicKey delegator,
            ulong eraId);
        
        Task<string> GetRpcSchema();

        Task<RpcResponse<GetChainspecResult>> GetChainspec();

        Task<RpcResponse<SpeculativeExecutionResult>> SpeceulativeExecution(Deploy deploy, string stateRootHash = null);

        Task<RpcResponse<PutDeployResult>> SpeceulativeExecutionWithBlockHash(Deploy deploy, string blockHash = null);
    }
}
