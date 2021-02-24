using System;
using System.Threading.Tasks;
using GeoJSON.Net.Feature;

namespace GeoJson.Core.Services.Interfaces
{
    public interface IGeoJsonService
    {
        Task<FeatureCollection> GetJson();
    }
}
