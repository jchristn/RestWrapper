namespace RestWrapper
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// Server-sent event.
    /// </summary>
    public class ServerSentEvent
    {
        #region Public-Members

        /// <summary>
        /// ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Event.
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// Data.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Retry.
        /// </summary>
        public int? Retry { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ServerSentEvent()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}