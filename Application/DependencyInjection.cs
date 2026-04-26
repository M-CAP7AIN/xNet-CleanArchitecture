using Application.Behaviors;
using Application.Common.Mappings;
using Application.Notes.Commands.CreateNote;
using FluentValidation;
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
            services.AddMediatR(cfg => {
                // Register all handlers from this assembly
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                // Add behaviors (Order matters - from outer to inner)
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
                //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
                //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            });

            // Add AutoMapper - راه حل صحیح
            services.AddAutoMapper(cfg => {
                cfg.AllowNullCollections = true;
            }, typeof(MappingProfile).Assembly);

            // Add FluentValidation
            services.AddValidatorsFromAssembly(typeof(CreateNoteCommandValidator).Assembly);


            return services;
        }
    }
}

