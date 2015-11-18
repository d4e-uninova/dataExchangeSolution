using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace DataExchangeSolution
{

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DataTranslation" in both code and config file together.
    public partial class Service : IDataTranslation
    {

        private gbXML2IDFConverter.gbXML2IDF gbXML2idf = new gbXML2IDFConverter.gbXML2IDF(@"C:\Program Files (x86)\DesignBuilder");
        private ifc2gbXMLConverter.ifc2gbXML ifc2gbXML = new ifc2gbXMLConverter.ifc2gbXML(@"C:\Program Files\Autodesk\Revit 2015");
        private ifc2RVTConverter.ifc2RVT ifc2rvt = new ifc2RVTConverter.ifc2RVT(@"C:\Program Files\Autodesk\Revit 2015");
        private rvt2IFCConverter.rvt2IFC rvt2ifc = new rvt2IFCConverter.rvt2IFC(@"C:\Program Files\Autodesk\Revit 2015");
        private rvt2gbXMLConverter.rvt2gbXML rvt2gbXML = new rvt2gbXMLConverter.rvt2gbXML(@"C:\Program Files\Autodesk\Revit 2015");

        public enum DataFormat
        {
            IFC = 1,
            gbXML = 2,
            RVT = 3,
            IDF = 4
        };



        public struct TranslationInfo
        {
            public DataFormat InputFormat;
            public DataFormat OutputFormat;
            public byte[] Data;
        }

        public struct TranslationResult
        {
            public byte[] Data;
            public bool Succeeded;

        }

        public bool convert(DataFormat inFormat, DataFormat outFormat, string inFile, string outFile)
        {

            bool result;
            if (inFormat == DataFormat.IFC && outFormat == DataFormat.gbXML)
            {
                lock (Program.revitLock)
                {
                    result = ifc2gbXML.Convert(inFile, outFile);
                }
                return result;
            }
            if (inFormat == DataFormat.gbXML && outFormat == DataFormat.IDF)
            {
                lock (Program.designBuilderLock)
                {
                    result = gbXML2idf.Convert(inFile, outFile);
                }
                return result;
            }
            if (inFormat == DataFormat.IFC && outFormat == DataFormat.RVT)
            {
                lock (Program.revitLock)
                {
                    result = ifc2rvt.Convert(inFile, outFile);
                }
                return result;
            }
            if(inFormat== DataFormat.RVT && outFormat == DataFormat.IFC)
            {
                lock(Program.revitLock)
                {
                    result = rvt2ifc.Convert(inFile, outFile);
                }
                return result;
            }
            if(inFormat == DataFormat.RVT && outFormat == DataFormat.gbXML)
            {
                lock(Program.revitLock)
                {
                    result = rvt2gbXML.Convert(inFile, outFile);
                }
                return result;
            }
            return false;
        }
        public string getExtension(DataFormat dataFormat)
        {
            switch (dataFormat)
            {
                case DataFormat.IFC:
                    return ".ifc";
                case DataFormat.gbXML:
                    return ".xml";
                case DataFormat.RVT:
                    return ".rvt";
                case DataFormat.IDF:
                    return ".idf";
                default:
                    return "";
            }
        }



        public TranslationResult translate(TranslationInfo info)
        {
            OperationContext context = OperationContext.Current;
            MessageProperties messageProperties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty enpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            
            GraphContainer graphContainer = new GraphContainer();
            if (info.InputFormat != 0 && info.OutputFormat != 0 && info.InputFormat != info.OutputFormat)
            {
                //(@"C:\Program Files (x86)\DesignBuilder");

                string appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\DataExchangeSolution\";
                if (!Directory.Exists(appPath))
                {
                    Directory.CreateDirectory(appPath);
                }
                string guid = Guid.NewGuid().ToString();
                List<string> files = new List<string>();
                List<DataFormat> formats = new List<DataFormat>();
                List<DataFormat> path;
                byte[] outFile = new byte[0];
                bool result = false;
                path = graphContainer.FindPath(info.InputFormat, info.OutputFormat);

                if (path.Count() > 0)
                {
                    files.Add(appPath + guid + getExtension(info.InputFormat));
                    formats.Add(info.InputFormat);
                    File.WriteAllBytes(files.ElementAt(0), info.Data);
                    for (var i = 0; i < path.Count(); i++)
                    {
                        files.Add(appPath + guid + getExtension(path[i]));
                        formats.Add(path[i]);
                    }
                    Program.logger.log("Starting conversion from " + formats.First() + " to " + formats.Last() + " requested by: " + enpointProperty.Address);
                    for (var i = 0; i < path.Count(); i++)
                    {
                        try
                        {
                            result = convert(formats.ElementAt(i), formats.ElementAt(i + 1), files.ElementAt(i), files.ElementAt(i + 1));
                        }
                        catch (Exception e)
                        {
                            Program.logger.log(e.Message, Logger.LogType.ERROR);
                        }

                        if (!result)
                        {
                            break;
                        }
                    }
                    if (result)
                    {
                        Program.logger.log("Conversion requested by: " + enpointProperty.Address + " succeeded");
                        outFile = File.ReadAllBytes(files.Last());
                    }
                    else
                    {
                        Program.logger.log("Conversion requested by: " + enpointProperty.Address + " failed",Logger.LogType.ERROR);
                    }
                    for (var i = 0; i < files.Count(); i++)
                    {
                        if (File.Exists(files.ElementAt(i)))
                        {
                            File.Delete(files.ElementAt(i));
                        }
                    }
                    if (result)
                    {
                        return new TranslationResult
                        {
                            Data = outFile,
                            Succeeded = true
                        };
                    }
                }

            }



            return new TranslationResult();
        }
    }

    public partial class Service : IDataSimulation
    {
        private energyAnalysis.Energy energy = new energyAnalysis.Energy(@"C:\EnergyPlusV8-4-0");

        public enum SimulationType
        {
            Energetic = 1
        };

        public struct SimulationInfo
        {
            public SimulationType Type;
            public DataFormat ModelFormat;
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
            
            OperationContext context = OperationContext.Current;
            MessageProperties messageProperties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty enpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            SimulationResult result = new SimulationResult
            {
                Data=null,
                Succeeded=false
            };

            TranslationResult transResult = new TranslationResult{
                Data=null,
                Succeeded=false
            };

            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\DataExchangeSolution\";
            string guid = Guid.NewGuid().ToString();
            Program.logger.log("Starting " + info.Type + " simulation requested by: " + enpointProperty.Address);

            if(info.Type==SimulationType.Energetic)
            {
                if(info.ModelFormat!=DataFormat.IDF)
                {
                    transResult = translate(new TranslationInfo
                    {
                        Data = info.Data,
                        InputFormat = info.ModelFormat,
                        OutputFormat = DataFormat.IDF
                    });
                }
                if (info.ModelFormat == DataFormat.IDF || transResult.Succeeded)
                {
                    string idfFile = appPath + guid + ".idf";
                    string epwFile = appPath + guid + ".epw";
                    string outFile = appPath + guid + ".zip";
                    File.WriteAllBytes(idfFile,info.ModelFormat == DataFormat.IDF? info.Data:transResult.Data);
                    File.WriteAllBytes(epwFile, info.WeatherData);
                    if (energy.Simulate(idfFile, epwFile, outFile))
                    {
                        Program.logger.log("Simulation requested by: " + enpointProperty.Address + " succeeded");
                        result = new SimulationResult
                        {
                            Data = File.ReadAllBytes(outFile),
                            Succeeded = true
                        };

                    }
                    if (File.Exists(idfFile))
                    {
                        File.Delete(idfFile);
                    }
                    if (File.Exists(epwFile))
                    {
                        File.Delete(epwFile);
                    }
                    if (File.Exists(outFile))
                    {
                        File.Delete(outFile);
                    }
                }
            }
            if(!result.Succeeded)
            {
                Program.logger.log("Simulation requested by: " + enpointProperty.Address + " failed",Logger.LogType.ERROR);  
            }
            return result;
        }
    }
}
