using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Baseline.Labourer.Internal.Utils;

/// <summary>
/// Contains numerous utilities related to serialization/deserialization of objects to/from JSON.
/// </summary>
public static class SerializationUtils
{
    /// <summary>
    /// Serializes an object to a string.
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    public static async Task<string> SerializeToStringAsync<T>(T obj)
    {
        await using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(
            memoryStream,
            obj,
            new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles }
        );
        memoryStream.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(memoryStream).ReadToEndAsync();
    }

    /// <summary>
    /// Deserializes a string into an object defined by the type parameter.
    /// </summary>
    /// <param name="serialized"></param>
    /// <param name="type"></param>
    public static async Task<object> DeserializeFromStringAsync(string serialized, Type type)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
        return (await JsonSerializer.DeserializeAsync(stream, type, new JsonSerializerOptions()))!;
    }

    /// <summary>
    /// Deserializes a string into an object defined by the generic type of the method.
    /// </summary>
    /// <param name="serialized"></param>
    /// <typeparam name="T"></typeparam>
    public static async Task<T> DeserializeFromStringAsync<T>(string serialized)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
        return (await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions()))!;
    }
}
