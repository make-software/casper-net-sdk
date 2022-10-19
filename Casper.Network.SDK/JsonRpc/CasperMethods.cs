using System.Collections.Generic;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc
{
    public class GetStateRootHash : RpcMethod
    {
        /// <summary>
        /// Returns a state root hash at a given Block
        /// </summary>
        /// <param name="blockHash">Block hash for which the state root is queried. Null for the most recent.</param>
        public GetStateRootHash(string blockHash = null) : base("chain_get_state_root_hash", blockHash)
        {
        }

        /// <summary>
        /// Returns the state root hash at a given Block
        /// </summary>
        /// <param name="height">Block height for which the state root is queried.</param>
        public GetStateRootHash(int height) : base("chain_get_state_root_hash", height)
        {
        }
    }

    public class GetNodeStatus : RpcMethod
    {
        /// <summary>
        /// Returns the current status of the node.
        /// </summary>
        public GetNodeStatus() : base("info_get_status")
        {
        }
    }

    public class GetNodePeers : RpcMethod
    {
        /// <summary>
        /// Returns a list of peers connected to the node.
        /// </summary>
        public GetNodePeers() : base("info_get_peers")
        {
        }
    }

    public class GetAuctionInfo : RpcMethod
    {
        /// <summary>
        /// Returns the bids and validators at a given block.
        /// </summary>
        /// <param name="blockHash">Block hash for which the auction info is queried. Null for the most recent auction info.</param>
        public GetAuctionInfo(string blockHash) : base("state_get_auction_info", blockHash)
        {
        }

        /// <summary>
        /// Returns the bids and validators at a given block.
        /// </summary>
        /// <param name="height">Block height for which the auction info is queried.</param>
        public GetAuctionInfo(int height) : base("state_get_auction_info", height)
        {
        }
    }

    public class GetAccountInfo : RpcMethod
    {
        /// <summary>
        /// Returns the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account.</param>
        /// <param name="blockHash">A block hash for which the information of the account is queried. Null for most recent information.</param>
        public GetAccountInfo(string publicKey, string blockHash = null) : base("state_get_account_info", blockHash)
        {
            this.Parameters.Add("public_key", publicKey);
        }

        /// <summary>
        /// Returns the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account.</param>
        /// <param name="height">A block height for which the information of the account is queried.</param>
        public GetAccountInfo(string publicKey, int height) : base("state_get_account_info", height)
        {
            this.Parameters.Add("public_key", publicKey);
        }
    }
    
    public class GetItem : RpcMethod
    {
        /// <summary>
        /// Returns a stored value from the network. This RPC is deprecated, use `QueryGlobalState` instead.
        /// </summary>
        /// <param name="key">GlobalStateKey as a string.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        public GetItem(string key, string stateRootHash, List<string> path = null) : base("state_get_item")
        {
            if (path == null) path = new List<string>();
            this.Parameters = new Dictionary<string, object>
            {
                {"state_root_hash", stateRootHash},
                {"path", path},
                {"key", key}
            };
        }
    }

    public class QueryGlobalState : RpcMethod
    {
        /// <summary>
        /// A query to the global state that returns a stored value from the network.
        /// </summary>
        /// <param name="key">A global state key formatted as a string to query the value from the network.</param>
        /// <param name="hash">A block hash or a state root hash.</param>
        /// <param name="isBlockHash">true if hash is a Block hash. False for state root hash.</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        public QueryGlobalState(string key, string hash, bool isBlockHash, string[] path = null) :
            base("query_global_state")
        {
            Dictionary<string, string> stateIdentifier = new Dictionary<string, string>
            {
                {isBlockHash ? "BlockHash" : "StateRootHash", hash}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"state_identifier", stateIdentifier},
                {"path", path ?? new string[] {}},
                {"key", key}
            };
        }
    }

    public class GetBalance : RpcMethod
    {
        /// <summary>
        /// Returns a purse's balance from the network.
        /// </summary>
        /// <param name="purseURef">Purse URef formatted as a string.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetBalance(string purseURef, string stateRootHash) : base("state_get_balance")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"state_root_hash", stateRootHash},
                {"purse_uref", purseURef}
            };
        }
    }

    public class PutDeploy : RpcMethod
    {
        /// <summary>
        /// Sends a Deploy to the network for its execution.
        /// </summary>
        /// <param name="deploy">The deploy object.</param>
        public PutDeploy(Deploy deploy) : base("account_put_deploy")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"deploy", deploy}
            };
        }
    }

    public class GetDeploy : RpcMethod
    {
        /// <summary>
        /// Retrieves a Deploy from the network.
        /// </summary>
        /// <param name="deployHash">Hash of the deploy to retrieve.</param>
        /// <param name="finalizedApprovals">Whether to return the deploy with the finalized approvals
        /// substituted. If `false` or omitted, returns the deploy with the approvals that were originally
        /// received by the node.</param>
        public GetDeploy(string deployHash, bool finalizedApprovals = false) : base("info_get_deploy")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"deploy_hash", deployHash}
            };
            
            if(finalizedApprovals)
                this.Parameters.Add("finalized_approvals", true);
        }
    }

    public class GetBlock : RpcMethod
    {
        /// <summary>
        /// Retrieves a Block from the network by its hash. 
        /// </summary>
        /// <param name="blockHash">Hash of the block to retrieve. Null for the most recent block.</param>
        public GetBlock(string blockHash) : base("chain_get_block", blockHash)
        {
        }

        /// <summary>
        /// Retrieves a Block from the network by its height number.
        /// </summary>
        /// <param name="height">Height of the block to retrieve.</param>
        public GetBlock(int height) : base("chain_get_block", height)
        {
        }
    }

    public class GetBlockTransfers : RpcMethod
    {
        /// <summary>
        /// Retrieves all transfers for a Block from the network
        /// </summary>
        /// <param name="blockHash">Hash of the block to retrieve the transfers from. Null for the most recent block</param>
        public GetBlockTransfers(string blockHash) : base("chain_get_block_transfers", blockHash)
        {
        }

        /// <summary>
        /// Retrieves all transfers for a Block from the network
        /// </summary>
        /// <param name="height">Height of the block to retrieve the transfers from.</param>
        public GetBlockTransfers(int height) : base("chain_get_block_transfers", height)
        {
        }
    }

    public class GetEraInfoBySwitchBlock : RpcMethod
    {
        /// <summary>
        /// Retrieves an EraInfo from the network given a switch block 
        /// </summary>
        /// <param name="blockHash">Block hash of a switch block.</param>
        public GetEraInfoBySwitchBlock(string blockHash) : base("chain_get_era_info_by_switch_block", blockHash)
        {
        }

        /// <summary>
        /// Retrieves an EraInfo from the network given a switch block.
        /// </summary>
        /// <param name="height">Block height of a switch block.</param>
        public GetEraInfoBySwitchBlock(int height) : base("chain_get_era_info_by_switch_block", height)
        {
        }
    }

    public class GetDictionaryItem : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item from its dictionary item key..
        /// </summary>
        /// <param name="dictionaryItem">The dictionary item key to retrieve.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetDictionaryItem(string dictionaryItem, string stateRootHash) : base("state_get_dictionary_item")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"dictionary_identifier", new Dictionary<string, object>
                {
                    {"Dictionary", dictionaryItem}
                }},
                {"state_root_hash", stateRootHash},
            };
        }
    }
    
    public class GetDictionaryItemByAccount : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via an Account's named keys.
        /// </summary>
        /// <param name="accountKey">The account key as a formatted string whose named keys contains dictionaryName.</param>
        /// <param name="dictionaryName">The named key under which the dictionary seed URef is stored.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetDictionaryItemByAccount(string accountKey, string dictionaryName, string dictionaryItem,
            string stateRootHash) : base("state_get_dictionary_item")
        {
            var accountNamedKey = new Dictionary<string, string>
            {
                {"key", accountKey},
                {"dictionary_name", dictionaryName},
                {"dictionary_item_key", dictionaryItem}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {
                    "dictionary_identifier", new Dictionary<string, object>
                    {
                        {"AccountNamedKey", accountNamedKey}
                    }
                },
                {"state_root_hash", stateRootHash},
            };
        }
    }

    public class GetDictionaryItemByContract : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via a Contract named keys.
        /// </summary>
        /// <param name="contractKey">The contract key as a formatted string whose named keys contains dictionaryName.</param>
        /// <param name="dictionaryName">The named key under which the dictionary seed URef is stored.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetDictionaryItemByContract(string contractKey, string dictionaryName, string dictionaryItem, string stateRootHash) : base("state_get_dictionary_item")
        {
            var contractNamedKey = new Dictionary<string, string>
            {
                {"key", contractKey},
                {"dictionary_name", dictionaryName},
                {"dictionary_item_key", dictionaryItem}
            };
            
            this.Parameters = new Dictionary<string, object>
            {
                {"dictionary_identifier", new Dictionary<string, object>
                {
                    {"ContractNamedKey", contractNamedKey}
                }},
                {"state_root_hash", stateRootHash},
            };
        }
    }

    public class GetDictionaryItemByURef : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via its seed URef.
        /// </summary>
        /// <param name="seedURef">The dictionary's seed URef.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetDictionaryItemByURef(string seedURef, string dictionaryItem, string stateRootHash) : base("state_get_dictionary_item")
        {
            var contractNamedKey = new Dictionary<string, string>
            {
                {"seed_uref", seedURef},
                {"dictionary_item_key", dictionaryItem}
            };
            
            this.Parameters = new Dictionary<string, object>
            {
                {"dictionary_identifier", new Dictionary<string, object>
                {
                    {"URef", contractNamedKey}
                }},
                {"state_root_hash", stateRootHash},
            };
        }
    }

    public class GetValidatorChanges : RpcMethod
    {
        /// <summary>
        /// Returns status changes of active validators
        /// </summary>
        public GetValidatorChanges() : base("info_get_validator_changes")
        {
        }
    }

    public class GetRpcSchema : RpcMethod
    {
        /// <summary>
        /// Returns the RPC Json schema.
        /// </summary>
        public GetRpcSchema() : base("rpc.discover")
        {
        }
    }
}
