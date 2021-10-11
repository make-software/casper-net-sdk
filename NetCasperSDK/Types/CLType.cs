namespace NetCasperSDK.Types
{
    public enum CLType : byte
    {
        /** boolean primitive */
        Bool =  0,
        /** signed 32-bit integer primitive */
        I32 =  1,
        /** signed 64-bit integer primitive */
        I64 =  2,
        /** unsigned 8-bit integer primitive */
        U8 =  3,
        /** unsigned 32-bit integer primitive */
        U32 =  4,
        /** unsigned 64-bit integer primitive */
        U64 =  5,
        /** unsigned 128-bit integer primitive */
        U128 =  6,
        /** unsigned 256-bit integer primitive */
        U256 =  7,
        /** unsigned 512-bit integer primitive */
        U512 =  8,
        /** singleton value without additional semantics */
        Unit = 9,
        /** e.g. "Hello, World!" */
        String = 10,
        /** global state key */
        Key = 11,
        /** unforgeable reference */
        URef = 12,
        /** optional value of the given type Option(CLType) */
        Option = 13,
        /** List of values of the given type (e.g. Vec in rust). List(CLType) */
        List = 14,
        /** Byte array prefixed with U32 length (FixedList) */
        ByteArray = 15,
        /** co-product of the the given types; one variant meaning success, the other failure */
        Result = 16,
        /** Map(CLType, CLType), // key-value association where keys and values have the given types */
        Map = 17,
        /** Tuple1(CLType) single value of the given type */
        Tuple1 = 18,
        /** Tuple2(CLType, CLType), // pair consisting of elements of the given types */
        Tuple2 = 19,
        /** Tuple3(CLType, CLType, CLType), // triple consisting of elements of the given types */
        Tuple3 = 20,
        /** Indicates the type is not known */
        Any = 21,
        /** Public key */
        PublicKey = 22
    }
}