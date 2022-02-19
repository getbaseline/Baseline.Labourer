using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Internal
{
    /// <summary>
    /// Contains numerous utilities related to serialization/deserialization of objects to/from JSON.
    /// </summary>
    public static class SerializationUtils
    {
        /// <summary>
        /// Serializes an object to a string.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        public static async Task<string> SerializeToStringAsync<T>(T obj, CancellationToken cancellationToken = default)
        {
            await using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, obj, new JsonSerializerOptions(), cancellationToken);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return await new StreamReader(memoryStream).ReadToEndAsync();
        }

        /// <summary>
        /// Deserializes a string into an object defined by the type parameter. 
        /// </summary>
        /// <param name="serialized"></param>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        public static async Task<object> DeserializeFromStringAsync(
            string serialized,
            Type type,
            CancellationToken cancellationToken
        )
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            return (await JsonSerializer.DeserializeAsync(stream, type, new JsonSerializerOptions(), cancellationToken))!;
        }

        /// <summary>
        /// Deserializes a string into an object defined by the generic type of the method.
        /// </summary>
        /// <param name="serialized"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        public static async Task<T> DeserializeFromStringAsync<T>(
            string serialized,
            CancellationToken cancellationToken = default
        )
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            return (await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions(), cancellationToken))!;
        }
    }
}