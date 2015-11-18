using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DataExchangeSolution
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDataTranslation" in both code and config file together.
    [ServiceContract]
    public interface IDataTranslation
    {
        [OperationContract]
        Service.TranslationResult translate(Service.TranslationInfo info);
    }
}
