namespace RestWrapper
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// Chunk data.
    /// </summary>
    public class ChunkData
    {
        #region Public-Members

        /// <summary>
        /// Chunk data.
        /// </summary>
        public byte[] Data { get; set; } = null;

        /// <summary>
        /// Boolean indicating if the chunk is the final chunk.
        /// </summary>
        public bool IsFinal { get; set; } = false;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ChunkData()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}