using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services) => services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader());
        });

        //Hosting in IIS will require an integration that helps in deployments
        public static void ConfigureIISIntegration(this IServiceCollection services) => services.Configure<IISOptions>(options => 
        {
            //Default values, for properties inside options, are fine for now
        });
    }
}
