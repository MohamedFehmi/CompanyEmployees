using Contracts;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services) => 
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader());
            });

        //Hosting in IIS will require an integration that helps in deployments
        public static void ConfigureIISIntegration(this IServiceCollection services) => 
            services.Configure<IISOptions>(options => 
            {
                //Default values, for properties inside options, are fine for now
            });

        public static void ConfigureLoggerService(this IServiceCollection services) => 
            services.AddScoped<ILoggerManager, LoggerManager>();

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDbContext<RepositoryContext>(opts =>
                opts.UseSqlServer(configuration.GetConnectionString("sqlConnection"), mig => 
                    mig.MigrationsAssembly("CompanyEmployees")));

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();
                if(newtonsoftJsonOutputFormatter != null)
                {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.compemployees.hateoas+json");
                }

                var xmlOutputFormatter = config.OutputFormatters.OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();
                if(xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.compemployees.hateoas+xml");
                }
            });
        }
    }
}
