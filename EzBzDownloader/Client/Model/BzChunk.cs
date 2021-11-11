namespace EzBzDownloader.Client.Model
{
    public sealed class BzChunk
    {
        public const int ChunkSize = 40 * 1024 * 1024;

        public BzChunk(byte[] content, BzChunkMetadata metadata)
        {
            Content = content;
            Metadata = metadata;
        }

        public byte[] Content { get; }

        public BzChunkMetadata Metadata { get; }
    }

    public sealed class BzChunkMetadata
    {
        public BzChunkMetadata(byte[] sha1)
        {
            Sha1 = sha1;
        }

        public byte[] Sha1 { get; }
    }
}