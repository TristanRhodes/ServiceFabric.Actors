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
            //var appName = Context.CodePackageActivationContext.ApplicationName;
            //var serviceName = "TestActorService";

            //var actorProxies = CreateActors<ITestActorService>(appName, serviceName, 10);
            //var tasks = RunActors(actorProxies, cancellationToken);

            var appName = Context.CodePackageActivationContext.ApplicationName;
            var serviceName = "MapReduceSupervisorActorService";
            var file = "D:\\Temp\\stocks.json";
            var actorProxy = ActorFactory.CreateActor<IMapReduceSupervisorActor>(appName, serviceName, "supervisor", 1);
            var reducedData = await actorProxy.Process(file);
        }


        private static IEnumerable<Task> RunActors(IEnumerable<ITestActorService> actorProxies, CancellationToken cancellationToken)
        {
            foreach(var actor in actorProxies)
            {
                yield return RunActor(actor, cancellationToken);
            }
        }

        private static async Task RunActor(ITestActorService actorProxy, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var count = await actorProxy.GetCountAsync(cancellationToken);
                    await actorProxy.SetCountAsync(++count, cancellationToken);
                    ServiceEventSource.Current.Message("Count: " + count);

                    if (count > 100)
                        break;
                }
                catch (Exception ex)
                {
                    // Must be a better way to log exceptions... Insights?
                    ServiceEventSource.Current.Message("EXCEPTION: " + ex);
                }
                finally
                {
                    // Cleanup
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }
        }
    }
}
