using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Swordfish.NET.Charts {
  public class ChartControlD2D : ChartControl {

    private PlotRendererD2D _plotCanvasD2D;

    public ChartControlD2D() {
      this.Loaded += ChartControlD2D_Loaded;
    }

    public override ChartPrimitiveHBar CreateHBar(double centerPoint, double height) {
      return new ChartPrimitiveHBarD2D(centerPoint, height);
    }

    public override ChartPrimitiveEventLine CreateEventLine() {
      return new ChartPrimitiveEventLineD2D();
    }

    public override ChartPrimitiveXY CreateXY() {
      return new ChartPrimitiveXYD2D();
    }

    public override ChartPrimitiveLineSegments CreateLineSegments() {
      return new ChartPrimitiveLineSegmentsD2D();
    }

    void ChartControlD2D_Loaded(object sender, RoutedEventArgs e) {
      this.Loaded -= ChartControlD2D_Loaded;
      Window parentWindow = Window.GetWindow(this);
      _plotCanvasD2D = new PlotRendererD2D();
      _plotCanvasD2D.Initialize(parentWindow);
      if(_plotCanvasD2D.IsValid) {
        // Disable and replace the WPF renderer
        _plotRenderer.PrimitiveList = null;

        _plotCanvasD2D.PrimitiveList = _primitiveList;
        _plotCanvasD2D.PrimitiveTransform = _shapeTransform;
        _plotRenderer = _plotCanvasD2D;
        _imageHost.Source = _plotCanvasD2D.ImageSource;
        OnPlotResized();
      }
    }
  }
}
