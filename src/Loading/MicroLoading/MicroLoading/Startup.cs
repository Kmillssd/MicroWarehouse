using EventBusCore;
using MicroLoading.DataAccess;
using MicroLoading.Integration;
using MicroLoading.Integration.Callbacks;
using MicroLoading.Integration.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MicroLoading
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
            services.AddLogging();

            // DataAccess
            services.AddSingleton<InMemoryLoadingContext>();

            // Event bus
            switch (Configuration["EventBus"])
            {
                case "RabbitMQ":
                    services.AddSingleton<IEventBus, EventBusRabbitMQ.EventBusRabbitMQ>();
                    break;
                case "AWSSQS":
                    services.AddSingleton<IEventBus, EventBusAWSSQS.EventBusAWSSQS>();
                    break;
                default:
                    throw new NotImplementedException("event bus does not exist");
            }

            // Integration
            services.AddScoped<ILoadingIntegrationService, LoadingIntegrationService>();
            services.AddScoped<UserCreatedEventCallBack>();
            services.AddScoped<UserUpdatedEventCallBack>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Event bus subscribe
            var eventBus = app.ApplicationServices.GetService<IEventBus>();
            eventBus.Subscribe<UserCreatedEvent, UserCreatedEventCallBack>();
            eventBus.Subscribe<UserUpdatedEvent, UserUpdatedEventCallBack>();
        }
    }
}
