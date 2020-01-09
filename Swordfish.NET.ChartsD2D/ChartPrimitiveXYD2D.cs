using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct2D1;
using System.Windows;

namespace Swordfish.NET.Charts {
  internal class ChartPrimitiveXYD2D : ChartPrimitiveXY, IChartRendererD2D {

    /// <summary>
    /// Geometry to render, geometry created by one factory can't be rendered by another factory
    /// </summary>
    protected Dictionary<IntPtr, GeometryAndFlag> _geometryByFactory = new Dictionary<IntPtr, GeometryAndFlag>();
    protected StrokeStyle _strokeStyle;

    public ChartPrimitiveXYD2D() {
    }

    public ChartPrimitiveXYD2D(ChartPrimitiveXYD2D chartPrimitiveXY)
      : base(chartPrimitiveXY) {
    }

    public override ChartPrimitiveXY Clone() {
      return new ChartPrimitiveXYD2D(this);
    }

    protected override void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      base.Points_CollectionChanged(sender, e);
      foreach(GeometryAndFlag gnf in _geometryByFactory.Values) {
        gnf.RecalcGeometry = true;
      }
    }

    protected void CalculateGeometry(Factory d2dFactory, System.Windows.Rect rect) {
      if (_strokeStyle == null) {
        StrokeStyleProperties porps = new StrokeStyleProperties();
        porps.LineJoin = LineJoin.Bevel;
        _strokeStyle = new StrokeStyle(d2dFactory,porps);
      }

      Func<bool, bool, System.Windows.Media.Color, GeometryGroup> buildGeometry = (isFilled, isOk, color) => {
        List<Geometry> geometry = new List<Geometry>();
        if (isOk && Points.Count > 0 && color != System.Windows.Media.Colors.Transparent) {

          var childGeometry = new PathGeometry(d2dFactory);
          using (GeometrySink ctx = childGeometry.Open()) {

            ctx.BeginFigure(Points.First().ToD2D(), isFilled ? FigureBegin.Filled : FigureBegin.Hollow);
            foreach (Point point in Points.Skip(1)) {
              ctx.AddLine(point.ToD2D());
            }
            ctx.EndFigure(isFilled ? FigureEnd.Closed : FigureEnd.Open);
            ctx.Close();
          }
          geometry.Add(childGeometry);
          return new GeometryGroup(d2dFactory,FillMode.Winding, geometry.ToArray());
        }
        return null;
      };

      _geometryByFactory[d2dFactory.NativePointer] = new GeometryAndFlag(
        buildGeometry(false, LineThickness > 0, LineColor),
        buildGeometry(true, true, FillColor), rect);
    }

    public void RenderFilledElements(PlotRendererD2D canvas, System.Windows.Rect chartArea, System.Windows.Media.MatrixTransform PrimitiveTransform) {
      if(this.FillColor != System.Windows.Media.Colors.Transparent) {
        GeometryAndFlag gnf = null;
        if(!_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf) || gnf.RecalcGeometry) {
          CalculateGeometry(canvas.D2DFactory, chartArea);
        }
        //Brush brush = IsDashed ? (Brush)(ChartUtilities.CreateHatch50(this.FillColor, new Size(2, 2))) : (Brush)(new SolidColorBrush(this.FillColor));
        if(_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf)) {

          var transformedGeometry = new TransformedGeometry(canvas.D2DFactory, gnf.FilledGeometry, PrimitiveTransform.ToD2D());
          var brush = new SolidColorBrush(canvas.RenderTarget, FillColor.ToD2D());
          canvas.RenderTarget.FillGeometry(transformedGeometry, brush);
        }
      }
    }

    public void RenderUnfilledElements(PlotRendererD2D canvas, System.Windows.Rect chartArea, System.Windows.Media.MatrixTransform PrimitiveTransform) {

      if(this.LineColor != System.Windows.Media.Colors.Transparent && LineThickness > 0) {
        GeometryAndFlag gnf = null;
        if(!_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf) || gnf.RecalcGeometry) {
          CalculateGeometry(canvas.D2DFactory, chartArea);
        }
        if(_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf)) {
          if (gnf.UnfilledGeometry != null) {
            var transformedGeometry = new TransformedGeometry(canvas.D2DFactory, gnf.UnfilledGeometry, PrimitiveTransform.ToD2D());
            var brush = new SolidColorBrush(canvas.RenderTarget, LineColor.ToD2D());
            canvas.RenderTarget.DrawGeometry(transformedGeometry, brush, (float)LineThickness, _strokeStyle);
          }
        }
      }
    }
  }
}
