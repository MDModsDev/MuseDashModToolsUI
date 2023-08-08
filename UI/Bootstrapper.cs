﻿using System.IO.Abstractions;
using System.Net.Http;
using Autofac;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Services;
using MuseDashModToolsUI.ViewModels;
using MuseDashModToolsUI.ViewModels.Dialogs;
using MuseDashModToolsUI.ViewModels.Tabs;

namespace MuseDashModToolsUI;

public static class Bootstrapper
{
    public static void Register()
    {
        var builder = new ContainerBuilder();

        // Instances
        builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
        builder.RegisterInstance(new HttpClient());

        // Services
        builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
        builder.RegisterType<FontManageService>().As<IFontManageService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<GitHubService>().As<IGitHubService>().PropertiesAutowired();
        builder.RegisterType<LocalizationService>().As<ILocalizationService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LocalService>().As<ILocalService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LogAnalyzeService>().As<ILogAnalyzeService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<MessageBoxService>().PropertiesAutowired().As<IMessageBoxService>();
        builder.RegisterType<ModService>().As<IModService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<SavingService>().As<ISavingService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<UpdateTextService>().As<IUpdateTextService>().PropertiesAutowired().SingleInstance();

        // View Models
        builder.RegisterType<ProjectWindowViewModel>().As<IProjectWindowViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<DownloadWindowViewModel>().As<IDownloadWindowViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<ModManageViewModel>().As<IModManageViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<SettingsViewModel>().As<ISettingsViewModel>().SingleInstance();
        builder.RegisterType<LogAnalysisViewModel>().As<ILogAnalysisViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().SingleInstance();

        var container = builder.Build();
        DependencyInjectionExtension.Resolver = type => container.Resolve(type!);
        LocalizeExtensions.LocalizationService = container.Resolve<ILocalizationService>();
        FontExtensions.FontManageService = container.Resolve<IFontManageService>();
    }
}