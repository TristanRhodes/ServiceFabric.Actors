using Microsoft.ServiceFabric.Actors;
using ServiceFabric.ActorService.Interfaces.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.ActorService.Interfaces
{
    public interface IMapActor : IActor
    {
        Task<MappedData> MapAsync(string text);
    }
}
