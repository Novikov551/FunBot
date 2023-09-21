using Serilog;
using FunBot.Configurations;

namespace FunBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();
        try
        {
            Log.Information("Application Starting");
            await CreateWebHostBuilder(args).Build().RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "The Application failed to start");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateWebHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            })
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true);

                config.AddEmbeddedConfiguration("embeddedsettings.json")
                    .AddEmbeddedConfiguration(
                        $"embeddedsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);

                config.AddEnvironmentVariables();
            })
            .UseSerilog();
    }
}