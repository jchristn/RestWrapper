namespace Test.Shared
{
    /// <summary>
    /// Indicates whether RestRequest should create its own HttpClient or use a caller-supplied instance.
    /// </summary>
    public enum RequestTransportMode
    {
        /// <summary>
        /// RestRequest creates and owns its internal HttpClient.
        /// </summary>
        Internal,
        /// <summary>
        /// RestRequest uses a caller-supplied HttpClient.
        /// </summary>
        External
    }
}
