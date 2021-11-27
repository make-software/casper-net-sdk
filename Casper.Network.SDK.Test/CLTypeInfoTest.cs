using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace NetCasperTest
{
    public class CLTypeInfoTest
    {
        [Test]
        public void CLTypeInfoToStringTest()
        {
            var cltype = new CLTypeInfo(CLType.Bool);
            Assert.AreEqual("Bool", cltype.ToString());
            
            cltype = new CLTypeInfo(CLType.I32);
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
            
            cltype = new CLResultTypeInfo(new CLMapTypeInfo(new CLTypeInfo(CLType.String),
                new CLKeyTypeInfo(KeyIdentifier.Account)),
                new CLTuple3TypeInfo(new CLTypeInfo(CLType.Bool), new CLTypeInfo(CLType.I32), new CLTypeInfo(CLType.U512)));
            Assert.AreEqual("Result(Map(String,Key(Account)),Tuple3(Bool,I32,U512))", cltype.ToString());
        }
    }
}