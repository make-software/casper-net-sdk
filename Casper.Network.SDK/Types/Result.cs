namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Equivalent to the `Result` type in Rust. It can wrap a value with type `TOk` when the
    /// returning method returns successfully. Or an error with type `TErr` when the method
    /// needs to return an error. 
    /// </summary>
    /// <typeparam name="TOk">Type of the object returned with Ok result.</typeparam>
    /// <typeparam name="TErr">Type of the object returned with Error result.</typeparam>
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