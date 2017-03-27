using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.ActorService.Interfaces.Model
{
    public class ReducedData
    {
        public Dictionary<string, int> CompaniesCount { get; set; }

        public int ProcessedRecordsCounter { get; set; }
    }
}
