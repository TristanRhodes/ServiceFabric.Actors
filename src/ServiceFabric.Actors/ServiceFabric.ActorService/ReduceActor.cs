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

        public async Task ReduceAsync(Dictionary<string, string> map)
        {
            foreach (var kvp in map)
            {
                var value = await StateManager.GetOrAddStateAsync(kvp.Key, 0);
                await StateManager.SetStateAsync(kvp.Key, ++value);
            }
        }

        public async Task<dynamic> GetResult()
        {
            var stateNames = await StateManager.GetStateNamesAsync();

            var dictionary = new Dictionary<string, int>();

            foreach (var name in stateNames)
            {
                var value = await StateManager.GetStateAsync<int>(name);
                dictionary[name] = value;
            }

            return dictionary;
        }
    }
}
