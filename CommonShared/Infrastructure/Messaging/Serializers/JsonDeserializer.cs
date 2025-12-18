using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace CommonShared.Infrastructure.Messaging.Serializers;

public class JsonDeserializer<T> : IDeserializer<T> where T : class
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull || data.IsEmpty)
            return null!;

        var json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}
