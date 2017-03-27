using Microsoft.ServiceFabric.Actors;
using ServiceFabric.ActorService.Interfaces.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.ActorService.Interfaces
{
    public interface IReduceActor : IActor
    {
        Task ReduceAsync(MappedData map);

        Task<ReducedData> GetResultAsync();
    }
}
