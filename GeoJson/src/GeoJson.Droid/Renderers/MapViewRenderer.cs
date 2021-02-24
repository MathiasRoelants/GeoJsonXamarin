using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using GeoJson.Droid.Extensions;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Platform.Android;
using APolygon = Android.Gms.Maps.Model.Polygon;
using GPolygon = GeoJSON.Net.Geometry.Polygon;

[assembly: ExportRenderer(typeof(GeoJson.UI.Controls.MapView), typeof(GeoJson.Droid.Renderers.MapViewRenderer))]
namespace GeoJson.Droid.Renderers
{
    public class MapViewRenderer : Xamarin.Forms.Maps.Android.MapRenderer
    {
        private Dictionary<GPolygon, APolygon> _polygons = new Dictionary<GPolygon, APolygon>();
        private bool _mapReady;
        private bool _polygonsQueued;

        public UI.Controls.MapView FormsMapView { get; private set; }

        public MapViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            if (e.OldElement != null && NativeMap != null)
            {
                NativeMap.PolygonClick -= OnPolygonClick;
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            switch (e.PropertyName)
            {
                case nameof(UI.Controls.MapView.Features):
                    OnUpdateFeatures();
                    break;
            }
        }

        private void OnUpdateFeatures()
        {
            _polygons.Clear();
            var mapView = (UI.Controls.MapView)Element;
            if (mapView == null || NativeMap == null || !_mapReady)
            {
                _polygonsQueued = true;
                return;
            }

            _polygonsQueued = false;

            FormsMapView = mapView;

            NativeMap.Clear();

            List<Feature> features = mapView?.Features?.Features;
            if (features == null || features.Count() == 0)
            {
                return;
            }

            foreach (Feature feature in mapView.Features.Features)
            {
                CreatePolygonAndReturnCoordinatesFor(feature);
            }
        }

        private void CreatePolygonAndReturnCoordinatesFor(Feature feature)
        {
            switch (feature.Geometry.Type)
            {
                case GeoJSONObjectType.MultiPolygon:
                    {
                        var multiPolygonGeometry = feature.Geometry as MultiPolygon;
                        foreach (GPolygon polygonGeometry in multiPolygonGeometry.Coordinates)
                        {
                            AddPolygonGeometryToMap(polygonGeometry);
                        }

                        break;
                    }

                case GeoJSONObjectType.Polygon:
                    {
                        var polygonGeometry = feature.Geometry as GPolygon;
                        AddPolygonGeometryToMap(polygonGeometry);
                        break;
                    }
            }
        }

        private void AddPolygonGeometryToMap(GPolygon polygonGeometry)
        {
            PolygonOptions polygon = GeoJsonPolygonToPolygon(polygonGeometry);
            if (polygon != null)
            {
                try
                {
                    APolygon aPolygon = NativeMap.AddPolygon(polygon);
                    _polygons.Add(polygonGeometry, aPolygon);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to add polygon to map or dictionary");
                }
            }
        }

        private PolygonOptions GeoJsonPolygonToPolygon(GPolygon geoJsonPolygon)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<LineString> coords = geoJsonPolygon?.Coordinates;
            if (coords == null || coords.Count() == 0)
            {
                return null;
            }

            PolygonOptions polygonOptions = GetPolygonOptions();

            LineString outer = coords.FirstOrDefault();
            IEnumerable<LineString> inner = coords.Count > 1 ? coords.Skip(1) : null;

            foreach (IPosition coordinate in outer.Coordinates)
            {
                polygonOptions.Add(new LatLng(coordinate.Latitude, coordinate.Longitude));
            }

            if (inner != null)
            {
                foreach (LineString linestring in inner)
                {
                    var holes = linestring.Coordinates.Select(coordinate => new LatLng(coordinate.Latitude, coordinate.Longitude)).ToList();
                    polygonOptions.Holes.Add(holes.ToJavaList());
                }
            }

            return polygonOptions;
        }

        private PolygonOptions GetPolygonOptions()
        {
            var polygonOptions = new PolygonOptions();
            polygonOptions.InvokeStrokeColor(Android.Graphics.Color.Orange);
            polygonOptions.InvokeFillColor(Android.Graphics.Color.GreenYellow);
            polygonOptions.InvokeStrokeWidth(3);
            polygonOptions.Clickable(true);
            return polygonOptions;
        }

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);

            _mapReady = true;

            if (_polygonsQueued)
            {
                OnUpdateFeatures();
            }

            map.PolygonClick += OnPolygonClick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (NativeMap != null)
                {
                    NativeMap.PolygonClick -= OnPolygonClick;
                }
            }
            base.Dispose(disposing);
        }

        private void OnPolygonClick(object sender, GoogleMap.PolygonClickEventArgs e)
        {
            //Do things

            GPolygon geoPolygon = _polygons.FirstOrDefault(p => p.Value.Id == e.Polygon.Id).Key;
            if (geoPolygon == null)
            {
                return;
            }
        }
    }
}