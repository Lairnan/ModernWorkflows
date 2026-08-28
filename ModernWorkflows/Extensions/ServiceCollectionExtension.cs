using Microsoft.Extensions.DependencyInjection;
using ModernWorkflows.Interfaces;
using ModernWorkflows.Models;
using ModernWorkflows.Primitives;
using ModernWorkflows.Steps;
using WorkflowCore.Interface;

namespace ModernWorkflows.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddModernWorkflows(this IServiceCollection services)
    {
        services.AddTransient<StartWorkflowStep>();
        services.AddTransient<InitializeWorkflowStep>();
        services.AddTransient<ShowMessageStep>();
        services.AddTransient<InputValueStep>();
        services.AddTransient<ValidateInputStep>();
        services.AddTransient<WaitEvent>();
        
        services.AddSingleton<IWorkflowHostEx, WorkflowHostEx>();
        services.AddSingleton<IWorkflowHost>(provider => provider.GetRequiredService<IWorkflowHostEx>());
        services.AddSingleton<IWorkflowEventPublisher, WorkflowEventPublisher>();
        return services;
    }
}