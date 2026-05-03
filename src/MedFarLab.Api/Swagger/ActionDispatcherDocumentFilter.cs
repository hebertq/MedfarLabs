using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MedfarLabs.Core.Domain.Common.Attributes;
using MedfarLabs.Core.Domain.Const;
using System.Linq;
using System.Collections.Generic;

namespace MedFarLab.Api.Swagger
{
    public class ActionDispatcherDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // 1. Encontramos todas las clases explícitamente cargando los ensamblados de MedfarLabs
            var assemblies = System.IO.Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Where(f => f.Contains("MedFarLab") || f.Contains("MedfarLabs"))
                .Select(f => { try { return Assembly.LoadFrom(f); } catch { return null; } })
                .Where(a => a != null)
                .ToList();

            var typesWithMapping = assemblies
                .SelectMany(a => 
                {
                    try { return a!.GetTypes(); } catch { return new System.Type[0]; }
                })
                .Where(t => t.GetCustomAttribute<ActionMappingAttribute>() != null)
                .ToList();

            // 2. Extraemos un diccionario para los nombres de las acciones
            var actionNameDictionary = new Dictionary<int, string>();
            var nestedActionClasses = typeof(AppAction).GetNestedTypes();
            foreach (var nestedClass in nestedActionClasses)
            {
                var fields = nestedClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                foreach (var field in fields)
                {
                    if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(int))
                    {
                        var rawValue = field.GetRawConstantValue();
                        if (rawValue != null)
                        {
                            int actionCode = (int)rawValue;
                            actionNameDictionary[actionCode] = field.Name;
                        }
                    }
                }
            }

            // 3. Crear rutas dinámicas pseudo-nativas
            foreach (var type in typesWithMapping)
            {
                var actionMapping = type.GetCustomAttribute<ActionMappingAttribute>();
                if (actionMapping == null) continue;

                // Corregimos los módulos en la API si es que difieren del Enum, pero mantenemos simpleza
                var moduleName = actionMapping.Module.ToString();
                var actionCode = actionMapping.Action;

                string actionName = actionNameDictionary.ContainsKey(actionCode) ? actionNameDictionary[actionCode] : $"Action{actionCode}";

                // Determinamos el verbo HTTP
                var method = actionMapping.GetType().GetProperty("Method")?.GetValue(actionMapping) as string ?? "POST";
                bool isGet = method.Equals("GET", System.StringComparison.OrdinalIgnoreCase);

                // Construimos la ruta literal en Swagger (ej. /api/Care/5001)
                string routePath = $"/api/{moduleName}/{actionCode}";

                // Registramos el esquema DTO de entrada y el BaseResponse de salida genérica
                var schemaRef = context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
                var baseResponseSchema = context.SchemaGenerator.GenerateSchema(typeof(MedfarLabs.Core.Domain.Common.Responses.Generic.BaseResponse<object>), context.SchemaRepository);

                var operation = new OpenApiOperation
                {
                    Summary = $"{actionName} ({actionCode})",
                    Description = $"Endpoint dinámico autogenerado mapeado al DTO `{type.Name}`.",
                    Tags = new List<OpenApiTag> { new OpenApiTag { Name = moduleName } },
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "Operación Ejecutada Exitosamente.", Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new OpenApiMediaType { Schema = baseResponseSchema } } },
                        ["400"] = new OpenApiResponse { Description = "Validación de Negocio / Petición Inválida.", Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new OpenApiMediaType { Schema = baseResponseSchema } } },
                        ["401"] = new OpenApiResponse { Description = "Acceso Restringido / No Autorizado.", Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new OpenApiMediaType { Schema = baseResponseSchema } } },
                        ["404"] = new OpenApiResponse { Description = "Recurso No Encontrado.", Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new OpenApiMediaType { Schema = baseResponseSchema } } },
                        ["500"] = new OpenApiResponse { Description = "Error Interno / PersistenceException.", Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new OpenApiMediaType { Schema = baseResponseSchema } } }
                    }
                };

                if (isGet)
                {
                    operation.Parameters = new List<OpenApiParameter>
                    {
                        new OpenApiParameter
                        {
                            Name = "payload",
                            In = ParameterLocation.Query,
                            Description = $"Estructura esperada: Schema de {type.Name} en formato JSON string.",
                            Required = true,
                            Schema = new OpenApiSchema { Type = "string" }
                        }
                    };
                }
                else
                {
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Required = true,
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType { Schema = schemaRef }
                        }
                    };
                }

                var pathItem = new OpenApiPathItem();
                pathItem.AddOperation(isGet ? OperationType.Get : OperationType.Post, operation);

                if (!swaggerDoc.Paths.ContainsKey(routePath))
                {
                    swaggerDoc.Paths.Add(routePath, pathItem);
                }
            }
        }
    }
}
