using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct2D1;
using System.Windows;

namespace Swordfish.NET.Charts {
  internal class ChartPrimitiveLineSegmentsD2D : ChartPrimitiveLineSegments, IChartRendererD2D {

    /// <summary>
    /// Geometry to render, geometry created by one factory can't be rendered by another factory
    /// </summary>
    protected Dictionary<IntPtr, GeometryAndFlag> _geometryByFactory = new Dictionary<IntPtr, GeometryAndFlag>();

    public ChartPrimitiveLineSegmentsD2D() {
    }

    public ChartPrimitiveLineSegmentsD2D(ChartPrimitiveLineSegmentsD2D chartPrimitiveXY)
      : base(chartPrimitiveXY) {
    }

    public override ChartPrimitiveLineSegments Clone() {
      return new ChartPrimitiveLineSegmentsD2D(this);
    }

    protected override void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      base.Points_CollectionChanged(sender, e);
      foreach (GeometryAndFlag gnf in _geometryByFactory.Values) {
        gnf.RecalcGeometry = true;
      }
    }

    protected void CalculateGeometry(Factory d2dFactory, System.Windows.Rect rect) {
      Func<bool, bool, System.Windows.Media.Color, GeometryGroup> buildGeometry = (isFilled, isOk, color) => {
        List<Geometry> geometry = new List<Geometry>();
        if (isOk && Points.Count > 0 && color != System.Windows.Media.Colors.Transparent) {

          var childGeometry = new PathGeometry(d2dFactory);
          using (GeometrySink ctx = childGeometry.Open()) {
            for (int pointIndex = 0; pointIndex < Points.Count; pointIndex += 2) {
                ctx.BeginFigure(Points[pointIndex].ToD2D(), isFilled ? FigureBegin.Filled : FigureBegin.Hollow);
                ctx.AddLine(Points[pointIndex+1].ToD2D());
                ctx.EndFigure(isFilled ? FigureEnd.Closed : FigureEnd.Open);
            }
            //  ctx.BeginFigure(Points.First().ToD2D(), isFilled ? FigureBegin.Filled : FigureBegin.Hollow);
            //foreach (Point point in Points.Skip(1)) {
            //  ctx.AddLine(point.ToD2D());
            //}
            //ctx.EndFigure(isFilled ? FigureEnd.Closed : FigureEnd.Open);
            ctx.Close();
          }
          geometry.Add(childGeometry);
          return new GeometryGroup(d2dFactory, FillMode.Winding, geometry.ToArray());
        }
        return null;
      };

      _geometryByFactory[d2dFactory.NativePointer] = new GeometryAndFlag(
        buildGeometry(false, LineThickness > 0, LineColor),
        buildGeometry(true, true, LineColor), rect);
    }


    public void RenderFilledElements(PlotRendererD2D canvas, System.Windows.Rect chartArea, System.Windows.Media.MatrixTransform PrimitiveTransform) {
    }

    public void RenderUnfilledElements(PlotRendererD2D canvas, System.Windows.Rect chartArea, System.Windows.Media.MatrixTransform PrimitiveTransform) {

      var storedTransform = canvas.RenderTarget.Transform;

      //canvas.RenderTarget.Transform = PrimitiveTransform.ToD2D();

      //for(int segmentIndex = 0; segmentIndex < _lineColors.Count; ++segmentIndex) {
      //  var brush = canvas.RenderTarget.CreateSolidColorBrush(_lineColors[segmentIndex].ToD2D());
      //  canvas.RenderTarget.DrawLine(Points[segmentIndex * 2].ToD2D(), Points[segmentIndex * 2 + 1].ToD2D(), brush, (float)LineThickness);
      //}
      //canvas.RenderTarget.Transform = storedTransform;


      if (this.LineColor != System.Windows.Media.Colors.Transparent && LineThickness > 0) {
        GeometryAndFlag gnf = null;
        if (!_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf) || gnf.RecalcGeometry) {
          CalculateGeometry(canvas.D2DFactory, chartArea);
        }
        if (_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf)) {

          var transformedGeometry = new TransformedGeometry(canvas.D2DFactory, gnf.UnfilledGeometry, PrimitiveTransform.ToD2D());

          var brush = new SolidColorBrush(canvas.RenderTarget, LineColor.ToD2D());

          canvas.RenderTarget.DrawGeometry(transformedGeometry, brush, (float)LineThickness);
        }
      }

    }
  }
}
