# Working with `CLValue`

`CLValue` is the type used in deploy input arguments. And it can also be returned as a result of a query to the network or a contract call.

A `CLValue` comprises three parts:

* `cl_type`. A `CLType` that defines the type of the value stored.
* `bytes`. The value encoded as a byte array.
* `parsed`. A human-readable representation of the value. This part is optional and may not be present in some cases.

As an example, the most simple CLValue representing a boolean `true` value is like this:

```json
{
    "cl_type":"Bool",
    "bytes":"01",
    "parsed":true
}
```

_NOTE_: Check the KVStorage contract tutorial in this repository to see some examples of converting from and to `CLValue`.

## `CLType`

The list of `CLType`s is in the table below. Some of the types have equivalent native types in `C#`. In these cases we can convert easily between both as described later in this document.

| Casper type | C# type | Description |
|-------------|---------|-------------|
| `Bool` | `bool` | Boolean primitive. |
| `I32` | `int` | Signed 32-bit integer primitive. |
| `I64` | `long` | Signed 64-bit integer primitive. |
| `U8` | `byte` | Unsigned 8-bit integer primitive. |
| `U32` | `uint` | Unsigned 32-bit integer primitive. |
| `U64` | `ulong` | Unsigned 64-bit integer primitive. |
| `U128` | `BigInteger` | Unsigned 128-bit integer primitive. |
| `U256` | `BigInteger` | Unsigned 256-bit integer primitive. |
| `U512` | `BigInteger` | Unsigned 512-bit integer primitive. |
| `Unit` | n/a | Singleton value without additional semantics. |
| `String` | `string` | A string. e.g. "Hello, World!". |
| `Key` | n/a | Global state key. |
| `URef` | n/a | Unforgeable reference. |
| `Option` | n/a | Optional value of the given type `Option(CLType)`. The value `None` is represented in C# as `null`. |
| `List` | `List<>` | Variable-length list of values of a single `CLType` `List(CLType)` |
| `ByteArray` | `byte[]` | Fixed-length list of bytes. |
| `Result` | n/a | Co-product of the the given types; one variant meaning success, the other failure. |
| `Map` | `Dictionary<,>` | Key-value association where keys and values have the given types `Map(CLType, CLType)` |
| `Tuple1` | n/a | Single value of the given type `Tuple1(CLType`). |
| `Tuple2` | n/a | Pair consisting of elements of the given types `Tuple2(CLType, CLType)` |
| `Tuple3` | n/a | Triple consisting of elements of the given types `Tuple3(CLType, CLType, CLType)` |
| `Any` | n/a | Indicates the type is not known. |
| `PublicKey` | n/a | A Public key. |

## Working with booleans

To create a `CLValue` value with a boolean type write:

```csharp
var clTrue = CLValue.Bool(true);
var clFalse = CLValue.Bool(false);
```

However, in many cases you won't need to explicitly create the `CLValue` object. For example, to create a `NamedArg`, e.g. the input argument for a contract call, you can use the C# type directly:

```csharp
var inputArg = new NamedArg("value", true);
```

To convert a `CLValue` with a boolean value to `bool`, use one of the following:

```csharp
var b1 = clValue.ToBoolean();
var b2 = (bool)clValue;
```

## Working with numbers

Similarly, numbers can be converted to `CLValue` objects explicitly:

```csharp
var myI32 = CLValue.I32(int.MinValue);
var myI64 = CLValue.I64(long.MaxValue);
var myU8 = CLValue.U8(0x7F);
var myU32 = CLValue.U32(0);
var myU64 = CLValue.U64(ulong.MaxValue);
```

But, as before, the C# native types can be used instead to create the `NamedArgs` objects used as input arguments. Thus, these two lines of code are identical:

```csharp
var i32Arg = new NamedArg("value", int.MinValue);
````

```csharp
var i32Arg = new NamedArg("value", myI32);
```

When you need to use the value from a `CLValue` object, call any of the available `.ToXXX()` methods or use an explicit cast:

```csharp
var num1 = myI32.ToInt32();
var num2 = myI64.ToInt64();
var num3 = myU8.ToByte();
var num4 = myU32.ToUInt32();
var num5 = myU64.ToUInt64();
```

```csharp
var num1 = (int)myI32;
var num2 = (long)myI64;
var num3 = (byte)myU8;
var num4 = (uint)myU32;
var num5 = (ulong)myU64;
```

For numbers with 128, 256 and 512 bits use the `BigInteger` class in the `System.Numerics` namespace. For instance, $CSPR amounts are indicated with a `U512` value containing the number of motes. So, to create a variable with a quantity of 3 $CSPR, you can write:

```csharp
var cspr3 = CLValue.U512(3_000_000_000);
```

And, in a `NamedArg` object, you would write:

```csharp
var payment = new NamedArg("payment", new BigInteger(3_000_000_000));
```

```csharp
var payment = new NamedArg("payment", cspr3);
```

## Working with strings

`string` objects can be converted to `CLValue` and viceversa. The same mechanism as with number applies:

```csharp
var clString = CLValue.String("Hello world!");
```

```csharp
var namedArgs = new List<NamedArg>()
    {
        new NamedArg("name", "Weekday"),
        new NamedArg("value", "Monday")
    };
```

```csharp
string value = clString.ToString();
```

```csharp
string value = (string)clValue;
```

## Working with byte arrays

Byte arrays can be encoded into `CLValue`s and converted back. The same mechanism  applies:

```csharp
var bytes = new byte[] {0x00, 0x01, 0x02, 0x03};
var clValue = CLValue.ByteArray(bytes);

var arg1 = new NamedArg("value", clValue);
var arg2 = new NamedArg("value", bytes);

var bytes1 = clValue.ToByteArray();
var bytes2 = (byte[]) clValue;
```

## Working with `Option`

The type `Option(CLType)` is either a `None` value or a value with type `CLType`. Some examples:

```csharp
var option1 = CLValue.Option(CLValue.String("Hello World!"));
// but also:
var option2 = CLValue.Option("Hello World!");
var option3 = CLValue.Option(int.MinValue);
var option4 = CLValue.Option(publicKey);
```

To encode `None`, indicate the `CLType` that represents:

```csharp
var option4 = CLValue.OptionNone(CLType.String);
```

To extract the value of an `Option(CLType)` use the `.ToXXX()` that corresponds to the correct `CLType` (or the explicit cast):

```csharp
var value1 = option1.ToString();
var value3 = (int) option3;
PublicKey pk = option4.ToPublicKey();

string value5 = (string) option5; // value5 = null
```

## Working with `PublicKey`s and `URef`s

`PublicKey` and `URef` objects can be encoded into `CLValue`s and converted back. The same mechanism  applies:

```csharp
var publicKey = PublicKey.FromPem("myPublicKey.pem");
var pkClValue = CLValue.PublicKey(publicKey);

var arg1 = new NamedArg("value", pkClValue); 
var arg2 = new NamedArg("value", publicKey);

var pk1 = pkClValue.ToPublicKey();
var pk2 = (PublicKey) pkClValue;            
```

```csharp
var uref = new URef("uref-000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f-007");
var urefClValue = CLValue.URef(uref);

var arg3 = new NamedArg("value", urefClValue); 
var arg4 = new NamedArg("value", uref);

var uref2 = urefClValue.ToURef();
var uref3 = (URef) urefClValue;
```

## Working with `Key`s

`GlobalStateKey` objects can be encoded into `CLValue`s and converted back. The same mechanism  applies. For instance, with an `AccountHashKey`, we can write:

```csharp
var publicKey = PublicKey.FromPem("myPublicKey.pem");
var accountHash = new AccountHashKey(publicKey);

var accountClValue = CLValue.Key(accountHash);

var arg1 = new NamedArg("value", accountClValue);
var arg2 = new NamedArg("value", accountHash);
```

## Working with `List`s

Create a `CLValue` with type `List` with an array of values from the same type:

```csharp
var list = CLValue.List(new[]
    {CLValue.U8(0x10), CLValue.U8(0x20), CLValue.U8(0x30), CLValue.U8(0x40)});

var namedArgs = new List<NamedArg>()
{
    new NamedArg("name", "MyListOfBytes"),
    new NamedArg("value", list)
};
```

And, vice versa, convert a `CLValue` with type `List` to a C# List with `.ToList()`:

```csharp
var list1 = list.ToList();
````

## Working with `Map`s

Create a `CLValue` with type `Map` with a Dictionary of key-values:

```csharp
var dict = new Dictionary<CLValue, CLValue>()
    {
        {CLValue.String("fourteen"), CLValue.Option(CLValue.String("14"))},
        {CLValue.String("fifteen"), CLValue.Option(CLValue.String("15"))},
        {CLValue.String("sixteen"), CLValue.Option(CLValue.String("16"))},
        {CLValue.String("none"), CLValue.OptionNone(CLType.String)},
    };

var map = CLValue.Map(dict);

var namedArgs = new List<NamedArg>()
    {
        new NamedArg("name", "MyMap"),
        new NamedArg("value", map)
    };
```

And, vice versa, convert a `CLValue` with type `Map` to a C# Dictionary with `.ToDictionary()`:

```csharp
var dict1 = map.ToDictionary();
```
