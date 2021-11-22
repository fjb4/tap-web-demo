using Steeltoe.Extensions.Logging;

namespace Tanzu.Common
{
    public class NullLogProcessor : IDynamicMessageProcessor
    {
        public string Process(string inputLogMessage) => inputLogMessage;
    }
}