using System;
using GeoJSON.Net.Feature;

namespace GeoJson.Core.Services.Interfaces
{
    public interface IGeoJsonService
    {
        FeatureCollection GetJson();
    }
}
