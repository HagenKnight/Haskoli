using Haskoli.Application.Behaviors;
using Haskoli.Application.Mappings;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Haskoli.Application.ServiceCollection
{
    public static class ServiceExtension
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
            /* AutoMapper resuelve los conversores del contenedor, y no registra por sí solo los
               genéricos abiertos: sin esto, mapear PagedList a MetaData falla en tiempo de
               ejecución. */
            services.AddTransient(typeof(ConverterPaging<,>));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        }
    }
}
