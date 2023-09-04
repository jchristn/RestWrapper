using System;
using System.Collections.Generic;
using System.Text;

namespace RestWrapper
{
    /// <summary>
    /// Authorization header options.
    /// </summary>
    public class AuthorizationHeader
    {
        /// <summary>
        /// The username to use in the authorization header, if any.
        /// </summary>
        public string User { get; set; } = null;

        /// <summary>
        /// The password to use in the authorization header, if any.
        /// </summary>
        public string Password { get; set; } = null;

        /// <summary>
        /// The bearer token to use in the authorization header, if any.
        /// </summary>
        public string BearerToken { get; set; } = null;

        /// <summary>
        /// Enable to encode credentials in the authorization header.
        /// </summary>
        public bool EncodeCredentials { get; set; } = true;

        /// <summary>
        /// Raw authorization header value.
        /// </summary>
        public string Raw { get; set; } = null;

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public AuthorizationHeader()
        {

        }
    }
}
