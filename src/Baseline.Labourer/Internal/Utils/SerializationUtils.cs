using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Internal.Utils
{
    public static class SerializationUtils
    {
        public static async Task<string> SerializeToStringAsync<T>(T obj, CancellationToken cancellationToken = default)
        {
            await using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync<T>(memoryStream, obj, new JsonSerializerOptions(), cancellationToken);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return await new StreamReader(memoryStream).ReadToEndAsync();
        }

        public static async Task<object> DeserializeFromStringAsync(
            string serialized, 
            Type type,
            CancellationToken cancellationToken
        )
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            return await JsonSerializer.DeserializeAsync(stream, type, new JsonSerializerOptions(), cancellationToken);
        }

        public static async Task<T> DeserializeFromStringAsync<T>(
            string serialized,
            CancellationToken cancellationToken = default
        )
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            return await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions(), cancellationToken);
        }
    }
}