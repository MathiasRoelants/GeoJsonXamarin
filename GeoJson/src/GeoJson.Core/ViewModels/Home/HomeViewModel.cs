using System.Threading.Tasks;
using GeoJson.Core.Services.Interfaces;
using GeoJSON.Net.Feature;

namespace GeoJson.Core.ViewModels.Home
{
    public class HomeViewModel : BaseViewModel
    {
        private IGeoJsonService _geoJsonService;

        public HomeViewModel(IGeoJsonService geoJsonService)
        {
            _geoJsonService = geoJsonService;
        }

        public FeatureCollection Features { get; private set; }

        public override async Task Initialize()
        {
            await base.Initialize();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            Task.Run(async () => { Features = await _geoJsonService.GetJson(); });
        }
    }
}
