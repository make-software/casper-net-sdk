using System.Collections.Generic;
using NetCasperSDK.JsonRpc;
using NetCasperSDK.Types;

namespace NetCasperSDK
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

    public class GetRpcSchema : RpcMethod
    {
        public GetRpcSchema() : base("rpc.discover")
        {
        }
    }
}