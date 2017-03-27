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
    using ServiceFabric.ActorService.Interfaces.Model;
    using System.IO;

    [ActorService(Name = "MapReduceSupervisorActorService")]
    [StatePersistence(StatePersistence.None)]
    public class MapReduceSupervisorActor : Actor, IMapReduceSupervisorActor
    {
        // * Recieve file path, stream file content.
        // * Push streamed content to Map actor(s)
        // * Take mapped result and push to reducer
        // * Return final result on completion

        public MapReduceSupervisorActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }


        public async Task<ReducedData> Process(string fileName)
        {
            var appName = ActorService.Context.CodePackageActivationContext.ApplicationName;
            var mapActor = ActorFactory.CreateActor<IMapActor>(appName, "MapActorService", "mapper", 1);
            var reduceActor = ActorFactory.CreateActor<IReduceActor>(appName, "ReduceActorService", "reducer", 1);

            using (var stream = File.OpenRead(fileName))
            {
                var lines = ReadLines(stream);

                foreach (var line in lines)
                {
                    var map = await mapActor.MapAsync(line);
                    await reduceActor.ReduceAsync(map);
                }
            }

            return await reduceActor.GetResultAsync();
        }

        private IEnumerable<string> ReadLines(Stream fileStream)
        {
            using (StreamReader reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream)
                    yield return reader.ReadLine();
            }
        }
    }
}
