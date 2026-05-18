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
        private readonly Stream _Stream;
        private readonly byte[] _ReadBuffer = new byte[1];
        private bool _disposedValue;

        /// <summary>
        /// Server-sent events reader.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ServerSentEventReader(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            _Stream = stream;
        }

        /// <summary>
        /// Read next event.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Server-sent event.</returns>
        public async Task<ServerSentEvent> ReadNextEventAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            ServerSentEvent eventData = new ServerSentEvent();
            StringBuilder dataBuilder = new StringBuilder();
            string line = null;

            while ((line = await ReadLineAsync(token).ConfigureAwait(false)) != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (dataBuilder.Length > 0 || eventData.Event != null)
                    {
                        eventData.Data = dataBuilder.ToString().TrimEnd('\r', '\n');
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
                        dataBuilder.Append(value).Append('\n');
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
                eventData.Data = dataBuilder.ToString().TrimEnd('\r', '\n');
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
                    _Stream?.Dispose();
                }
                _disposedValue = true;
            }
        }

        private async Task<string> ReadLineAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using (MemoryStream lineBuffer = new MemoryStream())
            {
                while (true)
                {
                    int read = await _Stream.ReadAsync(_ReadBuffer, 0, 1, token).ConfigureAwait(false);
                    if (read == 0)
                    {
                        if (lineBuffer.Length == 0) return null;
                        return Encoding.UTF8.GetString(lineBuffer.ToArray());
                    }

                    byte current = _ReadBuffer[0];

                    if (current == '\n')
                    {
                        return Encoding.UTF8.GetString(lineBuffer.ToArray());
                    }

                    if (current != '\r')
                    {
                        lineBuffer.WriteByte(current);
                    }
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ServerSentEventReader));
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
