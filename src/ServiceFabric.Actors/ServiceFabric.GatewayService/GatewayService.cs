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
using System.Diagnostics;

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

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            var task = AsyncSolution(cancellationToken);

            // TODO: Perform Synchronously.

            return task;
        }

        private async Task AsyncSolution(CancellationToken cancellationToken)
        {
            var appName = Context.CodePackageActivationContext.ApplicationName;
            var serviceName = "MapReduceSupervisorActorService";
            var file = "D:\\Temp\\stocks.json";
            var actorProxy = ActorFactory.CreateActor<IMapReduceSupervisorActor>(appName, serviceName, "supervisor", 1);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var reducedData = await actorProxy.Process(file);
            stopWatch.Stop();

            ServiceEventSource.Current.Message("ACTOR SOLUTION: Total Processing Time: {0}, Total Records: {1}, Per Record Processing Time: {2:0.00}ms", stopWatch.Elapsed, reducedData.ProcessedRecordsCounter, (double)stopWatch.Elapsed.TotalMilliseconds / (double)reducedData.ProcessedRecordsCounter);
        }
    }
}
