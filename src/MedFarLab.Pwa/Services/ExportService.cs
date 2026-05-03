using System.Text;
using Microsoft.JSInterop;
using System.Reflection;

namespace MedFarLab.Pwa.Services
{
    public interface IExportService
    {
        Task ExportToCsvAsync<T>(IEnumerable<T> data, string fileName);
    }

    public class ExportService : IExportService
    {
        private readonly IJSRuntime _jsRuntime;

        public ExportService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task ExportToCsvAsync<T>(IEnumerable<T> data, string fileName)
        {
            if (data == null || !data.Any())
                return;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                                 .ToList();

            var csv = new StringBuilder();

            // Escribir cabeceras
            var headers = properties.Select(p => EscapeCsvValue(p.Name));
            csv.AppendLine(string.Join(",", headers));

            // Escribir filas
            foreach (var item in data)
            {
                var row = properties.Select(p =>
                {
                    var val = p.GetValue(item, null);
                    return EscapeCsvValue(val?.ToString());
                });
                csv.AppendLine(string.Join(",", row));
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var base64 = Convert.ToBase64String(bytes);

            // Asegurarse de que el archivo termine en .csv
            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".csv";
            }

            // Invocar JS Interop
            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, "text/csv;charset=utf-8", base64);
        }

        private string EscapeCsvValue(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            // Si contiene comas, comillas o saltos de línea, escapar
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        private bool IsSimpleType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsPrimitive 
                || underlyingType.IsEnum 
                || underlyingType == typeof(string) 
                || underlyingType == typeof(decimal) 
                || underlyingType == typeof(DateTime) 
                || underlyingType == typeof(DateTimeOffset) 
                || underlyingType == typeof(TimeSpan) 
                || underlyingType == typeof(Guid);
        }
    }
}
