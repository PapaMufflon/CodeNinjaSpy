using System.Windows;
using System.Windows.Media;
using MufflonoSoft.CodeNinjaSpy.ViewModels;

namespace MufflonoSoft.CodeNinjaSpy.Views
{
    public partial class CodeNinjaSpyShell
    {
        public CodeNinjaSpyShell()
        {
            InitializeComponent();
            PaintBackground();
            DataContext = new CodeNinjaSpyViewModel();
        }

        private void PaintBackground()
        {
            var backgroundSquare = new GeometryDrawing(Brushes.Black, null, new RectangleGeometry(new Rect(0, 0, 100, 100)));

            var aGeometryGroup = new GeometryGroup();
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 50, 50)));
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(50, 50, 50, 50)));

            var checkerBrush = new LinearGradientBrush();
            checkerBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
            checkerBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0, 22, 0), 1.0));

            var checkers = new GeometryDrawing(checkerBrush, null, aGeometryGroup);

            var checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);
            checkersDrawingGroup.Children.Add(checkers);

            var myBrush = new DrawingBrush
            {
                Drawing = checkersDrawingGroup,
                Viewport = new Rect(0, 0, 0.02, 0.02),
                TileMode = TileMode.Tile,
                Opacity = 0.5
            };

            LayoutRoot.Background = myBrush;
        }
    }
}