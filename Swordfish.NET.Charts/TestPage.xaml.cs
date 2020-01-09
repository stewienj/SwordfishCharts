// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Charts\TestPage.xaml.cs                         **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using Microsoft.Win32;
using Swordfish.NET.General;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Interaction logic for Page1.xaml
  /// </summary>
  public partial class TestPage : Window {

    public ChartPrimitiveXY _dummyLine = null;

    public TestPage() {
      this.DataContext = this;
      InitializeComponent();
      this.Title += " (Build " + PeHeaderReader.GetAssemblyHeader().TimeStamp.ToString() + ")";
      ChartUtilities.AddTestLines(xyLineChart);
      xyLineChart.SubNotes = new string[] { "Right or Middle Mouse Button To Zoom, Left Mouse Button To Pan, Right Double-Click To Reset" };
      copyToClipboard.DoCopyToClipboard = ChartUtilities.CopyChartToClipboard;
      xyLineChart.PointSelected += xyLineChart_PointSelected;
    }

    void xyLineChart_PointSelected(object sender, PointSelectedArgs e) {
      _pointsClicked.Text += string.Format("{0}\t{1}\n", e.SelectedPoint.X, e.SelectedPoint.Y);
      LastPointSelected = e.SelectedPoint;

    }

    public Point LastPointSelected {
      get { return (Point)GetValue(LastPointSelectedProperty); }
      set { SetValue(LastPointSelectedProperty, value); }
    }

    // Using a DependencyProperty as the backing store for LastPointSelected.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LastPointSelectedProperty =
        DependencyProperty.Register("LastPointSelected", typeof(Point), typeof(TestPage), new PropertyMetadata(new Point(0,0)));


    public string TestText {
      get { return (string)GetValue(TestTextProperty); }
      set { SetValue(TestTextProperty, value); }
    }

    // Using a DependencyProperty as the backing store for TestText.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TestTextProperty =
        DependencyProperty.Register("TestText", typeof(string), typeof(TestPage), new PropertyMetadata("Testing"));

    

    public string LastPointSelectedX {
      get { return (string)GetValue(LastPointSelectedXProperty); }
      set { SetValue(LastPointSelectedXProperty, value); }
    }

    // Using a DependencyProperty as the backing store for LastPointSelected.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LastPointSelectedXProperty =
        DependencyProperty.Register("LastPointSelectedX", typeof(string), typeof(TestPage), new PropertyMetadata("0.0"));
   

    public ChartControl XYLineChart {
      get {
        return xyLineChart;
      }
    }

    private void LoadBackdrop_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      OpenFileDialog dialog = new OpenFileDialog();
      if (dialog.ShowDialog() == true) {
        xyLineChart.Reset();
        xyLineChart.Oversize = 0.0;

        using (MemoryStream backupStream = new MemoryStream()) {
          using (FileStream stream = new FileStream(dialog.FileName, FileMode.Open)) {
            stream.CopyTo(backupStream);
          }
        }
        xyLineChart.Backdrop = new BitmapImage(new Uri(dialog.FileName));

        _dummyLine = xyLineChart.CreateXY();

        _dummyLine.IsHitTest = false;
        _dummyLine.ShowPoints = false;
        _dummyLine.AddPoint(0, 1);
        _dummyLine.AddPoint(0, 0);
        _dummyLine.AddPoint(1, 0);
        xyLineChart.AddPrimitive(_dummyLine);

        xyLineChart.RedrawPlotLines();

      }
    }

    private void ClearAllPoints_Click(object sender, RoutedEventArgs e) {
      _pointsClicked.Text = "";
      _transformedPoints.Text = "";
    }

  }
}