using Mango.Services.EmailAPI.Messaging;
using System.Reflection.Metadata;

namespace Mango.Services.EmailAPI.Extension
{
    public static class ApplicationBuilderExtensions
    {
        private static IAzureServiceBusConsumer ServiceBusConsumer { get; set; }
        public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
        {
            ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
            var hostApplicationLifeTime = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            hostApplicationLifeTime.ApplicationStarted.Register(OnStart);
            hostApplicationLifeTime.ApplicationStopping.Register(OnStop);

            return app;
        }

        private static void OnStart()
        {
            ServiceBusConsumer.Start();
        }
        private static void OnStop()
        {
            ServiceBusConsumer.Stop();
        }
    }
}
