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
    using Microsoft.ServiceFabric.Data.Collections;
    using ServiceFabric.ActorService.Interfaces;
    using ServiceFabric.ActorService.Interfaces.Model;

    [ActorService(Name = "ReduceActorService")]
    [StatePersistence(StatePersistence.Persisted)]
    public class ReduceActor : Actor, IReduceActor
    {
        // * Recieve mapped record (JSON text)
        // * Aggregate values

        public ReduceActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        public async Task ReduceAsync(MappedData map)
        {
            var value = await StateManager.GetOrAddStateAsync(map.CompanyName, 0);
            await StateManager.SetStateAsync(map.CompanyName, ++value);
        }

        public async Task<ReducedData> GetResultAsync()
        {
            var stateNames = await StateManager.GetStateNamesAsync();

            var dictionary = new Dictionary<string, int>();

            foreach (var name in stateNames)
            {
                var value = await StateManager.GetStateAsync<int>(name);
                dictionary[name] = value;
            }

            return new ReducedData()
            {
                CompaniesCount = dictionary
            };
        }
    }
}
