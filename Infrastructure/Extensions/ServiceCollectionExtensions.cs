
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddAppSetings(config);
        services.AddSwaggers();
        services.AddApiServices();
        services.AddJwtIdentification(config);
        services.AddDataStorage<BaseDbContext>(config);
        return services;
    }
    ///  <summary> 
    ///批量注入Api
    ///  </summary> 
    ///  <param name="services"></param> 
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        //公开接口
        var transientServiceType = typeof(ITransientService);
        var transientServices = AppDomain.CurrentDomain.GetAssemblies()  //获取应用域已加载的的程序集
          .SelectMany(s => s.GetTypes()) //然后获取类型
          .Where(p => transientServiceType.IsAssignableFrom(p)) //加入判断类型是否为某个类的父类 instanceof 关键字判断，
          .Where(t => t.IsClass && !t.IsAbstract)  //isclass 检查是否为一个类  IsAbstract 检查是否为一个抽象类 
          .Select(t => new
          {
              Service = t.GetInterfaces().FirstOrDefault(), //GetInterfaces 获取当前type实现或继承的所有接口，并返回序列的第一个元素，如果序列不包含任何元素，则返回默认值。
              Implementation = t // TService 类型的赋值
          })
          .Where(t => t.Service != null);
        //遍历将服务注册
        foreach (var transientService in transientServices)
        {
            if (transientServiceType.IsAssignableFrom(transientService.Service))
            {
                services.AddTransient(transientService.Service, transientService.Implementation);
            }
        }
        return services;
    }
    public static IServiceCollection AddAppSetings(this IServiceCollection services, IConfiguration config)
    {
        services
            .Configure<MailSettings>(config.GetSection(nameof(MailSettings))); //单独创建
        return services;
    }
}
