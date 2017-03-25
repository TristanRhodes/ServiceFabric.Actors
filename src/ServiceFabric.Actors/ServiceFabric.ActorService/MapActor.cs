using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.ActorService
{
    using Microsoft.ServiceFabric.Actors.Runtime;
    using ServiceFabric.ActorService.Interfaces;

    [ActorService(Name = "MapActorService")]
    [StatePersistence(StatePersistence.None)]
    public class MapActor : Actor, IMapActor
    {
        // * Recieve record (JSON text)
        // * Perform map operation
        // * Return projection

        public MapActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        public Task<Dictionary<string, string>> MapAsync(string text)
        {
            var result = text
                .Split(' ', ',', ':', '.', ';')
                .GroupBy(k => k)
                .ToDictionary(k => k.Key, v => v.Key);

            var entity = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(text);

            return Task.FromResult(result);
        }
    }
}
