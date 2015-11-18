using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;

namespace DataExchangeSolution
{
    public class DataSimulation : IDataSimulation
    {
        public enum SimulationType
        {
            Energetic = 1 
        };

        public struct SimulationInfo
        {
            public SimulationType Type;
            public byte[] Data;
            public byte[] WeatherData;
        }

        public struct SimulationResult
        {
            public byte[] Data;
            public bool Succeeded;
        }

        public SimulationResult simulate(SimulationInfo info)
        {
            return new SimulationResult { };
        }
    }
}
