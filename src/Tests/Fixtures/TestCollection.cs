using Xunit;

namespace Tests.Fixtures
{
    [CollectionDefinition(nameof(Tests))]
    public class TestCollection : ICollectionFixture<TestManagerFixture>, ICollectionFixture<MoqFixture> { }
}