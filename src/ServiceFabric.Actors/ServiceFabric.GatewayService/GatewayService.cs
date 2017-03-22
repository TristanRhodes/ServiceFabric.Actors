using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using ServiceFabric.ActorService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using System.Globalization;

namespace ServiceFabric.GatewayService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class GatewayService : StatelessService
    {
        public GatewayService(StatelessServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext => new OwinCommunicationListener(Startup.ConfigureApp, serviceContext, ServiceEventSource.Current, "ServiceEndpoint"))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var appName = Context.CodePackageActivationContext.ApplicationName;
            var serviceName = "TestActorServiceActorService";
            var actorId = new ActorId(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}-Actor",
                        appName));

            var actorServiceUri = new Uri(appName + "/" + serviceName);

            // fabric:/VisualObjects/VisualObjects.ActorService
            // fabric:/ServiceFabric.Actors/TestActorServiceActorService

            var testProxy = ActorProxy.Create<ITestActorService>(actorId, actorServiceUri);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var count = await testProxy.GetCountAsync(cancellationToken);
                    await testProxy.SetCountAsync(count++, cancellationToken);
                    Console.WriteLine("Count: " + count);
                }
                catch (Exception ex)
                {
                    // Ignore Exceptions
                    Console.WriteLine("Exception: " + ex);
                }
                finally
                {
                    // Do something?
                }

                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
            }


            //await base.RunAsync(cancellationToken);
        }
    }
}
