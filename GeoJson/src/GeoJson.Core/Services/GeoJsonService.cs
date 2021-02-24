using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GeoJson.Core.Services.Interfaces;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;

namespace GeoJson.Core.Services
{
    public class GeoJsonService : IGeoJsonService
    {
        public GeoJsonService()
        {
        }

        public async Task<FeatureCollection> GetJson()
        {
            Assembly assembly = typeof(GeoJsonService).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream(Constants.App.JSON_RESOURCE_NAME);
            using (var sr = new StreamReader(stream))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    //hack
                    await Task.Delay(500);
                    return serializer.Deserialize<FeatureCollection>(reader);
                }
            }
        }
    }
}
