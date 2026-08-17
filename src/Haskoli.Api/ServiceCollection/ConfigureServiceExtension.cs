using Haskoli.Api.Middleware;
using Haskoli.Application.ServiceCollection;
using Haskoli.Infrastructure.Common.ServiceCollection;
using Haskoli.Infrastructure.Identity;
using Haskoli.Infrastructure.Persistence.ServiceCollection;
using Haskoli.Infrastructure.UnitOfWork.ServiceCollection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Serialization;

namespace Haskoli.Api.ServiceCollection
{
    public static class ConfigureServiceExtension
    {
        public static void InitConfigurationAPI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationLayer();
            services.AddPersistenceLayer(configuration);
            services.AddUnitOfWorkLayer();
            services.AddCommonLayer(configuration);
            services.AddTransient<ErrorHandlerMiddleware>();
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.UseCamelCasing(true);
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.Culture = System.Globalization.CultureInfo.CurrentCulture;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
                options.SerializerSettings.FloatFormatHandling = Newtonsoft.Json.FloatFormatHandling.DefaultValue;
                options.SerializerSettings.FloatParseHandling = Newtonsoft.Json.FloatParseHandling.Decimal;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            /* Puede ser también: services.AddHttpContextAccessor(); */
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Haskoli API",
                    Version = "v1"
                });
            });

            // call Identity services builder
            services.ConfigureIdentityServices(configuration);

            services.AddCors(options =>
            {
                options.AddPolicy("CorsePolicy", builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                );
            });

        }
    }
}
