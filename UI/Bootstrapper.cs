using System;
using System.Net.Http;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Services;
using MuseDashModToolsUI.ViewModels;
using Splat;

namespace MuseDashModToolsUI;

public static class Bootstrapper
{
    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.Register<IDialogueService>(() => new DialogueService()); // Call services.Register<T> and pass it lambda that creates instance of your service
        services.Register<IGitHubService>(() => new GitHubService(new HttpClient(), GetRequiredService<IDialogueService>(resolver))); // Call services.Register<T> and pass it lambda that creates instance of your service
        services.Register<ILocalService>(() => new LocalService()); // Call services.Register<T> and pass it lambda that creates instance of your service
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ILocalService>(),
            resolver.GetRequiredService<IDialogueService>()));
    }
    public static TService GetRequiredService<TService>(this IReadonlyDependencyResolver resolver)
    {
        var service = resolver.GetService<TService>();
        if (service is null) // Splat is not able to resolve type for us
        {
            throw new InvalidOperationException($"Failed to resolve object of type {typeof(TService)}"); // throw error with detailed description
        }

        return service; // return instance if not null
    }
}