using System;
using System.ServiceModel;
using System.Threading;


namespace DataExchangeSolution
{

    class Program
    {

        public static Object designBuilderLock = new Object();
        public static Object revitLock = new Object();
        public static Logger logger = new Logger("DataExchangeSolution");
        static void Main(string[] args)
        {
            try
            {
                ServiceHost dataExchangeService = null;
                Uri httpBaseAddress = new Uri(args.Length == 0 ? "http://127.0.0.1:8040" : args[0]);
 
                dataExchangeService = new ServiceHost(typeof(Service), httpBaseAddress);
                WSHttpBinding binding = new WSHttpBinding();
                binding.Security = new WSHttpSecurity { Mode = SecurityMode.None };

                binding.Security.Transport = new HttpTransportSecurity { ClientCredentialType = HttpClientCredentialType.None };
                binding.MaxBufferPoolSize = 1073741824;
                binding.MaxReceivedMessageSize = 1073741824;
                binding.ReceiveTimeout = TimeSpan.FromHours(1);
                binding.SendTimeout = TimeSpan.FromHours(1);
                binding.ReaderQuotas.MaxArrayLength = 1073741824;
                binding.ReaderQuotas.MaxBytesPerRead = 1073741824;
                binding.ReaderQuotas.MaxDepth = 1073741824;
                binding.ReaderQuotas.MaxStringContentLength = 1073741824;

                dataExchangeService.AddServiceEndpoint(typeof(IDataTranslation), binding, "");
                dataExchangeService.AddServiceEndpoint(typeof(IDataSimulation), binding, "");
                dataExchangeService.Open();
                logger.log("Service is now at: " + httpBaseAddress);
                while (true)
                {
                    Thread.Sleep(10000);
                }
            }
            catch (Exception ex)
            {
                logger.log( ex.Message, Logger.LogType.ERROR);
            }


        }
    }
}
