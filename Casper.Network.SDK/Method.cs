using System.Collections.Generic;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK
{
    public class GetStateRootHashMethod : RpcMethod
    {
        public GetStateRootHashMethod() : base("chain_get_state_root_hash")
        {
        }

        public GetStateRootHashMethod(string blockHash) : base("chain_get_state_root_hash")
        {
            var blockIdentifier = new Dictionary<string, string>
            {
                {"Hash", blockHash}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockHash != null ? blockIdentifier : null}
            };
        }

        public GetStateRootHashMethod(int height) : base("chain_get_state_root_hash")
        {
            var blockIdentifier = new Dictionary<string, int>
            {
                {"Height", height}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockIdentifier}
            };
        }
    }

    public class GetNodeStatus : RpcMethod
    {
        public GetNodeStatus() : base("info_get_status")
        {
        }
    }

    public class GetNodePeers : RpcMethod
    {
        public GetNodePeers() : base("info_get_peers")
        {
        }
    }

    public class GetAuctionInfo : RpcMethod
    {
        public GetAuctionInfo() : base("state_get_auction_info")
        {
        }
    }

    public class GetAccountInfo : RpcMethod
    {
        public GetAccountInfo(string publicKey, string blockHash = null) : base("state_get_account_info")
        {
            var blockIdentifier = new Dictionary<string, string>
            {
                {"Hash", blockHash}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockHash != null ? blockIdentifier : null},
                {"public_key", publicKey}
            };
        }

        public GetAccountInfo(string publicKey, int height) : base("state_get_account_info")
        {
            var blockIdentifier = new Dictionary<string, int>
            {
                {"Height", height}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockIdentifier},
                {"public_key", publicKey}
            };
        }
    }
    
    public class GetItemMethod : RpcMethod
    {
        public GetItemMethod(string key, string state_root_hash, List<string> path = null) : base("state_get_item")
        {
            if (path == null) path = new List<string>();
            this.Parameters = new Dictionary<string, object>
            {
                {"state_root_hash", state_root_hash},
                {"path", path},
                {"key", key}
            };
        }
    }

    public class QueryGlobalState : RpcMethod
    {
        public QueryGlobalState(string key, string hash, bool isBlockHash, List<string> path = null) :
            base("query_global_state")
        {
            Dictionary<string, string> stateIdentifier = new Dictionary<string, string>
            {
                {isBlockHash ? "BlockHash" : "StateRootHash", hash}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"state_identifier", stateIdentifier},
                {"path", path ?? new List<string>()},
                {"key", key}
            };
        }

        public QueryGlobalState(GlobalStateKey key, string hash, bool isBlockHash, List<string> path = null) :
            this(key.ToString(), hash, isBlockHash, path)
        {
        }
    }

    public class GetBalanceMethod : RpcMethod
    {
        public GetBalanceMethod(string purse_uref, string state_root_hash) : base("state_get_balance")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"state_root_hash", state_root_hash},
                {"purse_uref", purse_uref}
            };
        }
    }

    public class PutDeployMethod : RpcMethod
    {
        public PutDeployMethod(Deploy deploy) : base("account_put_deploy")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"deploy", deploy}
            };
        }
    }

    public class GetDeployMethod : RpcMethod
    {
        public GetDeployMethod(string deployHash) : base("info_get_deploy")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"deploy_hash", deployHash}
            };
        }
    }

    public class GetBlockMethod : RpcMethod
    {
        public GetBlockMethod(string blockHash) : base("chain_get_block")
        {
            var blockIdentifier = new Dictionary<string, string>
            {
                {"Hash", blockHash}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockHash != null ? blockIdentifier : null}
            };
        }

        public GetBlockMethod(int height) : base("chain_get_block")
        {
            var blockIdentifier = new Dictionary<string, int>
            {
                {"Height", height}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockIdentifier}
            };
        }
    }

    public class GetBlockTransfersMethod : RpcMethod
    {
        public GetBlockTransfersMethod(string blockHash) : base("chain_get_block_transfers")
        {
            var blockIdentifier = new Dictionary<string, string>
            {
                {"Hash", blockHash}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockHash != null ? blockIdentifier : null}
            };
        }

        public GetBlockTransfersMethod(int height) : base("chain_get_block_transfers")
        {
            var blockIdentifier = new Dictionary<string, int>
            {
                {"Height", height}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockIdentifier}
            };
        }
    }

    public class GetEraInfoBySwitchBlockMethod : RpcMethod
    {
        public GetEraInfoBySwitchBlockMethod(string blockHash) : base("chain_get_era_info_by_switch_block")
        {
            var blockIdentifier = new Dictionary<string, string>
            {
                {"Hash", blockHash}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockHash != null ? blockIdentifier : null}
            };
        }

        public GetEraInfoBySwitchBlockMethod(int height) : base("chain_get_era_info_by_switch_block")
        {
            var blockIdentifier = new Dictionary<string, int>
            {
                {"Height", height}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {"block_identifier", blockIdentifier}
            };
        }
    }

    public class GetDictionaryItem : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via its seed URef.
        /// </summary>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        public GetDictionaryItem(string dictionaryItem, string state_root_hash) : base("state_get_dictionary_item")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"dictionary_identifier", new Dictionary<string, object>
                {
                    {"Dictionary", dictionaryItem}
                }},
                {"state_root_hash", state_root_hash},
            };
        }
    }
    
    public class GetDictionaryItemByAccount : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via an Account's named keys.
        /// </summary>
        /// <param name="accountKey">The account key as a formatted string whose named keys contains dictionary_name.</param>
        /// <param name="dictionaryName">The named key under which the dictionary seed URef is stored.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        public GetDictionaryItemByAccount(string accountKey, string dictionaryName, string dictionaryItem,
            string state_root_hash) : base("state_get_dictionary_item")
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
                {"state_root_hash", state_root_hash},
            };
        }
    }

    public class GetDictionaryItemByContract : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via a Contract named keys.
        /// </summary>
        /// <param name="contractKey">The contract key as a formatted string whose named keys contains dictionary_name.</param>
        /// <param name="dictionaryName">The named key under which the dictionary seed URef is stored.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        public GetDictionaryItemByContract(string contractKey, string dictionaryName, string dictionaryItem, string state_root_hash) : base("state_get_dictionary_item")
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
                {"state_root_hash", state_root_hash},
            };
        }
    }

    public class GetDictionaryItemByURef : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via its seed URef.
        /// </summary>
        /// <param name="seed_uref">The dictionary's seed URef.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        public GetDictionaryItemByURef(string seedURef, string dictionaryItem, string state_root_hash) : base("state_get_dictionary_item")
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
                {"state_root_hash", state_root_hash},
            };
        }
    }

    public class GetValidatorChanges : RpcMethod
    {
        public GetValidatorChanges() : base("info_get_validator_changes")
        {
        }
    }

    public class GetRpcSchema : RpcMethod
    {
        public GetRpcSchema() : base("rpc.discover")
        {
        }
    }
}