using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RelinkModOrganizer.Services;
using RelinkModOrganizer.ThirdParties.DataTools;
using RelinkModOrganizer.ViewModels;
using RelinkModOrganizer.Views;

namespace RelinkModOrganizer;

public partial class App : Application
{
    public new static App? Current => Application.Current as App;
    public IServiceProvider ServiceProvider { get; } = new ServiceCollection()
        .AddServices()
        .AddViewModels()
        .BuildServiceProvider();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow(
                dataContext: ServiceProvider.GetRequiredService<MainWindowViewModel>());

        base.OnFrameworkInitializationCompleted();
    }
}

public static partial class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services.AddSingleton<ConfigurationService>()
                .AddSingleton<ModificationService>()
                .AddSingleton<DataToolsService>()
                .AddSingleton<LocalizationService>();

    public static IServiceCollection AddViewModels(this IServiceCollection services) =>
        services.AddSingleton<MainWindowViewModel>()
                .AddSingleton<ModListViewModel>()
                .AddSingleton<LogViewModel>()
                .AddSingleton<SettingsViewModel>()
                .AddSingleton<DialogWindowViewModel>();

    //private static IServiceCollection AddFileProvider(this IServiceCollection services)
    //{
    //    var root = Path.Combine(AppContext.BaseDirectory, Consts.ModsDirName);
    //    if (!Directory.Exists(root))
    //        Directory.CreateDirectory(root);

    //    return services.AddSingleton<IFileProvider>(new PhysicalFileProvider(root));
    //}
}