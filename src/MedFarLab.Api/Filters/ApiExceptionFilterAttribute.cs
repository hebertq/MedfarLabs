using MedfarLabs.Core.Domain.Common.Responses.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace MedFarLab.Api.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            int statusCode = 500;
            Console.WriteLine("API Exception Filter Hit:");
            Console.WriteLine(context.Exception.ToString());
            string message = "An unexpected error occurred in the API.";
            List<string> errors = new List<string> { context.Exception.Message };

            if (context.Exception is MedfarLabs.Core.Domain.Common.Exceptions.BusinessValidationException busEx)
            {
                statusCode = 400;
                message = "Validación de Negocio";
                errors = busEx.Response?.Errors ?? errors;
            }
            else if (context.Exception is UnauthorizedAccessException unauthEx)
            {
                statusCode = 401;
                message = "No autorizado";
            }
            else if (context.Exception is KeyNotFoundException)
            {
                statusCode = 404;
                message = "Recurso no encontrado";
            }
            else if (context.Exception is MedfarLabs.Core.Domain.Common.Exceptions.PersistenceException)
            {
                statusCode = 500;
                message = "Error en Base de Datos";
            }

            var errorResponse = BaseResponse<object>.Failure(
                message: $"{message}: {context.Exception.Message}",
                errors: errors
            );

            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
            base.OnException(context);
        }
    }
}
