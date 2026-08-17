using Microsoft.Extensions.Logging;

namespace Haskoli.Domain.Exceptions.Api
{
    public class ExternalApiException : Exception
    {
        public int Code { get; set; }
        public string Details { get; set; }
        public LogLevel LogLevel { get; set; }

        public ExternalApiException(int code, string message = null, string details = null,
                                 Exception innerException = null, LogLevel logLevel = LogLevel.Warning)
            : base(message, innerException) { Code = Convert.ToInt32(code); Details = details; LogLevel = logLevel; }

        public ExternalApiException(string message, Exception innerException = null)
            : base(message, innerException) { }
        public ExternalApiException WithData(string name, object value) { Data[name] = value; return this; }

    }

}
