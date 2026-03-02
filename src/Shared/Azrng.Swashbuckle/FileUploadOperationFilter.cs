// using Microsoft.AspNetCore.Http;
// #if NET10_0_OR_GREATER
// using Microsoft.OpenApi;
// #else
// using Microsoft.OpenApi.Models;
// #endif
// using Swashbuckle.AspNetCore.SwaggerGen;
// using System.Reflection;
//
// namespace Azrng.Swashbuckle
// {
//     /// <summary>
//     /// 文件上传操作过滤器，用于处理 IFormFile 类型的参数
//     /// </summary>
//     public class FileUploadOperationFilter : IOperationFilter
//     {
//         public void Apply(OpenApiOperation operation, OperationFilterContext context)
//         {
//             var fileParameters = context.MethodInfo.GetParameters()
//                 .Where(p => p.ParameterType == typeof(IFormFile) ||
//                            p.ParameterType == typeof(IFormFileCollection))
//                 .ToList();
//
//             if (fileParameters.Count == 0)
//             {
//                 return;
//             }
//
//             // 移除原有的 IFormFile 参数
//             foreach (var parameter in fileParameters)
//             {
//                 var paramToRemove = operation.Parameters?.FirstOrDefault(p => p.Name == parameter.Name);
//                 if (paramToRemove != null)
//                 {
//                     operation.Parameters.Remove(paramToRemove);
//                 }
//             }
//
// #if NET10_0_OR_GREATER
//             // .NET 10+ 实现
//             if (operation.RequestBody == null)
//             {
//                 operation.RequestBody = new OpenApiRequestBody
//                 {
//                     Content = new Dictionary<string, OpenApiMediaType>
//                     {
//                         ["multipart/form-data"] = new OpenApiMediaType
//                         {
//                             Schema = new OpenApiSchema
//                             {
//                                 Type = JsonSchemaType.Object,
//                                 Properties = new Dictionary<string, IOpenApiSchema>(),
//                                 Required = new HashSet<string>()
//                             }
//                         }
//                     }
//                 };
//             }
//
//             var formDataSchema = operation.RequestBody.Content["multipart/form-data"].Schema;
//
//             // 为每个 IFormFile 参数添加属性
//             foreach (var parameter in fileParameters)
//             {
//                 var isRequired = !parameter.IsOptional &&
//                                parameter.HasDefaultValue &&
//                                context.ApiDescription.ParameterDescriptions
//                                    .FirstOrDefault(p => p.Name == parameter.Name)?.IsRequired == true;
//
//                 formDataSchema.Properties![parameter.Name] = new OpenApiSchema
//                 {
//                     Type = JsonSchemaType.String,
//                     Format = "binary"
//                 };
//
//                 if (isRequired)
//                 {
//                     formDataSchema.Required.Add(parameter.Name);
//                 }
//             }
//
//             // 添加其他 FromForm 参数
//             var otherFormParameters = context.MethodInfo.GetParameters()
//                 .Where(p => p.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null &&
//                            p.ParameterType != typeof(IFormFile) &&
//                            p.ParameterType != typeof(IFormFileCollection))
//                 .ToList();
//
//             foreach (var parameter in otherFormParameters)
//             {
//                 var propertySchema = context.SchemaGenerator.GenerateSchema(parameter.ParameterType, context.SchemaRepository);
//                 formDataSchema.Properties![parameter.Name] = propertySchema;
//
//                 if (!parameter.IsOptional)
//                 {
//                     formDataSchema.Required.Add(parameter.Name);
//                 }
//             }
// #else
//             // .NET 6-9 实现
//             if (operation.RequestBody == null)
//             {
//                 operation.RequestBody = new OpenApiRequestBody
//                 {
//                     Content = new Dictionary<string, OpenApiMediaType>
//                     {
//                         ["multipart/form-data"] = new OpenApiMediaType
//                         {
//                             Schema = new OpenApiSchema
//                             {
//                                 Type = "object",
//                                 Properties = new Dictionary<string, OpenApiSchema>(),
//                                 Required = new HashSet<string>()
//                             }
//                         }
//                     }
//                 };
//             }
//
//             var formDataSchema = operation.RequestBody.Content["multipart/form-data"].Schema;
//
//             // 为每个 IFormFile 参数添加属性
//             foreach (var parameter in fileParameters)
//             {
//                 var isRequired = !parameter.IsOptional &&
//                                parameter.HasDefaultValue &&
//                                context.ApiDescription.ParameterDescriptions
//                                    .FirstOrDefault(p => p.Name == parameter.Name)?.IsRequired == true;
//
//                 formDataSchema.Properties[parameter.Name] = new OpenApiSchema
//                 {
//                     Type = "string",
//                     Format = "binary"
//                 };
//
//                 if (isRequired)
//                 {
//                     formDataSchema.Required.Add(parameter.Name);
//                 }
//             }
//
//             // 添加其他 FromForm 参数
//             var otherFormParameters = context.MethodInfo.GetParameters()
//                 .Where(p => p.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null &&
//                            p.ParameterType != typeof(IFormFile) &&
//                            p.ParameterType != typeof(IFormFileCollection))
//                 .ToList();
//
//             foreach (var parameter in otherFormParameters)
//             {
//                 var propertySchema = context.SchemaGenerator.GenerateSchema(parameter.ParameterType, context.SchemaRepository);
//                 formDataSchema.Properties[parameter.Name] = propertySchema;
//
//                 if (!parameter.IsOptional)
//                 {
//                     formDataSchema.Required.Add(parameter.Name);
//                 }
//             }
// #endif
//         }
//     }
// }
