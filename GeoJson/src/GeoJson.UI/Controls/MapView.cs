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
        }

        public FeatureCollection Features
        {
            get => (FeatureCollection)GetValue(FeaturesProperty);
            set => SetValue(FeaturesProperty, value);
        }
    }
}
