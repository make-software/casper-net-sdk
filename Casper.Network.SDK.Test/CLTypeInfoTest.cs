using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    public class CLTypeInfoTest
    {
        [Test]
        public void CLTypeInfoToStringTest()
        {
            CLTypeInfo cltype = CLType.Bool; //implicit cast
            Assert.AreEqual("Bool", cltype.ToString());
            
            cltype = CLType.I32;
            Assert.AreEqual("I32", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.I64);
            Assert.AreEqual("I64", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.U8);
            Assert.AreEqual("U8", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.U32);
            Assert.AreEqual("U32", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.U64);
            Assert.AreEqual("U64", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.U128);
            Assert.AreEqual("U128", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.U256);
            Assert.AreEqual("U256", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.U512);
            Assert.AreEqual("U512", cltype.ToString());

            cltype = new CLTypeInfo(CLType.Unit);
            Assert.AreEqual("Unit", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.String);
            Assert.AreEqual("String", cltype.ToString());

            cltype = new CLTypeInfo(CLType.URef);
            Assert.AreEqual("URef", cltype.ToString());

            cltype = new CLTypeInfo(CLType.Any);
            Assert.AreEqual("Any", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.PublicKey);
            Assert.AreEqual("PublicKey", cltype.ToString());

            cltype = CLType.PublicKey; // implicit cast
            Assert.AreEqual("PublicKey", cltype.ToString());
            
            cltype = new CLKeyTypeInfo(KeyIdentifier.Account);
            Assert.AreEqual("Key(Account)", cltype.ToString());

            cltype = new CLKeyTypeInfo(KeyIdentifier.EraInfo);
            Assert.AreEqual("Key(EraInfo)", cltype.ToString());
            
            cltype = new CLOptionTypeInfo(new CLTypeInfo(CLType.U512));
            Assert.AreEqual("Option(U512)", cltype.ToString());
            
            cltype = new CLOptionTypeInfo(new CLKeyTypeInfo(KeyIdentifier.Account));
            Assert.AreEqual("Option(Key(Account))", cltype.ToString());
            
            cltype = new CLListTypeInfo(new CLKeyTypeInfo(KeyIdentifier.Account));
            Assert.AreEqual("List(Key(Account))", cltype.ToString());
            
            cltype = new CLMapTypeInfo(new CLTypeInfo(CLType.String),
                new CLKeyTypeInfo(KeyIdentifier.Account));
            Assert.AreEqual("Map(String,Key(Account))", cltype.ToString());

            cltype = new CLTuple2TypeInfo(new CLTuple1TypeInfo(new CLTypeInfo(CLType.Any)), 
                            new CLTypeInfo(CLType.Bool));
            Assert.AreEqual("Tuple2(Tuple1(Any),Bool)", cltype.ToString());
            
            cltype = new CLResultTypeInfo(new CLMapTypeInfo(CLType.String,
                new CLKeyTypeInfo(KeyIdentifier.Account)),
                new CLTuple3TypeInfo(CLType.Bool, CLType.I32, CLType.U512));
            Assert.AreEqual("Result(Map(String,Key(Account)),Tuple3(Bool,I32,U512))", cltype.ToString());
        }

        [Test]
        public void CLTypeInfoInvalidCastTest()
        {
            CLTypeInfo cltype = null;
            try
            {
                cltype = CLType.Key;
                Assert.Fail("Exception expected for an invalid cast");
            }
            catch 
            {
                Assert.IsNull(cltype);
            }
        }
        
        [Test]
        public void CLTypeInfoEqualityTest()
        {
            CLTypeInfo cl1 = CLType.Bool; //implicit cast
            var cl2 = new CLTypeInfo(CLType.Bool);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);

            cl1 = CLType.String;
            cl2 = CLType.String;
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);

            cl1 = CLType.Bool;
            cl2 = CLType.I32;
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }

        [Test]
        public void CLKeyTypeInfoEqualityTest()
        {
            var cl1 = new CLKeyTypeInfo(KeyIdentifier.Account);
            var cl2 = new CLKeyTypeInfo(KeyIdentifier.Account);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLKeyTypeInfo(KeyIdentifier.Account);
            cl2 = new CLKeyTypeInfo(KeyIdentifier.EraInfo);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }

        [Test]
        public void CLOptionTypeInfoEqualityTest()
        {
            var cl1 = new CLOptionTypeInfo(CLType.String);
            var cl2 = new CLOptionTypeInfo(new CLTypeInfo(CLType.String));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLOptionTypeInfo(CLType.String);
            cl2 = new CLOptionTypeInfo(CLType.I32);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLOptionTypeInfo(new CLKeyTypeInfo(KeyIdentifier.Account));
            cl2 = new CLOptionTypeInfo(new CLKeyTypeInfo(KeyIdentifier.Account));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLOptionTypeInfo(new CLKeyTypeInfo(KeyIdentifier.Account));
            cl2 = new CLOptionTypeInfo(new CLKeyTypeInfo(KeyIdentifier.Hash));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }

        [Test]
        public void CLListTypeInfoEqualityTest()
        {
            var cl1 = new CLListTypeInfo(CLType.String);
            var cl2 = new CLListTypeInfo(CLType.String);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLListTypeInfo(CLType.String);
            cl2 = new CLListTypeInfo(CLType.U128);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLListTypeInfo(CLType.String);
            cl2 = new CLListTypeInfo(new CLOptionTypeInfo(CLType.String));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLListTypeInfo(new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLListTypeInfo(new CLOptionTypeInfo(CLType.String));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLListTypeInfo(new CLKeyTypeInfo(KeyIdentifier.Account));
            cl2 = new CLListTypeInfo(new CLOptionTypeInfo(CLType.String));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }

        [Test]
        public void CLByteArrayTypeInfoEqualityTest()
        {
            var cl1 = new CLByteArrayTypeInfo(32);
            var cl2 = new CLByteArrayTypeInfo(32);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLByteArrayTypeInfo(32);
            cl2 = new CLByteArrayTypeInfo(64);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            
            cl1 = new CLByteArrayTypeInfo(0);
            cl2 = new CLByteArrayTypeInfo(0);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLByteArrayTypeInfo(0);
            cl2 = new CLByteArrayTypeInfo(64);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }

        [Test]
        public void CLResultTypeInfoEqualityTest()
        {
            var cl1 = new CLResultTypeInfo(CLType.String, CLType.String);
            var cl2 = new CLResultTypeInfo(CLType.String, CLType.String);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLResultTypeInfo(CLType.String, CLType.Bool);
            cl2 = new CLResultTypeInfo(CLType.String, new CLTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLResultTypeInfo(CLType.String, CLType.String);
            cl2 = new CLResultTypeInfo(CLType.Bool, new CLTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLResultTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLResultTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLResultTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLResultTypeInfo(new CLOptionTypeInfo(CLType.Bool), new CLOptionTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLResultTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLResultTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.I32));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }

        [Test]
        public void CLMapTypeInfoEqualityTest()
        {
            var cl1 = new CLMapTypeInfo(CLType.String, CLType.String);
            var cl2 = new CLMapTypeInfo(CLType.String, CLType.String);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLMapTypeInfo(CLType.String, CLType.Bool);
            cl2 = new CLMapTypeInfo(CLType.String, new CLTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLMapTypeInfo(CLType.String, CLType.String);
            cl2 = new CLMapTypeInfo(CLType.Bool, new CLTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLMapTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLMapTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLMapTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLMapTypeInfo(new CLOptionTypeInfo(CLType.Bool), new CLOptionTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLMapTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLMapTypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.I32));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }

        [Test]
        public void CLTuple1TypeInfoEqualityTest()
        {
            var cl1 = new CLTuple1TypeInfo(CLType.String);
            var cl2 = new CLTuple1TypeInfo(new CLTypeInfo(CLType.String));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLTuple1TypeInfo(CLType.String);
            cl2 = new CLTuple1TypeInfo(new CLTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLTuple1TypeInfo(new CLOptionTypeInfo(CLType.String));
            cl2 = new CLTuple1TypeInfo(new CLOptionTypeInfo(CLType.String));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLTuple1TypeInfo(new CLOptionTypeInfo(CLType.String));
            cl2 = new CLTuple1TypeInfo(new CLOptionTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLTuple1TypeInfo(CLType.Bool);
            cl2 = new CLTuple1TypeInfo(new CLOptionTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }

        [Test]
        public void CLTuple2TypeInfoEqualityTest()
        {
            var cl1 = new CLTuple2TypeInfo(CLType.String, CLType.String);
            var cl2 = new CLTuple2TypeInfo(CLType.String, CLType.String);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLTuple2TypeInfo(CLType.String, CLType.Bool);
            cl2 = new CLTuple2TypeInfo(CLType.String, new CLTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLTuple2TypeInfo(CLType.String, CLType.String);
            cl2 = new CLTuple2TypeInfo(CLType.Bool, new CLTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLTuple2TypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLTuple2TypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLTuple2TypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLTuple2TypeInfo(new CLOptionTypeInfo(CLType.Bool), new CLOptionTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLTuple2TypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLTuple2TypeInfo(new CLOptionTypeInfo(CLType.String), new CLOptionTypeInfo(CLType.I32));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }
        
        [Test]
        public void CLTuple3TypeInfoEqualityTest()
        {
            var cl1 = new CLTuple3TypeInfo(CLType.String, CLType.String, CLType.String);
            var cl2 = new CLTuple3TypeInfo(CLType.String, CLType.String, CLType.String);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLTuple3TypeInfo(CLType.String, CLType.I32, CLType.Bool);
            cl2 = new CLTuple3TypeInfo(CLType.String, CLType.I32, new CLTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLTuple3TypeInfo(CLType.String, CLType.String, CLType.String);
            cl2 = new CLTuple3TypeInfo(CLType.Bool, CLType.Bool, new CLTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLTuple3TypeInfo(new CLOptionTypeInfo(CLType.String), CLType.Bool, new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLTuple3TypeInfo(new CLOptionTypeInfo(CLType.String), CLType.Bool, new CLOptionTypeInfo(CLType.Bool));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreEqual(cl1, cl2);
            
            cl1 = new CLTuple3TypeInfo(new CLOptionTypeInfo(CLType.String), CLType.Bool, CLType.I32);
            cl2 = new CLTuple3TypeInfo(new CLOptionTypeInfo(CLType.Bool), CLType.Bool, CLType.I32);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLTuple3TypeInfo(CLType.String, new CLOptionTypeInfo(CLType.Bool), CLType.String);
            cl2 = new CLTuple3TypeInfo(CLType.String, new CLOptionTypeInfo(CLType.I32), CLType.String);
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
            
            cl1 = new CLTuple3TypeInfo(CLType.String, CLType.String, new CLOptionTypeInfo(CLType.Bool));
            cl2 = new CLTuple3TypeInfo(CLType.String, CLType.String, new CLOptionTypeInfo(CLType.I32));
            Assert.IsNotNull(cl1);
            Assert.IsNotNull(cl2);
            Assert.AreNotEqual(cl1, cl2);
        }
    }
}
