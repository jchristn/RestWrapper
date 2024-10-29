namespace RestWrapper
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Server-sent events reader.
    /// </summary>
    public class ServerSentEventReader : IDisposable
    {
        #region Public-Methods

        #endregion

        #region Private-Methods

        private readonly Stream _Stream;
        private readonly StreamReader _Reader;
        private bool _disposedValue;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ServerSentEventReader(Stream stream)
        {
            _Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _Reader = new StreamReader(stream, Encoding.UTF8);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Read next event.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Server-sent event.</returns>
        public async Task<ServerSentEvent> ReadNextEventAsync(CancellationToken token = default)
        {
            ServerSentEvent eventData = new ServerSentEvent();
            StringBuilder dataBuilder = new StringBuilder();
            string line;

            while (!token.IsCancellationRequested &&
                   (line = await _Reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (dataBuilder.Length > 0 || eventData.Event != null)
                    {
                        eventData.Data = dataBuilder.ToString().TrimEnd('\n');
                        return eventData;
                    }
                    continue;
                }

                if (line.StartsWith(":"))
                    continue;

                int colonIndex = line.IndexOf(":");
                if (colonIndex == -1)
                    continue;

                string field = line.Substring(0, colonIndex);
                string value = colonIndex < line.Length - 1 ?
                    line.Substring(colonIndex + 1).TrimStart() :
                    string.Empty;

                switch (field)
                {
                    case "event":
                        eventData.Event = value;
                        break;

                    case "data":
                        dataBuilder.AppendLine(value);
                        break;

                    case "id":
                        eventData.Id = value;
                        break;

                    case "retry":
                        if (int.TryParse(value, out int retry))
                            eventData.Retry = retry;
                        break;
                }
            }

            if (dataBuilder.Length > 0 || eventData.Event != null)
            {
                eventData.Data = dataBuilder.ToString().TrimEnd('\n');
                return eventData;
            }

            return null;
        }

        /// <summary>
        /// Read next event.
        /// </summary>
        /// <returns>Server-sent event.</returns>
        public ServerSentEvent ReadNextEvent()
        {
            return ReadNextEventAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _Reader?.Dispose();
                }
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}