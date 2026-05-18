namespace Test.Shared
{
    /// <summary>
    /// Simple payload used for request and response serialization tests.
    /// </summary>
    public class SamplePayload
    {
        /// <summary>
        /// Sample name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Sample numeric value.
        /// </summary>
        public int Value { get; set; }
    }
}
