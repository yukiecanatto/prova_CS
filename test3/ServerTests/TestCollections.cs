using Xunit;

namespace ServerTests
{
    [CollectionDefinition("Unit Tests", DisableParallelization = true)]
    public class UnitTestCollection { }

    [CollectionDefinition("Functional Tests", DisableParallelization = true)]
    public class FunctionalTestCollection { }
}
