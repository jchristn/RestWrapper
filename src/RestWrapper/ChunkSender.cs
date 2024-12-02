namespace RestWrapper
{
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System;
    using System.Threading;
    using System.Text;

    /// <summary>
    /// Chunked sender.
    /// </summary>
    public class ChunkedSender : IDisposable
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private readonly HttpClient _Client = null;
        private readonly HttpRequestMessage _Request = null;
        private readonly ChunkedContent _Content = null;
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="client">HTTP client.</param>
        /// <param name="request">HTTP request message.</param>
        public ChunkedSender(HttpClient client, HttpRequestMessage request)
        {
            _Client = client ?? throw new ArgumentNullException(nameof(client));
            _Request = request ?? throw new ArgumentNullException(nameof(request));

            _Request.Headers.TransferEncodingChunked = true;
            _Content = new ChunkedContent();
            _Request.Content = _Content;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Disposed.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _Content?.Dispose();
                }

                _Disposed = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Send a chunk.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="isFinal">Boolean indicating if the chunk is the final chunk.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>HTTP response message.</returns>
        public async Task<HttpResponseMessage> SendChunk(string data, bool isFinal = false, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(data)) data = "";
            return await SendChunk(Encoding.UTF8.GetBytes(data), isFinal, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a chunk.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="isFinal">Boolean indicating if the chunk is the final chunk.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>HTTP response message.</returns>
        public async Task<HttpResponseMessage> SendChunk(byte[] data, bool isFinal = false, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (data == null) data = Array.Empty<byte>();
            await _Content.SendChunk(data, token).ConfigureAwait(false);
            if (isFinal)
            {
                await _Content.SendEmptyChunk(token).ConfigureAwait(false);
                return await _Client.SendAsync(_Request);
            }
            return null;
        }

        #endregion

        #region Private-Methods

        private void ThrowIfDisposed()
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(ChunkedSender));
        }

        #endregion
    }
}