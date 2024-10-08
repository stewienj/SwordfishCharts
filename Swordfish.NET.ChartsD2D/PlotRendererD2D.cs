using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System;
using System.Windows;

namespace Swordfish.NET.Charts
{
  public class PlotRendererD2D : PlotRenderer{

    // CodePack D2D
    private SharpDX.Direct2D1.Factory _d2dFactory;
    private RenderTarget _renderTarget;
    private D2DD3DImage _interopImage;
    private System.Windows.Rect _canvasRect;

    // Maintained simply to detect changes in the interop back buffer
    private IntPtr m_pIDXGISurfacePreviousNoRef;

    private bool _isValid = false;

    public PlotRendererD2D() {
      _interopImage = new D2DD3DImage();
      _interopImage.SetPixelSize(100, 100);
      _canvasRect = new System.Windows.Rect(0, 0, 100, 100);
    }

    public void Initialize(Window parentWindow) {
      // Create the D2D Factory
      _d2dFactory = new SharpDX.Direct2D1.Factory(FactoryType.SingleThreaded);

      _interopImage.HWNDOwner = (new System.Windows.Interop.WindowInteropHelper(parentWindow)).Handle;
      _interopImage.OnRender = this.DoRender;

      // Start rendering now!
      _interopImage.RequestRender();
    }

    public override void InvalidateVisual(System.Windows.Rect canvasRect) {
      _interopImage.SetPixelSize((uint)canvasRect.Width, (uint)canvasRect.Height);
      _canvasRect = canvasRect;
      _interopImage.RequestRender();
    }

    private void DoRender(IntPtr pIDXGISurface, bool dunnoWhatThisIsFor) {
      if(pIDXGISurface != m_pIDXGISurfacePreviousNoRef) {
        m_pIDXGISurfacePreviousNoRef = pIDXGISurface;

        // Create the render target
        Surface dxgiSurface = new Surface(pIDXGISurface);
        SurfaceDescription sd = dxgiSurface.Description;

        RenderTargetProperties rtp =
            new RenderTargetProperties(
                RenderTargetType.Default,
                new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                96,
                96,
                // Force bitmap rendering if you want it to work on remote desktop connections
                RenderTargetUsage.None,
                FeatureLevel.Level_DEFAULT);
        try {
          _renderTarget =  new RenderTarget( _d2dFactory,dxgiSurface, rtp);
        } catch(Exception) {
          return;
        }

        // Clear the surface to transparent
        //_renderTarget.BeginDraw();
        //_renderTarget.Clear(new ColorF(1, 1, 1, 0));
        //_renderTarget.EndDraw();

        _isValid = true;
      }

      if(PrimitiveTransform == null || PrimitiveList == null) {
        return;
      }

      _renderTarget.BeginDraw();
      _renderTarget.Clear(new SharpDX.Mathematics.Interop.RawColor4(1, 1, 1, 0));

      foreach(IChartRendererD2D primitive in PrimitiveList) {
        primitive.RenderFilledElements(this, ChartDataRange, PrimitiveTransform);
      }
      foreach(IChartRendererD2D primitive in PrimitiveList) {
        primitive.RenderUnfilledElements(this, ChartDataRange, PrimitiveTransform);
      }

      // Now render the points

      System.Windows.Rect bounds = new System.Windows.Rect(-3, -3, _canvasRect.Width + 6, _canvasRect.Height + 6);

      foreach(ChartPrimitive primitive in PrimitiveList) {
        if(primitive.ShowPoints) {
          var brush = new SolidColorBrush(RenderTarget, primitive.PointColor.ToD2D());
          Ellipse ellipse = new Ellipse(new SharpDX.Mathematics.Interop.RawVector2(0,0),2,2);
          foreach(Point point in primitive.Points) {
            Point transformedPoint = PrimitiveTransform.Transform(point);
            if(bounds.Contains(transformedPoint)) {
              ellipse.Point = transformedPoint.ToD2D();
              RenderTarget.DrawEllipse(ellipse, brush, 1);
            }
          }
        }
      }

      _renderTarget.EndDraw();
    }

    public bool IsValid {
      get {
        return _isValid;
      }
    }

    public override System.Windows.Media.ImageSource ImageSource {
      get {
        return _interopImage;
      }
    }

    public SharpDX.Direct2D1.Factory D2DFactory {
      get {
        return _d2dFactory;
      }
    }
    public RenderTarget RenderTarget {
      get {
        return _renderTarget;
      }
    }


  }
}
