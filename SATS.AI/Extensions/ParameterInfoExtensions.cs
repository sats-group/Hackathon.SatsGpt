using System.Reflection;
using System.Runtime.CompilerServices;

namespace SATS.AI.Extensions;

public static class ParameterInfoExtensions
{
    /// <summary>
    /// Checks if a reference type parameter is marked as nullable in a nullable context.
    /// </summary>
    public static bool HasNullableAnnotation(this ParameterInfo param)
    {
        var nullableAttr = param.GetCustomAttribute<NullableAttribute>();
        return nullableAttr != null && nullableAttr.NullableFlags.Length > 0 && nullableAttr.NullableFlags[0] == 2;
    }
}

