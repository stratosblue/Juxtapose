namespace Juxtapose.SourceGenerator.Internal;

internal sealed class PersistentStringComparer : StringComparer
{
    #region Public 属性

    public static PersistentStringComparer Instance { get; } = new();

    #endregion Public 属性

    #region Public 方法

    public override int Compare(string x, string y) => x.AsSpan().CompareTo(y.AsSpan(), StringComparison.Ordinal);

    public override bool Equals(string x, string y) => x.AsSpan().SequenceEqual(y.AsSpan());

    public override int GetHashCode(string str)
    {
        // https://stackoverflow.com/questions/36845430/persistent-hashcode-for-strings

        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = (hash1 << 5) + hash1 ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = (hash2 << 5) + hash2 ^ str[i + 1];
            }

            return hash1 + hash2 * 1566083941;
        }
    }

    #endregion Public 方法
}
