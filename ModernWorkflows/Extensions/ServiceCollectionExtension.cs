using Microsoft.Extensions.DependencyInjection;
using ModernWorkflows.Interfaces;
using ModernWorkflows.Models;
using ModernWorkflows.Primitives;
using ModernWorkflows.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Extensions;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Add base workflow, workflowDSL and updated configurations
    /// </summary>
    /// <param name="services">Service collection from DI</param>
    /// <param name="setupAction">Setup action for standard AddWorkflow</param>
    /// <returns>Service collection from DI with updated workflows</returns>
    public static IServiceCollection AddModernWorkflows(
        this IServiceCollection services,
        Action<WorkflowOptions>? setupAction = null
    )
    {
        services.AddWorkflow(setupAction);
        services.AddWorkflowDSL();
        
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