using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Haskoli.AvaloniaUI.ViewModels;
using Haskoli.AvaloniaUI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using Microsoft.Extensions.Hosting.Internal;
using Haskoli.Application.ServiceCollection;
using Haskoli.AvaloniaUI.Messenger;


namespace Haskoli.AvaloniaUI
{
    public partial class App : Avalonia.Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

                // Configura la instancia de IConfiguration y reg�strala en el contenedor de servicios.
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                services.AddSingleton<IConfiguration>(configuration);

                // adding Serilog settings
                Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(configuration)
                   .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                   .WriteTo.Console()
                   .CreateLogger();

                services.AddApplicationLayer();
                services.AddLogging(builder =>
                {
                    builder.AddSerilog();
                });

                // Crea el servicio de mensajería
                var messageService = new MessageService();
                services.AddSingleton<MessageService>(messageService);

                // Build the service provider.
                var serviceProvider = services.BuildServiceProvider();

                //using (var dbContextParkAccess = serviceProvider.GetRequiredService<ParkAccessDbContext>())
                //{
                //    dbContextParkAccess.Database.Migrate();
                //}

                //using var dbContextIdentityParkAccess = serviceProvider.GetRequiredService<ParkAccessIdentityDbContext>();
                //dbContextIdentityParkAccess.Database.Migrate();

                desktop.MainWindow = new MainWindow(serviceProvider.GetRequiredService<MessageService>())
                {
                    DataContext = new MainWindowViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}