namespace RestWrapper
{
    /// <summary>
    /// Server-sent event.
    /// </summary>
    public class ServerSentEvent
    {
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

        /// <summary>
        /// Server-sent event.
        /// </summary>
        public ServerSentEvent()
        {

        }
    }
}