using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using RelinkModOrganizer.Services;
using RelinkModOrganizer.ThirdParties.DataTools;
using RelinkModOrganizer.ViewModels;
using RelinkModOrganizer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace RelinkModOrganizer;

public partial class App : Application
{
    public override void Initialize() =>
        AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddServices();
        services.AddViewModels();

        var serviceProvider = services.BuildServiceProvider();
        var mainWindowViewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };

            services.AddSingleton<IStorageProvider>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public static partial class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services.AddSingleton<ConfigurationService>()
                .AddSingleton<ModificationService>()
                .AddSingleton<DataToolsService>()
                .AddSingleton <DialogService>()
                .AddFileProvider();

    public static IServiceCollection AddViewModels(this IServiceCollection services) =>
        services.AddSingleton<MainWindowViewModel>()
                .AddSingleton<ModListViewModel>()
                .AddSingleton<LogViewModel>()
                .AddSingleton<SettingsViewModel>()
                .AddSingleton<DialogWindowViewModel>();

    private static IServiceCollection AddFileProvider(this IServiceCollection services)
    {
        var root = Path.Combine(AppContext.BaseDirectory, Consts.ModsDirName);
        if (!Directory.Exists(root))
            Directory.CreateDirectory(root);

        return services.AddSingleton<IFileProvider>(new PhysicalFileProvider(root));
    }
}