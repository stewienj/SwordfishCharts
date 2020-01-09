using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Swordfish.NET.Charts {
  public static class ChartPrimitiveExtensions {

    public static SharpDX.Mathematics.Interop.RawVector2 ToD2D(this Point point) {
      return new SharpDX.Mathematics.Interop.RawVector2((float)point.X, (float)point.Y);
    }

    public static SharpDX.Mathematics.Interop.RawMatrix3x2 ToD2D(this System.Windows.Media.MatrixTransform transform) {
      return new SharpDX.Mathematics.Interop.RawMatrix3x2(
        (float)transform.Matrix.M11,
        (float)transform.Matrix.M12,
        (float)transform.Matrix.M21,
        (float)transform.Matrix.M22,
        (float)transform.Matrix.OffsetX,
        (float)transform.Matrix.OffsetY
        );
    }

    public static SharpDX.Mathematics.Interop.RawColor4 ToD2D(this System.Windows.Media.Color color) {
      return new SharpDX.Mathematics.Interop.RawColor4(
        color.R * 0.00390625f,
        color.G * 0.00390625f,
        color.B * 0.00390625f,
        color.A * 0.00390625f
        );
    }
  }
}
