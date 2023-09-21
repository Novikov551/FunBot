using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FunBot.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddRepositoryContext(configuration, environment);
        }

        private static IServiceCollection AddRepositoryContext(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
        {   //TODO
            //services.AddDbContext<DBCONTEXT>(options =>
            //{
            //    options.UseNpgsql(configuration.GetConnectionString(nameof(DBCONTEXT)));

            //    if (environment.IsDevelopment())
            //    {
            //        options.EnableSensitiveDataLogging();
            //    }
            //});

            //services.AddScoped<IUnitOfWork>(s => s.GetService<DBCONTEXT>()!);

            return services;
        }
    }
}
