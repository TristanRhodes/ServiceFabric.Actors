using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.ActorService.Interfaces
{
    public class ActorFactory
    {
        public static IEnumerable<T> CreateActors<T>(string appName, string serviceName, string actorType, int count) where T : IActor
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateActor<T>(appName, serviceName, actorType, i);
            }
        }

        public static T CreateActor<T>(string appName, string serviceName, string actorType, int ordinal) where T : IActor
        {
            var actorId = GenerateActorId(appName, actorType, ordinal);
            var actorServiceUri = new Uri(appName + "/" + serviceName);

            //var message = string.Format("Creating Actor: Id: {0}, Service Uri: {1}", actorId, actorServiceUri);
            //ServiceEventSource.Current.Message(message);

            return ActorProxy.Create<T>(actorId, actorServiceUri);
        }

        public static ActorId GenerateActorId(string appName, string actorType, int ordinal)
        {
            var actorIdString = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}-{1}-{2}",
                        appName,
                        actorType,
                        ordinal);

            return new ActorId(actorIdString);
        }
    }
}
