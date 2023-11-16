using System.Buffers;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Juxtapose.Communication.Codec;

/// <summary>
/// 默认基于json的编码器
/// </summary>
public class DefaultJsonBasedMessageCodec : ICommunicationMessageCodec
{
    #region Private 字段

    private readonly ILogger _logger;

    private readonly ReadOnlyDictionary<int, Type> _messageIdTypes;

    private readonly ReadOnlyDictionary<Type, int> _messageTypeIds;

    private readonly JsonSerializerOptions _serializerOptions;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="DefaultJsonBasedMessageCodec"/>
    public DefaultJsonBasedMessageCodec(IEnumerable<Type> messageTypes, ILoggerFactory loggerFactory, JsonSerializerOptions? jsonSerializerOptions) : this(IndexMessageTypes(messageTypes), loggerFactory, jsonSerializerOptions)
    {
    }

    /// <inheritdoc cref="DefaultJsonBasedMessageCodec"/>
    public DefaultJsonBasedMessageCodec(IEnumerable<KeyValuePair<int, Type>> messageTypes, ILoggerFactory loggerFactory, JsonSerializerOptions? jsonSerializerOptions)
    {
        _messageIdTypes = new ReadOnlyDictionary<int, Type>(new Dictionary<int, Type>(messageTypes ?? throw new ArgumentNullException(nameof(messageTypes))));
        _messageTypeIds = new ReadOnlyDictionary<Type, int>(_messageIdTypes.ToDictionary(m => m.Value, m => m.Key));

        _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger("Juxtapose.Communication.Codec.DefaultJsonBasedMessageCodec");

        _serializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IncludeFields = true,
        };
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public object Decode(ReadOnlySequence<byte> buffer)
    {
        var typeIdentifier = BitConverter.ToInt32(buffer.Slice(0, sizeof(int)).ToArray());

        if (!_messageIdTypes.TryGetValue(typeIdentifier, out var messageType))
        {
            throw new MessageParseFailException($"消息解析失败，未知的消息类型 - {typeIdentifier}");
        }

        var jsonReader = new Utf8JsonReader(buffer.Slice(sizeof(int)));
        try
        {
            return JsonSerializer.Deserialize(ref jsonReader, messageType, _serializerOptions) ?? throw new MessageParseFailException($"消息解析失败，反序列化到对象 {messageType} 失败");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JsonSerializer.Deserialize Error \n --------origin message data------- \n [{OriginMessageData}]", Encoding.UTF8.GetString(buffer.Slice(sizeof(int))));
            throw;
        }
    }

    /// <inheritdoc/>
    public ValueTask<long> Encode(object message, IBufferWriter<byte> bufferWriter)
    {
        var messageType = message.GetType();
        if (!_messageTypeIds.TryGetValue(messageType, out var typeIdentifier))
        {
            throw new MessageParseFailException($"消息序列化失败，未登记的消息类型 - {messageType}");
        }

        var typeMemory = bufferWriter.GetMemory(sizeof(int));
        BitConverter.GetBytes(typeIdentifier).CopyTo(typeMemory);
        bufferWriter.Advance(sizeof(int));

        using var jsonWriter = new Utf8JsonWriter(bufferWriter);
        try
        {
            JsonSerializer.Serialize(jsonWriter, message, messageType, _serializerOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JsonSerializer.Serialize Error \n --------message------- \n [{Message}]", message);
            throw;
        }
        return new ValueTask<long>(jsonWriter.BytesCommitted + sizeof(int));
    }

    #endregion Public 方法

    #region Util

    /// <summary>
    /// 索引消息列表
    /// </summary>
    /// <param name="messageTypes"></param>
    /// <returns></returns>
    public static Dictionary<int, Type> IndexMessageTypes(IEnumerable<Type> messageTypes)
    {
        var indexedMessageTypes = new Dictionary<int, Type>();
        var index = 1;
        foreach (var messageType in messageTypes)
        {
            indexedMessageTypes.Add(index++, messageType);
        }
        return indexedMessageTypes;
    }

    #endregion Util
}
