using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace MedFarLab.Application.Common.Serialization
{
    public static class PwaSerializationConfig
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                PwaJsonContext.Default,
                new DefaultJsonTypeInfoResolver() // Fallback for types not explicitly registered
            )
        };
    }
}
