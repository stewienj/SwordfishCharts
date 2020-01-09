using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Interaction logic for CoordinateConverterCorner.xaml
  /// </summary>
  public partial class CoordinateConverterCorner : UserControl {

    public CoordinateConverterCorner() {
      InitializeComponent();
    }

    public string Header {
      get { return (string)GetValue(HeaderProperty); }
      set { SetValue(HeaderProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(CoordinateConverterCorner), new PropertyMetadata("Corner"));

    public string RegisteredClickX {
      get { return (string)GetValue(RegisteredClickXProperty); }
      set { SetValue(RegisteredClickXProperty, value); }
    }

    // Using a DependencyProperty as the backing store for RegisteredClickX.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RegisteredClickXProperty =
        DependencyProperty.Register("RegisteredClickX", typeof(string), typeof(CoordinateConverterCorner), new PropertyMetadata("0.0"));

    public string RegisteredClickY {
      get { return (string)GetValue(RegisteredClickYProperty); }
      set { SetValue(RegisteredClickYProperty, value); }
    }

    // Using a DependencyProperty as the backing store for RegisteredClickY.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RegisteredClickYProperty =
        DependencyProperty.Register("RegisteredClickY", typeof(string), typeof(CoordinateConverterCorner), new PropertyMetadata("0.0"));

    public string ClickX {
      get { return (string)GetValue(ClickXProperty); }
      set { SetValue(ClickXProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ClickX.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ClickXProperty =
        DependencyProperty.Register("ClickX", typeof(string), typeof(CoordinateConverterCorner), new PropertyMetadata("0.0", (s,e)=>{
          CoordinateConverterCorner control = (CoordinateConverterCorner)s;
          if (control.IsFloating == true) {
            control.RegisteredClickX = (string)e.NewValue;
          }
        }));

    public string ClickY {
      get { return (string)GetValue(ClickYProperty); }
      set { SetValue(ClickYProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ClickY.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ClickYProperty =
        DependencyProperty.Register("ClickY", typeof(string), typeof(CoordinateConverterCorner), new PropertyMetadata("0.0", (s, e) => {
          CoordinateConverterCorner control = (CoordinateConverterCorner)s;
          if (control.IsFloating == true) {
            control.RegisteredClickY = (string)e.NewValue;
          }
        }));

    public string ConvertToX {
      get { return (string)GetValue(ConvertToXProperty); }
      set { SetValue(ConvertToXProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ConvertToX.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ConvertToXProperty =
        DependencyProperty.Register("ConvertToX", typeof(string), typeof(CoordinateConverterCorner), new PropertyMetadata("0.0", (d, e) => {
        }));

    public string ConvertToY {
      get { return (string)GetValue(ConvertToYProperty); }
      set { SetValue(ConvertToYProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ConvertToY.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ConvertToYProperty =
        DependencyProperty.Register("ConvertToY", typeof(string), typeof(CoordinateConverterCorner), new PropertyMetadata("0.0"));

    public bool? IsFloating {
      get { return (bool?)GetValue(IsFloatingProperty); }
      set { SetValue(IsFloatingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsFloating.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsFloatingProperty =
        DependencyProperty.Register("IsFloating", typeof(bool?), typeof(CoordinateConverterCorner), new PropertyMetadata(true));
  }
}
