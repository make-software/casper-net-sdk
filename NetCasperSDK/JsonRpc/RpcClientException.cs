using System;

namespace NetCasperSDK.JsonRpc
{
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