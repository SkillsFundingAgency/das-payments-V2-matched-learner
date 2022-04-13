using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Payments.MatchedLearner.Api.Ioc;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;

namespace SFA.DAS.Payments.MatchedLearner.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration = Configuration.InitialiseConfigure();

            var applicationSettings = services.AddApplicationSettings(Configuration);

            services.AddAppDependencies(applicationSettings);
            services.AddApplicationInsightsTelemetry();
            services.AddHealthChecks();

            services.AddNLog(applicationSettings, "Api");

            if (!Configuration.IsDevelopment())
            {
                var azureAdConfiguration = Configuration
                    .GetSection(ApplicationSettingsKeys.AzureADConfigKey)
                    .Get<AzureActiveDirectoryConfiguration>();

                if (azureAdConfiguration == null)
                {
                    throw new InvalidOperationException("invalid Configuration, unable Add Authentication, unable find 'AzureAd' Configuration section");
                }

                var policies = new Dictionary<string, string>
                {
                    { PolicyNames.Default, RoleNames.Default },
                };

                services.AddAuthentication(azureAdConfiguration, policies);
            }

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddMvc(o =>
            {
                if (!Configuration.IsDevelopment())
                {
                    o.Conventions.Add(new AuthorizeControllerModelConvention(new List<string> { PolicyNames.Default }));
                }
                o.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Matched-Learner-Api", Version = "v1" });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Configuration.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); //NOSONAR
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse,
                });
                endpoints.MapHealthChecks("/ping", new HealthCheckOptions
                {
                    Predicate = _ => false,
                    ResponseWriter = (context, report) =>
                    {
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("");
                    }
                });
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Matched Learner Api V1");
                c.RoutePrefix = string.Empty;
            });
        }
    }
}