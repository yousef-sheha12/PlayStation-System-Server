using System.Text.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PlayStation.Domain.Common;

namespace PlayStation.API.Middleware;

public static class ResultFilter
{
    public static void ConfigureJsonOptions(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    }
}

public class ApiResponseFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value != null)
        {
            var valueType = objectResult.Value.GetType();

            if (valueType == typeof(Result))
            {
                var result = (Result)objectResult.Value;
                if (!result.IsSuccess)
                {
                    context.Result = new ObjectResult(result)
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var isSuccess = (bool)valueType.GetProperty("IsSuccess")!.GetValue(objectResult.Value)!;
                if (!isSuccess)
                {
                    context.Result = new ObjectResult(objectResult.Value)
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }
        }
    }
}
