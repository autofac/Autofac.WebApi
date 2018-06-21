using Autofac.Integration.WebApi.Test.TestTypes;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class ControllerTypeKeyFixture
    {
        [Fact]
        public void DerivedTypeDoesNotEqualBaseType()
        {
            var baseKey = new ControllerTypeKey(typeof(TestController));
            var derivedKey = new ControllerTypeKey(typeof(TestControllerA));

            Assert.NotEqual(baseKey, derivedKey);
        }

        [Fact]
        public void BaseTypeEqualsDerivedType()
        {
            var baseKey = new ControllerTypeKey(typeof(TestController));
            var derivedKey = new ControllerTypeKey(typeof(TestControllerA));

            Assert.Equal(derivedKey, baseKey);
        }
    }
}