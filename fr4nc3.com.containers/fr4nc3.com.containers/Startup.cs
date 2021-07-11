using fr4nc3.com.containers.CustomSettings;
using fr4nc3.com.containers.Middleware;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace fr4nc3.com.containers
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Enable to allow manual handling of model binding errors
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });



            // Setup swagger 
            SetupSwaggerDocuments(services);

            //  Setup custom settings
            SetupCustomSettings(services);

            //  Enable multi-stream read
            services.AddTransient<EnableMultipleStreamReadMiddleware>();

            // Setup the application insights integration
            SetupApplicationInsights(services);
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);

        }

        /// <summary>
        /// Setup the application insights integration
        /// </summary>
        /// <param name="services">The services collection</param>
        private void SetupApplicationInsights(IServiceCollection services)
        {
            // Access settings
            ApplicationInsights applicationInsightsSettings = Configuration.GetSection("ApplicationInsights").Get<ApplicationInsights>();

            // Setup app insights
            services.AddApplicationInsightsTelemetry(applicationInsightsSettings.InstrumentationKey);

            // Setup live monitering key so authentication is enabled allowing filtering of events
            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, _) => module.AuthenticationApiKey = applicationInsightsSettings.AuthenticationApiKey);
            // DEMO: Setup telemetry client
            services.AddSingleton(typeof(TelemetryClient));
            // DEMO: Setup snapshot debugging
            services.AddSnapshotCollector((configuration) => Configuration.Bind(nameof(SnapshotCollectorConfiguration), configuration));
        }

        /// <summary>
        /// Sets up custom, strongly typed settings
        /// </summary>
        /// <param name="services">The service colleciton</param>
        private void SetupCustomSettings(IServiceCollection services)
        {

            services.AddOptions();

            // Add the class that represents the settings for the ApplicationInsights section
            // in the JSON settings
            services.Configure<ApplicationInsights>(Configuration.GetSection(nameof(ApplicationInsights)));

            // Support Generic IConfiguration access for generic string access
            services.AddSingleton(Configuration);
        }
        /// <summary>
        /// Sets up the swagger documents
        /// </summary>
        /// <param name="services">The service collection</param>
        private static void SetupSwaggerDocuments(IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = " Containers API",
                    Version = "v1",
                    Description = "Francia Riesco  COnstainers API  ",
                });

                // Use method name as operationId so that ADD REST Client... will work
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            SetupSwaggerJsonGeneratgionAndUI(app);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable multi-stream read
            app.UseMultipleStreamReadMiddleware(); // not in use in this (no body payload model passed to validate) 

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        /// <summary>
        /// Sets up the Swagger JSON file and Swagger Interactive UI
        /// </summary>
        /// <param name="app">The application builder</param>
        private static void SetupSwaggerJsonGeneratgionAndUI(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                // Use the older 2.0 format so the ADD REST Client... will work
                c.SerializeAsV2 = true;
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            //       specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Containers  V1 API");

                // Serve the Swagger UI at the app's root (http://localhost:<port>)
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
