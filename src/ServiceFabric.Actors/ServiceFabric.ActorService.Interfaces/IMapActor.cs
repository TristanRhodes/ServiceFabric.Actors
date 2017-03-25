﻿using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.ActorService.Interfaces
{
    public interface IMapActor : IActor
    {
        Task<Dictionary<string, string>> MapAsync(string text);
    }
}
