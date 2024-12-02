namespace RestWrapper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Chunked stream reader.
    /// </summary>
    public class ChunkedStreamReader
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private readonly Stream _Stream;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="stream">Stream.</param>
        public ChunkedStreamReader(Stream stream)
        {
            _Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Read the next chunk.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>String.</returns>
        public async Task<ChunkData> ReadNextChunkAsync(CancellationToken token = default)
        {
            #region Read-Header

            byte[] header = ReadUntilCrlf();
            if (header == null || header.Length < 1) return null;
            string headerStr = Encoding.UTF8.GetString(header).Split(';')[0];
            int size = 0;
            if (!int.TryParse(headerStr, System.Globalization.NumberStyles.HexNumber, null, out size))
                throw new FormatException("Invalid chunk size format: " + headerStr + ".");

            if (size == 0)
            {
                return new ChunkData
                {
                    Data = Array.Empty<byte>(),
                    IsFinal = true
                };
            }

            #endregion

            #region Read-Data

            byte[] data = await ReadBytes(size, token).ConfigureAwait(false);

            // and data following CRLF
            byte[] buffer = await ReadBytes(2, token).ConfigureAwait(false);
            if (!IsCrlf(buffer))
                throw new FormatException("Invalid data read while expecting newline delimiter.");

            #endregion

            return new ChunkData
            {
                Data = data,
                IsFinal = false
            };
        }

        #endregion

        #region Private-Methods

        private async Task<byte[]> ReadBytes(int count, CancellationToken token = default)
        {
            byte[] buffer = new byte[count];
            int read = await _Stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
            if (read != count) throw new InvalidOperationException("Could not read " + count + " bytes from the stream.");
            return buffer;
        }

        private bool IsCrlf(byte[] buffer)
        {
            return buffer != null &&
                   buffer.Length == 2 &&
                   buffer[0] == (byte)'\r' &&
                   buffer[1] == (byte)'\n';
        }

        private byte[] ReadUntilCrlf()
        {
            List<byte> buffer = new List<byte>();
            int lastByte = -1;

            while (true)
            {
                int currentByte = _Stream.ReadByte();
                if (currentByte == -1)
                {
                    if (buffer.Count == 0)
                        return Array.Empty<byte>();
                    break;
                }

                buffer.Add((byte)currentByte);

                if (lastByte == '\r' && currentByte == '\n')
                {
                    buffer.RemoveRange(buffer.Count - 2, 2);
                    break;
                }

                lastByte = currentByte;
            }

            byte[] ret = buffer.ToArray();
            return ret;
        }

        #endregion
    }
}