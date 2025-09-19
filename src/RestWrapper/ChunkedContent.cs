namespace RestWrapper
{
    using System.IO;
    using System.Net.Http;
    using System.Net;
    using System.Threading.Tasks;
    using System;
    using System.Threading;
    using System.Text;

    /// <summary>
    /// Chunked content.
    /// </summary>
    public class ChunkedContent : HttpContent
    {
        private static readonly byte[] _Crlf = new byte[] { 13, 10 }; // \r\n
        private readonly Stream _Stream = null;
        private readonly TaskCompletionSource<bool> _StreamCompletion;

        /// <summary>
        /// Chunked content.
        /// </summary>
        public ChunkedContent()
        {
            _Stream = new MemoryStream();
            _StreamCompletion = new TaskCompletionSource<bool>();
        }

        /// <summary>
        /// Try compute length.
        /// </summary>
        /// <param name="length">Length.</param>
        /// <returns>True if successful.</returns>
        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _Stream.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Send a chunk.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task SendChunk(string data, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(data)) data = "";
            await SendChunk(Encoding.UTF8.GetBytes(data), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a chunk.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task SendChunk(byte[] data, CancellationToken token = default)
        {
            if (data == null) data = Array.Empty<byte>();

            // Write chunk size in hex
            string chunkSizeHex = data.Length.ToString("X") + "\r\n";
            byte[] chunkSizeBytes = Encoding.ASCII.GetBytes(chunkSizeHex);
            await _Stream.WriteAsync(chunkSizeBytes, 0, chunkSizeBytes.Length, token).ConfigureAwait(false);

            // Write chunk data
            if (data.Length > 0)
            {
                await _Stream.WriteAsync(data, 0, data.Length, token).ConfigureAwait(false);
            }

            // Write CRLF after chunk
            await _Stream.WriteAsync(_Crlf, 0, _Crlf.Length, token).ConfigureAwait(false);
            await _Stream.FlushAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send an empty chunk.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task SendEmptyChunk(CancellationToken token = default)
        {
            // Write final chunk (0\r\n\r\n)
            byte[] finalChunk = Encoding.ASCII.GetBytes("0\r\n\r\n");
            await _Stream.WriteAsync(finalChunk, 0, finalChunk.Length, token).ConfigureAwait(false);
            await _Stream.FlushAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Serialize to stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="context">Transport context.</param>
        /// <returns>Task.</returns>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            _Stream.Position = 0;
            await _Stream.CopyToAsync(stream);
            _StreamCompletion.SetResult(true);
        }
    }
}
