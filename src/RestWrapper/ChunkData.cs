namespace RestWrapper
{
    /// <summary>
    /// Chunk data.
    /// </summary>
    public class ChunkData
    {
        /// <summary>
        /// Chunk data.
        /// </summary>
        public byte[] Data { get; set; } = null;

        /// <summary>
        /// Boolean indicating if the chunk is the final chunk.
        /// </summary>
        public bool IsFinal { get; set; } = false;

        /// <summary>
        /// Chunk data.
        /// </summary>
        public ChunkData()
        {

        }
    }
}