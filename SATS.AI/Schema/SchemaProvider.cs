using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using SATS.AI.Extensions;

namespace SATS.AI.Schema;

public static class SchemaProvider
{
    public static BinaryData GenerateBinarySchema<T>(bool allowAdditionalProperties)
    {
        var schema = GenerateSchema<T>(allowAdditionalProperties);
        var jsonSchema = JsonConvert.SerializeObject(schema);
        return BinaryData.FromString(jsonSchema);
    }

    public static JSchema GenerateSchema<T>(bool allowAdditionalProperties)
    {
        var generator = new JSchemaGenerator
        {
            DefaultRequired = Required.Default
        };

        generator.GenerationProviders.Add(new StringEnumGenerationProvider());
        var schema = generator.Generate(typeof(T));

        if (!allowAdditionalProperties)
        {
            SetAdditionalPropertiesFalse(schema);
        }

        return schema;
    }

    public static JSchema? GenerateParameterSchema(MethodInfo method)
    {
        var methodParameters = method.GetParameters()
            .Where(p => p.ParameterType != typeof(CancellationToken))
            .ToList();

        if (methodParameters.Count == 0)
        {
            return null;
        }

        var generator = new JSchemaGenerator
        {
            DefaultRequired = Required.Default
        };

        generator.GenerationProviders.Add(new StringEnumGenerationProvider());

        var schema = new JSchema
        {
            Type = JSchemaType.Object
        };

        foreach (var param in methodParameters)
        {
            var paramSchema = generator.Generate(param.ParameterType);
            schema.Properties.Add(param.Name!, paramSchema);

            if (!IsNullable(param))
            {
                schema.Required.Add(param.Name!);
            }
        }

        return schema;
    }

    private static void SetAdditionalPropertiesFalse(JSchema schema)
    {
        if (schema.Type.HasValue && schema.Type == JSchemaType.Object)
        {
            schema.AllowAdditionalProperties = false;
        }

        foreach (var property in schema.Properties.Values)
        {
            SetAdditionalPropertiesFalse(property);
        }

        foreach (var itemSchema in schema.Items)
        {
            SetAdditionalPropertiesFalse(itemSchema);
        }
    }

    private static bool IsNullable(ParameterInfo param)
    {
        if (param.ParameterType.IsValueType)
        {
            return Nullable.GetUnderlyingType(param.ParameterType) != null;
        }
        else
        {
            return param.HasNullableAnnotation();
        }
    }
}
