using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DataExchangeSolution
{
    [ServiceContract]
    public interface IDataSimulation
    {
        [OperationContract]
        Service.SimulationResult simulate(Service.SimulationInfo info);
    }
}
