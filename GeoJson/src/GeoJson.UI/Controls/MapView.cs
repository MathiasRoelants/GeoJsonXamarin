using System;
using GeoJSON.Net.Feature;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace GeoJson.UI.Controls
{
    public class MapView : Map
    {
        public static readonly BindableProperty FeaturesProperty = BindableProperty.Create(nameof(Features), typeof(FeatureCollection), typeof(MapView), null);

        public MapView(MapSpan region) : base(region)
        {
        }

        public MapView()
        {
            IsShowingUser = true;
            MoveToRegion(MapSpan.FromCenterAndRadius(new Position(51.2496206, 4.3570323), Distance.FromKilometers(7.5)));
        }

        public FeatureCollection Features
        {
            get => (FeatureCollection)GetValue(FeaturesProperty);
            set => SetValue(FeaturesProperty, value);
        }
    }
}
