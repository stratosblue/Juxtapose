using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Juxtapose;

/// <summary>
/// 设置选项
/// </summary>
[method: DebuggerStepThrough]
public class JuxtaposeOptions(Dictionary<string, string?>? values = null)
    : IJuxtaposeOptions
{
    #region Private 字段

    private readonly Dictionary<string, string?> _values = values is null
                                                           ? new(8)
                                                           : new(values, StringComparer.OrdinalIgnoreCase);

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public string ContextIdentifier { get => Get(); set => Set(value); }

    /// <inheritdoc/>
    public bool EnableDebugger { get => GetUInt32() > 0; set => Set(value ? 1 : 0); }

    /// <inheritdoc/>
    public int? ParentProcessId { get => GetInt32(); set => Set(value); }

    /// <inheritdoc/>
    public string SessionId { get => Get(); set => Set(value); }

    /// <inheritdoc/>
    public uint Version { get => GetUInt32() ?? 0; set => Set(value); }

    #endregion Public 属性

    #region Public 索引器

    /// <inheritdoc/>
    public string? this[string key] { get => Get(key); set => Set(value, key); }

    #endregion Public 索引器

    #region Public 方法

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public IJuxtaposeOptions Clone()
    {
        return new JuxtaposeOptions(_values);
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _values.TryGetValue(key, out value);

    #region IEnumerable

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator() => _values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion IEnumerable

    #endregion Public 方法

    #region Deserialize

    /// <inheritdoc/>
    public static JuxtaposeOptions Deserialize(ReadOnlySpan<char> data)
    {
        try
        {
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            while (data.Length > 2)
            {
                var keyEndIndex = data.IndexOf('\t');
                var valueEndIndex = data.IndexOf('\n');
                var key = data.Slice(0, keyEndIndex).ToString();
                var value = data.Slice(keyEndIndex + 1, valueEndIndex - keyEndIndex - 1).ToString();
                data = data.Slice(valueEndIndex + 1);
                values.Add(key, value);
            }
            return new JuxtaposeOptions(values);
        }
        catch (Exception ex)
        {
            throw new JuxtaposeException($"the options string may has some error - \"{data}\" .", ex);
        }
    }

    #endregion Deserialize

    #region GetSet

#pragma warning disable CS8604 // 引用类型参数可能为 null。

    private string Get([CallerMemberName] string? key = null)
    {
        _values.TryGetValue(key, out var value);
        return value ?? string.Empty;
    }

    private int? GetInt32([CallerMemberName] string? key = null)
    {
        _values.TryGetValue(key, out var value);
        return string.IsNullOrWhiteSpace(value)
               ? null
               : int.Parse(value);
    }

    private uint? GetUInt32([CallerMemberName] string? key = null)
    {
        _values.TryGetValue(key, out var value);
        return string.IsNullOrWhiteSpace(value)
               ? null
               : uint.Parse(value);
    }

    private void Set(string? value, [CallerMemberName] string? key = null)
    {
        if (value is null)
        {
            _values.Remove(key);
        }
        else
        {
            _values[key] = value;
        }
    }

    private void Set(object? value, [CallerMemberName] string? key = null)
    {
        if (value is null)
        {
            _values.Remove(key);
        }
        else
        {
            _values[key] = value.ToString();
        }
    }

#pragma warning restore CS8604 // 引用类型参数可能为 null。

    #endregion GetSet
}
