using Microsoft.Extensions.DependencyInjection;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Azrng.Swashbuckle
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultSwaggerGen(this IServiceCollection services,
                                                              Action<SwaggerGenOptions>? action = null, string? title = null,
                                                              bool showJwtToken = false)
        {
            services.AddDefaultSwaggerGen(new OpenApiInfo { Title = title ?? "SwaggerAPI", Version = "v1" }, action, showJwtToken);
            return services;
        }

        /// <summary>
        /// 添加默认swagger配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="apiInfo"></param>
        /// <param name="action"></param>
        /// <param name="showJwtToken">是否显示jwtToken</param>
        /// <returns></returns>
        /// <remarks>如果想显示控制器等注释到swagger，要生成项目文档</remarks>
        public static IServiceCollection AddDefaultSwaggerGen(this IServiceCollection services, OpenApiInfo apiInfo,
                                                              Action<SwaggerGenOptions>? action = null, bool showJwtToken = false)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", apiInfo);

                // action 排序
                c.OrderActionsBy(o => o.RelativePath);

                //设置文档描述
                Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml")
                         .ToList()
                         .ForEach(file => c.IncludeXmlComments(file));

                if (showJwtToken)
                {
#if NET10_0_OR_GREATER
                    var security = new OpenApiSecurityScheme
                    {
                        Name = "Authorization", //jwt默认的参数名称
                        Description = "JWT授权(数据将在请求头中进行传输) 在下方输入Bearer {token} 即可，注意两者之间有空格",
                        In = ParameterLocation.Header, //jwt默认存放Authorization信息的位置(请求头中)
                        Type = SecuritySchemeType.ApiKey,
                        BearerFormat = "JWT"
                    };
                    c.AddSecurityDefinition("Bearer", security);
                    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
#else
                      var security = new OpenApiSecurityScheme
                                   {
                                       Name = "Authorization", //jwt默认的参数名称
                                       Description = "JWT授权(数据将在请求头中进行传输) 在下方输入Bearer {token} 即可，注意两者之间有空格",
                                       In = ParameterLocation.Header, //jwt默认存放Authorization信息的位置(请求头中)
                                       Type = SecuritySchemeType.ApiKey,
                                       Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
                                   };
                    c.AddSecurityDefinition("Bearer", security);
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { security, Array.Empty<string>() } });
#endif
                }

                // 对swaggerDoc做一些特殊的处理
                action?.Invoke(c);
            });

            return services;
        }
    }
}