using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beltzac.AIPlay.App.Api.Contract
{
    public class StatusUpdate
    {
        public Guid ProcessId { get; set; }
        public double PercentageProcessed { get; set; }
        public string Message { get; set; }
    }
}
