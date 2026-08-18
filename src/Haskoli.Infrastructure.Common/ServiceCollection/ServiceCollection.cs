using Haskoli.Application.Contracts.ExternalServices;
using Haskoli.Application.Models;
using Haskoli.Domain.Interfaces.Management;
using Haskoli.Domain.Interfaces.Repository;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Infrastructure.Common.Helpers;
using Haskoli.Infrastructure.Common.Repositories;
using Haskoli.Infrastructure.Common.Services;
using Haskoli.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haskoli.Infrastructure.Common.ServiceCollection
{
    public static class ServiceCollection
    {
        public static void AddCommonLayer(this IServiceCollection services, IConfiguration configuration)
        {
            /* Repositories */
            services.AddTransient<ICountryRepository<HaskoliDbContext>, CountryRepository>();
            services.AddTransient<IStudentRepository<HaskoliDbContext>, StudentRepository>();

            /* Services */
            services.AddTransient<ICountryService, CountryService>();
            services.AddTransient<IStudentService, StudentService>();

            /* Helpers */
            services.AddSingleton<IUriService>(o =>
            {
                var accessor = o.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var uri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
                return new UriService(uri);
            });
            services.AddScoped(typeof(IDataShapeHelper<>), typeof(DataShapeHelper<>));
            services.AddScoped<IModelHelper, ModelHelper>();

            var configEmail = configuration.GetSection("EmailSettings").Get<EmailSettings>();
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailService, EmailService>();
        }
    }
}
