using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreLocation;
using GeoJson.UI.Controls;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using MapKit;
using ObjCRuntime;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MapView), typeof(GeoJson.iOS.Renderers.MapViewRenderer))]
namespace GeoJson.iOS.Renderers
{
    public class MapViewRenderer : Xamarin.Forms.Maps.iOS.MapRenderer
    {
        private MKMapView _nativeMap;
        private Dictionary<Polygon, MKPolygon> _polygons = new Dictionary<Polygon, MKPolygon>();
        private Dictionary<Polygon, MKPolygon> _selectedPolygons = new Dictionary<Polygon, MKPolygon>();

        public MapView FormsMapView { get; private set; }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);
            if (_nativeMap != null)
            {
                return;
            }
            _nativeMap = Control as MKMapView;
            _nativeMap.ZoomEnabled = true;
            _nativeMap.ScrollEnabled = true;

            var uiTapGesture = new UITapGestureRecognizer(tappedGesture => MapTapped(_nativeMap, tappedGesture));
            _nativeMap.AddGestureRecognizer(uiTapGesture);

            if (e.OldElement != null)
            {
                _nativeMap.RemoveOverlays(_nativeMap.Overlays);
                _nativeMap.OverlayRenderer = null;
            }

            if (e.NewElement != null)
            {
                _nativeMap.OverlayRenderer = OverlayRendererHandler;
                OnUpdateFeatures();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            switch (e.PropertyName)
            {
                case nameof(MapView.Features):
                    OnUpdateFeatures();
                    break;
            }
        }

        private void OnUpdateFeatures()
        {
            var mapView = (MapView)Element;
            if (mapView == null || !(Control is MKMapView nativeMap))
            {
                return;
            }

            FormsMapView = mapView;

            if (nativeMap.Overlays != null)
            {
                nativeMap.RemoveOverlays(nativeMap.Overlays);
            }

            List<Feature> features = mapView?.Features?.Features;
            if (features == null || features.Count() == 0)
            {
                return;
            }

            foreach (Feature feature in mapView.Features.Features)
            {
                CreatePolygonFor(feature);
            }
        }

        private void MapTapped(MKMapView mapView, UITapGestureRecognizer tappedGesture)
        {
            var mapview = tappedGesture.View as MKMapView;

            CLLocationCoordinate2D coord2D = mapView.ConvertPoint(tappedGesture.LocationInView(mapView), mapView);
            var mapPoint = MKMapPoint.FromCoordinate(coord2D);

            foreach (IMKOverlay overlay in mapview.Overlays)
            {
                var renderer = (MKOverlayPathRenderer)mapview.RendererForOverlay(overlay);
                if (renderer == null)
                {
                    return;
                }

                var polyTouched = renderer.Path.ContainsPoint(renderer.PointForMapPoint(mapPoint), true);

                if (polyTouched)
                {
                    //Do something
                }
            }
        }

        private void CreatePolygonFor(Feature feature)
        {
            switch (feature.Geometry.Type)
            {
                case GeoJSONObjectType.MultiPolygon:
                    {
                        var multiPolygonGeometry = feature.Geometry as MultiPolygon;
                        foreach (Polygon polygonGeometry in multiPolygonGeometry.Coordinates)
                        {
                            AddPolygonToMap(polygonGeometry);
                        }

                        break;
                    }

                case GeoJSONObjectType.Polygon:
                    {
                        var polygonGeometry = feature.Geometry as Polygon;
                        AddPolygonToMap(polygonGeometry);
                        break;
                    }
            }
        }

        private void AddPolygonToMap(Polygon polygonGeometry)
        {
            MKPolygon polygon = GeoJsonPolygonToPolygon(polygonGeometry);
            if (polygon != null)
            {
                try
                {
                    _nativeMap.AddOverlay(polygon);
                    _polygons.Add(polygonGeometry, polygon);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to add polygon to map or dictionary");
                }
            }
        }

        private MKPolygon GeoJsonPolygonToPolygon(Polygon geoJsonPolygon)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<LineString> coords = geoJsonPolygon?.Coordinates;
            if (coords == null || coords.Count() == 0)
            {
                return null;
            }

            LineString outer = coords.FirstOrDefault();
            IEnumerable<LineString> inner = coords.Count > 1 ? coords.Skip(1) : null;

            var outerCoordinates = new List<CLLocationCoordinate2D>();
            foreach (IPosition coordinate in outer.Coordinates)
            {
                outerCoordinates.Add(new CLLocationCoordinate2D(coordinate.Latitude, coordinate.Longitude));
            }

            var innerPolygons = new List<MKPolygon>();

            if (inner != null)
            {
                foreach (LineString linestring in inner)
                {
                    var innerCoordinates = new List<CLLocationCoordinate2D>();
                    foreach (IPosition coordinate in linestring.Coordinates)
                    {
                        innerCoordinates.Add(new CLLocationCoordinate2D(coordinate.Latitude, coordinate.Longitude));
                    }
                    innerPolygons.Add(MKPolygon.FromCoordinates(innerCoordinates.ToArray()));
                }
            }
            return MKPolygon.FromCoordinates(outerCoordinates.ToArray(), innerPolygons.ToArray());
        }

        private MKOverlayRenderer OverlayRendererHandler(MKMapView mapView, IMKOverlay overlayWrapper)
        {
            var overlay = Runtime.GetNSObject(overlayWrapper.Handle) as IMKOverlay;
            return new MKPolygonRenderer(overlay as MKPolygon)
            {
                StrokeColor = UIColor.Orange,
                FillColor = UIColor.Red.ColorWithAlpha(0.5f),
                LineWidth = 3f
            };
        }
    }

}
