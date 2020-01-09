using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swordfish.NET.Charts {
  public interface IChartRendererD2D {
    void RenderFilledElements(PlotRendererD2D canvas, System.Windows.Rect ChartArea, System.Windows.Media.MatrixTransform PrimitiveTransform);
    void RenderUnfilledElements(PlotRendererD2D canvas, System.Windows.Rect ChartArea, System.Windows.Media.MatrixTransform PrimitiveTransform);
  }

  public class GeometryAndFlag {
    public GeometryAndFlag(GeometryGroup unfilledGeometry, GeometryGroup filledGeometry, System.Windows.Rect rect) {
      UnfilledGeometry = unfilledGeometry;
      FilledGeometry = filledGeometry;
      RecalcGeometry = false;
      Rect = rect;
    }

    public GeometryGroup UnfilledGeometry = null;
    public GeometryGroup FilledGeometry = null;
    public bool RecalcGeometry = true;
    public System.Windows.Rect Rect;
  }

}
