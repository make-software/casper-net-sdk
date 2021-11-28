namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Casper types, i.e. types which can be stored and manipulated by smart contracts.
    /// Provides a description of the underlying data type of a `CLValue`.
    /// </summary>
    public enum CLType : byte
    {
        /// <summary>
        /// Boolean primitive.
        /// </summary>
        Bool =  0,
        /// <summary>
        /// Signed 32-bit integer primitive.
        /// </summary>
        I32 =  1,
        /// <summary>
        /// Signed 64-bit integer primitive.
        /// </summary>
        I64 =  2,
        /// <summary>
        /// Unsigned 8-bit integer primitive.
        /// </summary>
        U8 =  3,
        /// <summary>
        /// Unsigned 32-bit integer primitive.
        /// </summary>
        U32 =  4,
        /// <summary>
        /// Unsigned 64-bit integer primitive.
        /// </summary>
        U64 =  5,
        /// <summary>
        /// Unsigned 128-bit integer primitive.
        /// </summary>
        U128 =  6,
        /// <summary>
        /// Unsigned 256-bit integer primitive.
        /// </summary>
        U256 =  7,
        /// <summary>
        /// Unsigned 512-bit integer primitive.
        /// </summary>
        U512 =  8,
        /// <summary>
        /// Singleton value without additional semantics.
        /// </summary>
        Unit = 9,
        /// <summary>
        /// A string. e.g. "Hello, World!".
        /// </summary>
        String = 10,
        /// <summary>
        /// Global state key.
        /// </summary>
        Key = 11,
        /// <summary>
        /// Unforgeable reference.
        /// </summary>
        URef = 12,
        /// <summary>
        /// Optional value of the given type Option(CLType).
        /// </summary>
        Option = 13,
        /// <summary>
        /// Variable-length list of values of a single `CLType` List(CLType).
        /// </summary>
        List = 14,
        /// <summary>
        /// Fixed-length list of a single `CLType` (normally a Byte).
        /// </summary>
        ByteArray = 15,
        /// <summary>
        /// Co-product of the the given types; one variant meaning success, the other failure.
        /// </summary>
        Result = 16,
        /// <summary>
        /// Key-value association where keys and values have the given types Map(CLType, CLType).
        /// </summary>
        Map = 17,
        /// <summary>
        /// Single value of the given type Tuple1(CLType).
        /// </summary>
        Tuple1 = 18,
        /// <summary>
        /// Pair consisting of elements of the given types Tuple2(CLType, CLType).
        /// </summary>
        Tuple2 = 19,
        /// <summary>
        /// Triple consisting of elements of the given types Tuple3(CLType, CLType, CLType).
        /// </summary>
        Tuple3 = 20,
        /// <summary>
        /// Indicates the type is not known.
        /// </summary>
        Any = 21,
        /// <summary>
        /// A Public key.
        /// </summary>
        PublicKey = 22
    }
}