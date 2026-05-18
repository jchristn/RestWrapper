namespace Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Test.Shared;

    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// NUnit host for the shared Touchstone RestWrapper descriptors.
    /// </summary>
    [TestFixture]
    public class RestWrapperNunitTests
    {
        private static IEnumerable TestCases()
        {
            return new TouchstoneTestCaseSource(RestWrapperSuites.All);
        }

        /// <summary>
        /// Execute a single shared test case descriptor.
        /// </summary>
        /// <param name="testCase">Test case descriptor.</param>
        /// <returns>Completion task.</returns>
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
