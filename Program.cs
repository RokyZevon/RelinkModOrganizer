﻿using Avalonia;
using Avalonia.ReactiveUI;
using RelinkModOrganizer.Models;
using RelinkModOrganizer.Services;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RelinkModOrganizer;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(Mod))]
[JsonSerializable(typeof(List<Mod>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{ }