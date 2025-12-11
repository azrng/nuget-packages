// using Azrng.Dapper.Repository;
//
// namespace Common.Dapper
// {
//     /// <summary>
//     /// 服务注册扩展
//     /// </summary>
//     public static class ServiceCollectionExtensions
//     {
//         /// <summary>
//         /// 注入dapper服务
//         /// </summary>
//         /// <param name="services"></param>
//         /// <param name="workId"></param>
//         /// <returns></returns>
//         public static IServiceCollection AddDapperHelper(this IServiceCollection services, int workId = 1)
//         {
//             services.AddScoped<IDapperRepository, DapperRepository>();
//             return services;
//         }
//     }
// }
