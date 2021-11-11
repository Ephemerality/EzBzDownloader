using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EzBzDownloader.Client.Model;
using EzBzDownloader.Lib;

namespace EzBzDownloader.Client.Logic
{
    public static class BzChunkMetadataHandler
    {
        public static async Task<Dictionary<int, BzChunkMetadata>> LoadMetadataAsync(Stream metadata, CancellationToken cancellationToken)
        {
            var result = new Dictionary<int, BzChunkMetadata>();
            var metadataBuffer = new byte[metadata.Length];
            await metadata.ReadAsync(metadataBuffer.AsMemory(0, (int)metadata.Length), cancellationToken);
            for (var i = 0;; i++)
            {
                var position = 20 * i;
                if (position >= metadata.Length)
                    break;
                var existingSha = metadataBuffer.AsMemory(position, 20);
                // Skip chunks that haven't been downloaded yet
                if (metadataBuffer.All(b => b == 0))
                    continue;
                result.Add(i, new BzChunkMetadata(existingSha.ToArray()));
            }

            return result;
        }

        /// <summary>
        /// Loads and validates the data in <paramref name="file"/> based on <paramref name="chunkMetadata"/>.
        /// Invalid chunks will be removed from <paramref name="chunkMetadata"/>
        /// </summary>
        public static async Task ValidateChunksAsync(Stream file, Dictionary<int, BzChunkMetadata> chunkMetadata, CancellationToken cancellationToken)
        {
            var validateChunkBuffer = new byte[BzChunk.ChunkSize];
            var invalidChunks = new List<int>();
            foreach (var (index, chunk) in chunkMetadata)
            {
                var position = index * (long) BzChunk.ChunkSize;
                if (file.Position != position)
                    file.Seek(position, SeekOrigin.Begin);
                await file.ReadAsync(validateChunkBuffer.AsMemory(0, BzChunk.ChunkSize), cancellationToken);
                var hash = validateChunkBuffer.ComputeSha1();
                if (!hash.SequenceEqual(chunk.Sha1))
                    invalidChunks.Add(index);
            }

            foreach (var i in invalidChunks)
                chunkMetadata.Remove(i);
        }
    }
}