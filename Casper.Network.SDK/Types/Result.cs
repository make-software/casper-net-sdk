namespace Casper.Network.SDK.Types
{
    public class Result<TOk, TErr>
    {
        public bool Success { get; init; }
        public bool IsFailure => !Success;
        
        public TOk Value { get; init; }
        public TErr Error { get; init; }

        protected internal Result(TOk ok, TErr err, bool success)
        {
            Value = ok;
            Error = err;
            Success = success;
        }

        public static Result<TOk,TErr> Fail(TErr err) => 
            new Result<TOk,TErr>(default(TOk), err, false);

        public static Result<TOk, TErr> Ok(TOk value) =>
            new Result<TOk, TErr>(value, default(TErr), true);
    }
}