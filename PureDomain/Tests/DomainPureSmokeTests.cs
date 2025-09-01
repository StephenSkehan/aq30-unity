using Xunit;

namespace AQ30.PureDomain.Tests
{
    public class PureSmoke
    {
        [Fact]
        public void DomainIsBuildable()
        {
            Assert.Equal("DomainAlive", AQ30.Domain.Placeholder.Value);
        }
    }
}