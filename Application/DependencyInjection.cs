using Application.Behaviors;
using Application.Common.Mappings;
using Application.Controller.Notes.Commands.CreateNote;
using Domain.Interfaces;
using FluentValidation;
using Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register MediatR with all handlers and behaviors
            services.AddMediatR(cfg =>
            {
                // Register all handlers from this assembly
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                // Add behaviors (Order matters - from outer to inner)
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
                //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
                //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
                //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            });

            // Add AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });

            // Add FluentValidation
            services.AddValidatorsFromAssembly(typeof(CreateNoteCommandValidator).Assembly);

            return services;
        }
    }
}

