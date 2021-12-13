using System;

namespace Casper.Network.SDK.JsonRpc
{
    /// <summary>
    /// Exception thrown when an RPC Response returns an error and not a result.
    /// The RPCError inside the exception contains the error data from the network.
    /// </summary>
    public class RpcClientException : Exception
    {
        public RpcError RpcError { get; }
        
        public RpcClientException(string message) : base(message)
        {
            RpcError = new RpcError()
            {
                Code = 0,
                Message = message
            };
        }

        public RpcClientException(string message, Exception innerException) : base(message, innerException)
        {
            RpcError = new RpcError()
            {
                Code = 0,
                Message = message
            };
        }

        public RpcClientException(string message, RpcError rpcError) : base(message)
        {
            RpcError = rpcError;
        }
    }
}