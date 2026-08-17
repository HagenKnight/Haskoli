using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Haskoli.Infrastructure.Common.Helpers
{
    public static class JsonHelper
    {
        public static JsonSerializerSettings JsonSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}
