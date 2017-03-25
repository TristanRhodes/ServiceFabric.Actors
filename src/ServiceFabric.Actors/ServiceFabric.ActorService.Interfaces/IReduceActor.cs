using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.ActorService.Interfaces
{
    public interface IReduceActor : IActor
    {
        Task ReduceAsync(Dictionary<string, string> map);

        Task<dynamic> GetResult();
    }
}
