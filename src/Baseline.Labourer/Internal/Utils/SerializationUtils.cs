using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Internal.Utils
{
    public static class SerializationUtils
    {
        public static async Task<string> SerializeToStringAsync<T>(T obj, CancellationToken cancellationToken)
        {
            await using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync<T>(memoryStream, obj, new JsonSerializerOptions(), cancellationToken);
            return await new StreamReader(memoryStream).ReadToEndAsync();
        }
    }
}