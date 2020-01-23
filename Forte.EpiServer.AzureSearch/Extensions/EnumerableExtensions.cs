using System;
using System.Collections.Generic;

namespace Forte.EpiServer.AzureSearch.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> enumerable, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Must be greater than 0");
            }

            var nextbatch = new List<T>(chunkSize);
            foreach (var item in enumerable)
            {
                nextbatch.Add(item);
                if (nextbatch.Count == chunkSize)
                {
                    yield return nextbatch;
                    nextbatch = new List<T>(chunkSize);
                }
            }

            if (nextbatch.Count > 0)
            {
                yield return nextbatch;
            }
        }
    }
}
