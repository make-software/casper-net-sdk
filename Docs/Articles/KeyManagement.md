# Key management with Casper .Net SDK

Use the `PublicKey` and `KeyPair` classes to work with public keys and private keys respectively.

### Creating new key pairs

To create a new key pair use the method `CreateNew()` indicating the algorithm used:

```csharp
var newKeyPair = KeyPair.CreateNew(KeyAlgo.ED25519);
```

```csharp
var anotherKeyPair = KeyPair.CreateNew(KeyAlgo.SECP256K1);
```

### Reading keys from PEM files

To read a key from a PEM file use the method `FromPEM()`:

```csharp
var pk = PublicKey.FromPem("/tmp/my_pk.pem");
```

```csharp
var keypair = KeyPair.FromPem("/tmp/faucetact.pem");
```

When working with a private key you can always get its public key:

```csharp
var faucetPK = keypair.PublicKey;
```

### Writing keys to PEM files

When you create a new key you may want to save the private part in a file for later usage. Currently, we support writing the keys to PEM files:

```csharp
newKeyPair.WriteToPem("newED25519_sk.pem");
```

```csharp
anotherKeyPair.WriteToPem("newSECP256K1_sk.pem");
```

You can save to a file also the public key part:

```csharp
newKeyPair.WritePublicKeyToPem("newED25519_pk.pem");
```

```csharp
newKeyPair.PublicKey.WriteToPem("newED25519_pk2.pem");
```

### Creating public key from values

To get a `PublicKey` object from a hexadecimal string, use the `FromHexString()` method if the string contains the algorithm identifier prefix as first byte:

```csharp
var pk = PublicKey.FromHexString("012629e6d0eed7db2d232b5b0a35d729796bb6f3cbd12811538a61de78c75870ba");
```

Or use the method `FromRawBytes()` and indicate the algorithm of the key explicitly:

```csharp
var pk2 = PublicKey.FromRawBytes("2629e6d0eed7db2d232b5b0a35d729796bb6f3cbd12811538a61de78c75870ba", KeyAlgo.ED25519);
```

```csharp
var pk3 = PublicKey.FromRawBytes("02793d6a3940502b0946bed65719d3e75d089a25b52a8fc740373c48a8031e83b3", KeyAlgo.SECP256K1);
```
