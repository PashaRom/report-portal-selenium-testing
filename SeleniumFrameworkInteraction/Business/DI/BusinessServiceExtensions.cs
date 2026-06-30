using Business.Clients;
using Business.Components;
using Business.Services;
using Business.Pages;
using Business.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Business.DI;

public static class BusinessServiceExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // API clients
        services.AddSingleton<IAuthClient, AuthClient>();
        services.AddSingleton<IDashboardApiClient, DashboardApiClient>();
        services.AddSingleton<IUserApiClient, UserApiClient>();
        services.AddSingleton<DashboardCleanupApiService>();
        services.AddSingleton<UserProvisioningService>();

        // Components — transient: new instance per resolution so each test gets a fresh state
        services.AddTransient<AddDashboardDialog>();
        services.AddTransient<AddWidgetDialog>();
        services.AddTransient<DeleteDashboardDialog>();
        services.AddTransient<SystemAlertDialog>();
        services.AddTransient<WidgetComponent>();

        // Pages — transient
        services.AddTransient<LoginPage>();
        services.AddTransient<DashboardListPage>();
        services.AddTransient<DashboardPage>();

        // Steps — transient
        services.AddTransient<AuthSteps>();
        services.AddTransient<DashboardSteps>();

        return services;
    }
}
