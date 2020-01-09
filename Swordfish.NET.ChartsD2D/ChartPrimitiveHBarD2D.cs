using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swordfish.NET.Charts {
  internal class ChartPrimitiveHBarD2D : ChartPrimitiveHBar, IChartRendererD2D {

    public class GeometryAndFlag {
      public GeometryAndFlag(List<Geometry> unfilledGeometry, List<Geometry> filledGeometry, System.Windows.Rect rect) {
        UnfilledGeometry = unfilledGeometry;
        FilledGeometry = filledGeometry;
        RecalcGeometry = false;
        Rect = rect;
      }

      public List<Geometry> UnfilledGeometry = null;
      public List<Geometry> FilledGeometry = null;
      public bool RecalcGeometry = true;
      public System.Windows.Rect Rect;
    }

    /// <summary>
    /// Geometry to render
    /// </summary>
    /// <summary>
    /// Geometry to render, geometry created by one factory can't be rendered by another factory
    /// </summary>
    protected Dictionary<IntPtr, GeometryAndFlag> _geometryByFactory = new Dictionary<IntPtr, GeometryAndFlag>();

    internal ChartPrimitiveHBarD2D(double centerPoint, double height)
      : base(centerPoint, height) {
    }

    protected override void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      base.Points_CollectionChanged(sender, e);
      foreach(GeometryAndFlag gnf in _geometryByFactory.Values) {
        gnf.RecalcGeometry = true;
      }
    }

    public void RenderFilledElements(PlotRendererD2D canvas, System.Windows.Rect chartArea, System.Windows.Media.MatrixTransform PrimitiveTransform) {
      GeometryAndFlag gnf = null;
      if(!_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf) || gnf.RecalcGeometry) {
        CalculateGeometry(canvas.D2DFactory, chartArea);
      }
      if(_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf)) {
        int colorIndex = 0;
        foreach(var childGeometry in gnf.FilledGeometry) {
          System.Windows.Media.Color fillColor = _colors[colorIndex].Item2;
          if(fillColor != System.Windows.Media.Colors.Transparent) {
            //Brush brush = IsDashed ? (Brush)(ChartUtilities.CreateHatch50(fillColor, new Size(2, 2))) : (Brush)(new SolidColorBrush(fillColor));
            var brush = new SolidColorBrush(canvas.RenderTarget, fillColor.ToD2D());
            var transformedGeometry = new TransformedGeometry(canvas.D2DFactory, childGeometry, PrimitiveTransform.ToD2D());
            canvas.RenderTarget.FillGeometry(transformedGeometry, brush);
          }
          ++colorIndex;
        }
      }
    }

    public void RenderUnfilledElements(PlotRendererD2D canvas, System.Windows.Rect chartArea, System.Windows.Media.MatrixTransform PrimitiveTransform) {
      GeometryAndFlag gnf = null;
      if(!_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf) || gnf.RecalcGeometry) {
        CalculateGeometry(canvas.D2DFactory, chartArea);
      }
      if(_geometryByFactory.TryGetValue(canvas.D2DFactory.NativePointer, out gnf)) {
        int colorIndex = 0;
        foreach(var childGeometry in gnf.UnfilledGeometry) {
          System.Windows.Media.Color lineColor = _colors[colorIndex].Item1;
          if(lineColor != System.Windows.Media.Colors.Transparent) {
            var brush = new SolidColorBrush(canvas.RenderTarget, lineColor.ToD2D());
            var transformedGeometry = new TransformedGeometry(canvas.D2DFactory,childGeometry, PrimitiveTransform.ToD2D());
            canvas.RenderTarget.DrawGeometry(transformedGeometry, brush, (float)LineThickness);

          }
          ++colorIndex;
        }
      }
    }

    /// <summary>
    /// Does a one off calculation of the geometry to be rendered
    /// </summary>
    protected void CalculateGeometry(Factory d2dFactory, System.Windows.Rect rect) {
      Func<bool, int, PathGeometry> buildGeometry = (bool isFilled, int pointIndex) => {
        PathGeometry childGeometry = new PathGeometry(d2dFactory);
        using(GeometrySink ctx = childGeometry.Open()) {
          // Break up into groups of 4
          ctx.BeginFigure(Points[pointIndex].ToD2D(), isFilled ? FigureBegin.Filled : FigureBegin.Hollow);
          for(int j = 1; j < 4; ++j) {
            ctx.AddLine(Points[pointIndex + j].ToD2D());
          }
          ctx.AddLine(Points[pointIndex].ToD2D());
          ctx.EndFigure(isFilled ? FigureEnd.Closed : FigureEnd.Open);
          ctx.Close();
        }
        return childGeometry;
      };

      List<Geometry> filledGeometry = new List<Geometry>();
      List<Geometry> unfilledGeometry = new List<Geometry>();

      if(Points.Count > 0) {
        for(int pointIndex = 0, colorIndex = 0; pointIndex < (Points.Count - 3); pointIndex += 4, colorIndex += 1) {
          unfilledGeometry.Add(buildGeometry(false, pointIndex));
          filledGeometry.Add(buildGeometry(true, pointIndex));
        }
      }
      _geometryByFactory[d2dFactory.NativePointer] = new GeometryAndFlag(unfilledGeometry, filledGeometry, rect);
    }
  }
}
