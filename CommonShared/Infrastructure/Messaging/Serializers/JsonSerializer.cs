using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace CommonShared.Infrastructure.Messaging.Serializers;

public class JsonSerializer<T> : ISerializer<T> where T : class
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        if (data == null)
            return null;

        var json = JsonSerializer.Serialize(data);
        return Encoding.UTF8.GetBytes(json);
    }
}