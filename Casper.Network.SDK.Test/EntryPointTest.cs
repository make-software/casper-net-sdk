
using System.Text.Json;
using Casper.Network.SDK.Types;
using NUnit.Framework;

namespace Casper.Network.SDK.Test
{
    public class EntryPointTest
    {
        [Test]
        public void TemplateAccessTest()
        {
            var json = @"{
                          ""ret"": ""U256"",
                          ""args"": [],
                          ""name"": ""total_supply"",
                          ""access"": ""Template"",
                          ""entry_point_type"": ""Called""
                        }";
            var entryPoint = JsonSerializer.Deserialize<EntryPoint>(json);
            Assert.IsNotNull(entryPoint);
            Assert.IsTrue(entryPoint.Access.IsTemplate);
            Assert.IsFalse(entryPoint.Access.IsPublic);
            Assert.IsNull(entryPoint.Access.Groups);
        }
    }
}