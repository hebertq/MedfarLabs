using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MedfarLabs.Core.Domain.Common.Responses.Generic;

namespace MedFarLab.Api.Converters
{
    public class BaseResponseObjectConverter : JsonConverter<BaseResponse<object>>
    {
        public override BaseResponse<object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Deserialization of BaseResponse<object> is not supported.");
        }

        public override void Write(Utf8JsonWriter writer, BaseResponse<object> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            
            writer.WriteBoolean("isSuccess", value.IsSuccess);
            
            if (value.Message != null)
            {
                writer.WriteString("message", value.Message);
            }
            else
            {
                writer.WriteNull("message");
            }

            if (value.Errors != null)
            {
                writer.WritePropertyName("errors");
                JsonSerializer.Serialize(writer, value.Errors, options);
            }

            writer.WritePropertyName("data");
            if (value.Data == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                // Key fix for Native AOT: Pass the exact runtime type so that the serializer
                // can resolve it correctly from the TypeInfoResolver without relying on 
                // unsupported polymorphic "object" serialization.
                JsonSerializer.Serialize(writer, value.Data, value.Data.GetType(), options);
            }
            
            writer.WriteEndObject();
        }
    }
}
