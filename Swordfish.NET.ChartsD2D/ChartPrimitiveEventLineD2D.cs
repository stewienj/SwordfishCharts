using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Swordfish.NET.Charts {
  internal class ChartPrimitiveEventLineD2D : ChartPrimitiveEventLine, IChartRendererD2D {

    /// <summary>
    /// Geometry to render
    /// </summary>
    protected Dictionary<IntPtr, GeometryAndFlag> _geometryByFactory = new Dictionary<IntPtr, GeometryAndFlag>();

    public void RenderFilledElements(PlotRendererD2D canvas, System.Windows.Rect chartArea, System.Windows.Media.MatrixTransform PrimitiveTransform) {
    }

    protected override void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      base.Points_CollectionChanged(sender, e);
      foreach(GeometryAndFlag gnf in _geometryByFactory.Values) {
        gnf.RecalcGeometry = true;
      }
    }

    public void RenderUnfilledElements(PlotRendererD2D canvas, System.Windows.Rect chartArea, System.Windows.Media.MatrixTransform PrimitiveTransform) {
      if(Points.Count > 0 && LineColor != System.Windows.Media.Colors.Transparent && LineThickness > 0) {
        GeometryAndFlag gnf = null;
        if(!_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf) || gnf.RecalcGeometry || gnf.Rect != chartArea) {
          CalculateGeometry(canvas.D2DFactory, chartArea);
        }
        if( _geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf)) {
          var transformedGeometry = new TransformedGeometry(canvas.D2DFactory, gnf.UnfilledGeometry, PrimitiveTransform.ToD2D());
          var brush = new SolidColorBrush(canvas.RenderTarget, LineColor.ToD2D());
          canvas.RenderTarget.DrawGeometry(transformedGeometry, brush, (float)LineThickness);
        }
      }
    }

    protected void CalculateGeometry(Factory d2dFactory, System.Windows.Rect rect) {

        Func<bool, GeometryGroup> buildGeometry = (isFilled) => {
          List<Geometry> geometry = new List<Geometry>();
          foreach(Point point in Points) {
            PathGeometry childGeometry = new PathGeometry(d2dFactory);
            using(GeometrySink ctx = childGeometry.Open()) {
              ctx.BeginFigure(new Point(point.X, rect.Bottom).ToD2D(), isFilled ? FigureBegin.Filled : FigureBegin.Hollow);
              ctx.AddLine(new Point(point.X, rect.Top).ToD2D());
              ctx.EndFigure(isFilled ? FigureEnd.Closed : FigureEnd.Open);
              ctx.Close();
            }
            geometry.Add(childGeometry);
          }
          return new GeometryGroup(d2dFactory, FillMode.Winding, geometry.ToArray());
        };
        GeometryGroup unfilledGeometry = buildGeometry(false);
        _geometryByFactory[d2dFactory.NativePointer] = new GeometryAndFlag(unfilledGeometry, null, rect);
      }
    }
}
