using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace ServiceFabric.ActorService
{
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Newtonsoft.Json.Linq;
    using ServiceFabric.ActorService.Interfaces;
    using ServiceFabric.ActorService.Interfaces.Model;

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

        public Task<MappedData> MapAsync(string text)
        {
            var jobject = JObject.Parse(text);
            var company = jobject["Company"].Value<string>();

            var mapData = new MappedData()
            {
                CompanyName = company
            };
            return Task.FromResult(mapData);
        }
    }
}
