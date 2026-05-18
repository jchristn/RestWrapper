namespace Test.Shared
{
    /// <summary>
    /// Snapshot of the inbound request as observed by the local test server.
    /// </summary>
    public class RequestSnapshot
    {
        /// <summary>
        /// HTTP method.
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Request path.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Request body text.
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Request content type.
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Transfer-Encoding header value.
        /// </summary>
        public string TransferEncoding { get; set; } = string.Empty;

        /// <summary>
        /// Content-Language header value.
        /// </summary>
        public string ContentLanguage { get; set; } = string.Empty;

        /// <summary>
        /// Accept header value.
        /// </summary>
        public string Accept { get; set; } = string.Empty;

        /// <summary>
        /// Host header value.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Raw authorization header.
        /// </summary>
        public string Authorization { get; set; } = string.Empty;

        /// <summary>
        /// Parsed basic-auth username.
        /// </summary>
        public string AuthorizationUsername { get; set; } = string.Empty;

        /// <summary>
        /// Parsed basic-auth password.
        /// </summary>
        public string AuthorizationPassword { get; set; } = string.Empty;

        /// <summary>
        /// Parsed bearer token.
        /// </summary>
        public string AuthorizationBearerToken { get; set; } = string.Empty;

        /// <summary>
        /// First custom test header.
        /// </summary>
        public string CustomHeader { get; set; } = string.Empty;

        /// <summary>
        /// Second custom test header.
        /// </summary>
        public string AnotherHeader { get; set; } = string.Empty;

        /// <summary>
        /// User-agent value observed by the server.
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;
    }
}
