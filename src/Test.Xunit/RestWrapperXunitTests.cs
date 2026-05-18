namespace Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;

    using global::Xunit;

    using Test.Shared;

    using Touchstone.Core;
    using Touchstone.XunitAdapter;

    /// <summary>
    /// xUnit host for the shared Touchstone RestWrapper descriptors.
    /// </summary>
    public class RestWrapperXunitTests
    {
        /// <summary>
        /// Provides one xUnit theory row per shared test case descriptor.
        /// </summary>
        /// <returns>Theory data.</returns>
        public static TheoryData<TestCaseDescriptor> TestCases()
        {
            return new TouchstoneTheoryData(RestWrapperSuites.All);
        }

        /// <summary>
        /// Execute a single shared test case descriptor.
        /// </summary>
        /// <param name="testCase">Test case descriptor.</param>
        /// <returns>Completion task.</returns>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
