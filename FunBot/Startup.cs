using FunBot.Extensions;
using FunBot.Infrastructure.Extensions;
using FunBot.Infrastructure.MediatR;
using FunBot.Logic.Extensions;
using FunBot.Services;
using Microsoft.AspNetCore.Localization;
using Serilog;
using System.Globalization;
using System.Reflection;

namespace FunBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        public const string CorsPolicyName = "SpecificCors";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfigOptions(Configuration);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ru") };

                options.DefaultRequestCulture = new RequestCulture("ru");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.AddServiceHealthChecks(Configuration);

            services.AddInfrastructureServices(Configuration, Environment);

            var assembly = Assembly.Load("FunBot.Logic");
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assembly);
            });

            services.AddLogic();
            services.AddHealthChecks();

            services.AddControllers();

            services.AddSwagger();

            services.AddScoped<MediatorSchedulerBridge>();

            services.AddHostedService<StartService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseForwarderHeaders();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FunBot v1"));

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseCors(CorsPolicyName);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();

                //TODO
                //endpoints.MapHealthChecks("/healthcheck", new HealthCheckOptions
                //{
                //    ResponseWriter = async (context, report) =>
                //    {
                //        var result = JsonConvert.SerializeObject(new HealthReport
                //        {
                //            ServiceName = "FunBot",
                //            Status = report.Status,
                //            TotalDuration = report.TotalDuration,
                //            Entries = report.Entries.Select(e => new HealthReportEntry
                //            {
                //                Data = e.Value.Data,
                //                Duration = e.Value.Duration,
                //                EntryName = e.Key,
                //                Status = e.Value.Status,
                //                Tags = e.Value.Tags.ToArray(),
                //            }).ToArray(),
                //        });

                //        context.Response.ContentType = MediaTypeNames.Application.Json;
                //        await context.Response.WriteAsync(result);
                //    }
                //});
            });
        }
    }
}