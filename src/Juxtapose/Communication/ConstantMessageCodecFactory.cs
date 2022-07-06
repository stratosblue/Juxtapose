using System;
using System.Collections.Generic;
using Juxtapose.Communication.Codec;
using Microsoft.Extensions.Logging;

namespace Juxtapose;

/// <summary>
/// 固定类型的消息编解码器
/// </summary>
public class ConstantMessageCodecFactory : ICommunicationMessageCodecFactory
{
    #region Private 字段

    private readonly DefaultJsonBasedMessageCodec _messageCodec;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="ConstantMessageCodecFactory"/>
    public ConstantMessageCodecFactory(IEnumerable<Type> messageTypes, ILoggerFactory loggerFactory)
    {
        var messageTypesMap = new Dictionary<int, Type>();
        var index = 1;
        foreach (var messageType in messageTypes)
        {
            messageTypesMap.Add(index++, messageType);
        }
        _messageCodec = new DefaultJsonBasedMessageCodec(messageTypesMap, loggerFactory);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public ICommunicationMessageCodec Create(IJuxtaposeOptions options) => _messageCodec;

    #endregion Public 方法
}