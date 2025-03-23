using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace ScriptDeployTools.Targets.SqlServer;

internal static class DbValues
{
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? FromDb<T>(this IDataRecord record, string fieldName, T? defaultValue)
    {
        var value = record[fieldName];

        if (Convert.IsDBNull(value))
            return defaultValue;

        return (T)value;
    }

    public static T? FromDb<T>(this IDataRecord record, string fieldName)
    {
        return record.FromDb<T>(fieldName, default);
    }
}
