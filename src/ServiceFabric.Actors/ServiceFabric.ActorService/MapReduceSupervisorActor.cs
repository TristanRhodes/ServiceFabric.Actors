using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.ActorService
{
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Microsoft.ServiceFabric.Data.Collections;
    using ServiceFabric.ActorService.Interfaces;
    using System.IO;

    [ActorService(Name = "MapReduceSupervisorActorService")]
    [StatePersistence(StatePersistence.None)]
    public class MapReduceSupervisorActor : Actor, IMapReduceSupervisorActor
    {
        private IReliableDictionary<string, int> _persistence;

        // * Recieve file path, stream file content.
        // * Push streamed content to Map actor(s)
        // * Take mapped result and push to reducer
        // * Return final result on completion

        public MapReduceSupervisorActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }


        public async Task Process(string fileName)
        {
            var appName = ActorService.Context.CodePackageActivationContext.ApplicationName;
            var mapActor = ActorFactory.CreateActor<IMapActor>(appName, "MapActorService", "mapper", 1);
            var reduceActor = ActorFactory.CreateActor<IReduceActor>(appName, "ReduceActorService", "reducer", 1);

            using (var stream = File.OpenRead(fileName))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var map = await mapActor.MapAsync(line);
                    await reduceActor.ReduceAsync(map);
                }
            }

            var result = reduceActor.GetResult();

            throw new NotImplementedException();
        }
    }
}
