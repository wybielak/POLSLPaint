using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dai;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using static System.Net.Mime.MediaTypeNames;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Ellipse = System.Windows.Shapes.Ellipse;
using Point = System.Windows.Point;
using PointCollection = System.Windows.Media.PointCollection;

namespace POLSLPaint
{
    public partial class MainWindow : Window
    {
        // VARIABLES DECLARATION

        Point currentPoint = new Point();
        int drawStyle = 0;

        Line[] lines = Array.Empty<Line>();

        Line activeLine = new Line();
        Ellipse activeEllipse = new Ellipse();
        Polygon activePoly = new Polygon();

        bool isFill = false;

        Image<Bgr, byte> img = new Image<Bgr, byte>(500, 1000);

        public MainWindow()
        {
            InitializeComponent();
            ColorStore.MAIN_COLOR = Colors.Black;
        }

        // ON CLICK REACTIONS - CHANGING DRAWING STYLE

        private void about_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aplikacja Dawid Nw. Grafika komputerowa i przetwarzanie obrazów POLSL MS 2023");
        }

        private void rubberbtn_Click(object sender, RoutedEventArgs e)
        {
            hideLineEditPoints();
            drawStyle = 0;
        }

        private void drawbtn_Click(object sender, RoutedEventArgs e)
        {
            hideLineEditPoints();
            drawStyle = 1;
        }

        private void drawpointbtn_Click(object sender, RoutedEventArgs e)
        {
            hideLineEditPoints();
            drawStyle = 2;
        }
        private void drawsegment_Click(object sender, RoutedEventArgs e)
        {
            hideLineEditPoints();
            drawStyle = 3;
        }

        private void editsegment_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 4;
            showLineEditPoints();
        }

        private void drawrect_Click(object sender, RoutedEventArgs e)
        {
            hideLineEditPoints();
            drawStyle = 5;
        }

        private void drawpoly_Click(object sender, RoutedEventArgs e)
        {
            hideLineEditPoints();
            drawStyle = 6;
        }

        // MOUSE BUTTON DOWN CLICK

        private void paintSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            colorprev.Background = new SolidColorBrush(ColorStore.MAIN_COLOR);

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                currentPoint = e.GetPosition(paintSurface);
            }

            if (drawStyle == 0)
            {
                var clickedItem = e.Source as FrameworkElement;
                if (clickedItem != null)
                {
                    if (paintSurface.Children.Contains(clickedItem))
                    {
                        paintSurface.Children.Remove(clickedItem);
                    }
                }
            }

            if (drawStyle == 2)
            {
                activeEllipse = DrawEllipse(e.GetPosition(paintSurface).X, e.GetPosition(paintSurface).Y, ColorStore.MAIN_COLOR);
                paintSurface.Children.Add(activeEllipse);
            }
            
            if (drawStyle == 3)
            {
                activeLine = DrawLine(currentPoint.X, currentPoint.Y, e.GetPosition(paintSurface).X, e.GetPosition(paintSurface).Y, ColorStore.MAIN_COLOR);
                paintSurface.Children.Add(activeLine);
            }

            if (drawStyle == 5)
            {
                activePoly = DrawRectangle(1, e.GetPosition(paintSurface).X, e.GetPosition(paintSurface).Y, ColorStore.MAIN_COLOR);
                paintSurface.Children.Add(activePoly);
            }

            if (drawStyle == 6)
            {
                activePoly = DrawPoly(1, e.GetPosition(paintSurface).X, e.GetPosition(paintSurface).Y, ColorStore.MAIN_COLOR);
                paintSurface.Children.Add(activePoly);
            }
        }

        // MOUSE MOVE ACTION

        private void paintSurface_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (e.LeftButton == MouseButtonState.Pressed && drawStyle == 0)
            {
                var clickedItem = e.Source as FrameworkElement;
                if (clickedItem != null)
                {
                    if (paintSurface.Children.Contains(clickedItem))
                    {
                        paintSurface.Children.Remove(clickedItem);
                    }
                }
            }

            if (e.LeftButton == MouseButtonState.Pressed && drawStyle == 1)
            {
                Line brush = DrawLine(currentPoint.X, currentPoint.Y, e.GetPosition(paintSurface).X, e.GetPosition(paintSurface).Y, ColorStore.MAIN_COLOR);
                currentPoint = e.GetPosition(paintSurface);
                paintSurface.Children.Add(brush);
            }

            if (drawStyle == 2)
            {
                UpdateCircle(e);
            }

            if (drawStyle == 3)
            {
                UpdateLine(e);
            }

            if (drawStyle == 5)
            {
                UpdateRectangle(e);
            }

            if (drawStyle == 6)
            {
                UpdatePolygon(e);
            }
        }

        // MOUSE BUTTON UP RELEASE

        private void paintSurface_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (drawStyle == 2)
            {
                ApplyCircle();
            }
            
            if (drawStyle == 3)
            {
                ApplyLine();
            }

            if (drawStyle == 5)
            {
                ApplyRectangle();
            }

            if (drawStyle == 6)
            {
                ApplyPolygon();
            }
        }

        // DRAWING FUNCTIONS

        private Line DrawLine(double x1, double y1, double x2, double y2, Color color)
        {
            Line line = new Line();

            Brush brushcolor = new SolidColorBrush(color);
            line.Stroke = brushcolor;
            line.StrokeEndLineCap = PenLineCap.Round;
            line.StrokeStartLineCap = PenLineCap.Round;

            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;

            return line;
        }

        private Ellipse DrawEllipse(double x, double y, Color color)
        {
            Ellipse ellipse = new Ellipse();

            double size = Math.Max(x, y);

            ellipse.Height = size;

            Canvas.SetTop(ellipse, y);
            Canvas.SetLeft(ellipse, x);

            Brush brushcolor = new SolidColorBrush(color);
            ellipse.Stroke = brushcolor;
            if (isFill) ellipse.Fill = brushcolor;

            return ellipse;
        }

        private Polygon DrawRectangle(double size, double x, double y, Color color)
        {
            PointCollection points = new PointCollection
            {
                new Point(x, y),
                new Point(x + size, y),
                new Point(x + size, y - size),
                new Point(x, y - size),
            };

            Polygon rect = new Polygon();

            Brush brushcolor = new SolidColorBrush(color);
            rect.Stroke = brushcolor;
            if (isFill) rect.Fill = brushcolor;

            rect.Points = points;

            return rect;
        }

        private Polygon DrawPoly(double size, double x, double y, Color color)
        {
            PointCollection points = new PointCollection
            {
                new Point(x, y + size),
                new Point(x + size, y + size/2),
                new Point(x + size, y - size/2),
                new Point(x, y - size),
                new Point(x - size, y - size/2),
                new Point(x - size, y + size/2),

            };

            Polygon poly = new Polygon();

            Brush brushcolor = new SolidColorBrush(color);
            poly.Stroke = brushcolor;
            if (isFill) poly.Fill = brushcolor;

            poly.Points = points;

            return poly;
        }

        // EDITING FUNCTIONS

        const double btSize = 8;
        private void showLineEditPoints()
        {

            foreach (var l in lines)
            {

                Button bts = new()
                {
                    Height = btSize,
                    Width = btSize,
                    Cursor = Cursors.SizeAll
                };

                Button bte = new()
                {
                    Height = btSize,
                    Width = btSize,
                    Cursor = Cursors.SizeAll,

                };

                bts.PreviewMouseMove += (s, e) =>
                {
                    if (bts.IsPressed)
                    {
                        double x = e.GetPosition(paintSurface).X;
                        double y = e.GetPosition(paintSurface).Y;
                        l.X1 = x;
                        l.Y1 = y;
                        Canvas.SetTop(bts, y - btSize / 2);
                        Canvas.SetLeft(bts, x - btSize / 2);

                    }

                };
                bte.PreviewMouseMove += (s, e) =>
                {
                    if (bte.IsPressed)
                    {
                        double x = e.GetPosition(paintSurface).X;
                        double y = e.GetPosition(paintSurface).Y;
                        l.X2 = x;
                        l.Y2 = y;
                        Canvas.SetTop(bte, y - btSize / 2);
                        Canvas.SetLeft(bte, x - btSize / 2);

                    }

                };


                Canvas.SetTop(bts, l.Y1 - btSize / 2);
                Canvas.SetLeft(bts, l.X1 - btSize / 2);

                Canvas.SetTop(bte, l.Y2 - btSize / 2);
                Canvas.SetLeft(bte, l.X2 - btSize / 2);

                paintSurface.Children.Add(bts);
                paintSurface.Children.Add(bte);

            }
        }
        private void hideLineEditPoints()
        {
            for (int i = paintSurface.Children.Count - 1; i >= 0; i--)
            {
                if (paintSurface.Children[i].GetType() == typeof(Button))
                {
                    paintSurface.Children.RemoveAt(i);
                }
            }
        }

        // UPDATING FUNCTIONS

        private void UpdateLine(MouseEventArgs e)
        {
            activeLine.X2 = e.GetPosition(paintSurface).X;
            activeLine.Y2 = e.GetPosition(paintSurface).Y;
        }

        private void ApplyLine()
        {
            Line line1 = activeLine;
            paintSurface.Children.Remove(activeLine);
            paintSurface.Children.Add(line1);
            lines = lines.Append(line1).ToArray();
        }

        private void UpdateCircle(MouseEventArgs e)
        {
            activeEllipse.Height = Math.Abs(currentPoint.Y - e.GetPosition(paintSurface).Y);
            activeEllipse.Width = Math.Abs(currentPoint.X - e.GetPosition(paintSurface).X);
        }
        private void ApplyCircle()
        {
            Ellipse circle1 = activeEllipse;
            paintSurface.Children.Remove(activeEllipse);
            paintSurface.Children.Add(circle1);
        }

        private void UpdateRectangle(MouseEventArgs e)
        {
            double x = currentPoint.X;
            double y = currentPoint.Y;

            double size = Math.Abs(x - e.GetPosition(paintSurface).X);

            PointCollection points = new PointCollection
                {
                    new Point(x, y),
                    new Point(x + size, y),
                    new Point(x + size, y - size),
                    new Point(x, y - size),
                };

            activePoly.Points = points;
        }

        private void ApplyRectangle()
        {
            Polygon rect1 = activePoly;
            paintSurface.Children.Remove(activePoly);
            paintSurface.Children.Add(rect1);
        }

        private void UpdatePolygon(MouseEventArgs e)
        {
            double x = currentPoint.X;
            double y = currentPoint.Y;

            double size = Math.Abs(x - e.GetPosition(paintSurface).X);

            PointCollection points = new PointCollection
            {
                new Point(x, y + size),
                new Point(x + size, y + size/2),
                new Point(x + size, y - size/2),
                new Point(x, y - size),
                new Point(x - size, y - size/2),
                new Point(x - size, y + size/2),

            };

            activePoly.Points = points;
        }

        private void ApplyPolygon()
        {
            Polygon poly1 = activePoly;
            paintSurface.Children.Remove(activePoly);
            paintSurface.Children.Add(poly1);
        }

        // SWITCHING FILL CHECKBOX

        private void fillshape_Checked(object sender, RoutedEventArgs e)
        {
            isFill = true;
            fillshape.Content = "X";
        }

        private void fillshape_Unchecked(object sender, RoutedEventArgs e)
        {
            isFill = false;
            fillshape.Content = "";
        }

        // SWITCHING COLORS

        private void cancelSelection()
        {
            black.BorderBrush = null;
            white.BorderBrush = null;
            red.BorderBrush = null;
            green.BorderBrush = null;
            blue.BorderBrush = null;
        }

        private void black_Click(object sender, RoutedEventArgs e)
        {
            cancelSelection();
            ColorStore.MAIN_COLOR = Colors.Black;
            black.BorderBrush = new SolidColorBrush(Colors.AliceBlue);
            colorprev.Background = new SolidColorBrush(ColorStore.MAIN_COLOR);
        }

        private void white_Click(object sender, RoutedEventArgs e)
        {
            cancelSelection();
            ColorStore.MAIN_COLOR = Colors.White;
            white.BorderBrush = new SolidColorBrush(Colors.AliceBlue);
            colorprev.Background = new SolidColorBrush(ColorStore.MAIN_COLOR);
        }

        private void red_Click(object sender, RoutedEventArgs e)
        {
            cancelSelection();
            ColorStore.MAIN_COLOR = Colors.Red;
            red.BorderBrush = new SolidColorBrush(Colors.AliceBlue);
            colorprev.Background = new SolidColorBrush(ColorStore.MAIN_COLOR);
        }

        private void green_Click(object sender, RoutedEventArgs e)
        {
            cancelSelection();
            ColorStore.MAIN_COLOR = Colors.Green;
            green.BorderBrush = new SolidColorBrush(Colors.AliceBlue);
            colorprev.Background = new SolidColorBrush(ColorStore.MAIN_COLOR);
        }

        private void blue_Click(object sender, RoutedEventArgs e)
        {
            cancelSelection();
            ColorStore.MAIN_COLOR = Colors.Blue;
            blue.BorderBrush = new SolidColorBrush(Colors.AliceBlue);
            colorprev.Background = new SolidColorBrush(ColorStore.MAIN_COLOR);
        }

        private void editcolor_Click(object sender, RoutedEventArgs e)
        {
            cancelSelection();
            ColorEditorWindow colorEditorWindow = new ColorEditorWindow();
            colorEditorWindow.Show();
        }

        //PHOTOS EDITING
        private void setScaledSize(int imgWidth, int imgHeight)
        {
            float widthRatio = (float)1100 / imgWidth;
            float heightRatio = (float)900 / imgHeight;

            float scaleRatio = Math.Min(widthRatio, heightRatio);

            int newWidth = (int)(imgWidth * scaleRatio);
            int newHeight = (int)(imgHeight * scaleRatio);

            paintSurface.Height = newHeight;
            paintSurface.Width = newWidth;
        }
        private void loadphoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image File|*.bmp;*.jpg;*.png";
            
            if (openDialog.ShowDialog() == true)
            {
                img = new Image<Bgr, byte>(openDialog.FileName);

                Bitmap bitmap = img.ToBitmap();

                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                
                var result = MessageBox.Show("Zalecane skalowanie dla początkujących użytkowników", "Czy chcesz przeskalować obraz?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    setScaledSize(bitmap.Width, bitmap.Height);
                }
                else
                {
                    paintSurface.Height = bitmap.Height;
                    paintSurface.Width = bitmap.Width;
                }

                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bitmapSource;
                paintSurface.Background = brush;

                orgphoto.IsEnabled = true;
                grayphoto.IsEnabled = true;
                sobelphoto.IsEnabled = true;
                linearphoto.IsEnabled = true;
            }
        }

        private void orgphoto_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = img.ToBitmap();

            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = bitmapSource;
            paintSurface.Background = brush;

            //CvInvoke.Imshow("Image", img);
            //CvInvoke.WaitKey(0);
        }

        private void grayphoto_Click(object sender, RoutedEventArgs e)
        {
            Image<Gray, byte> img2 = img.Convert<Gray, byte>();

            Bitmap bitmap = img2.ToBitmap();

            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = bitmapSource;
            paintSurface.Background = brush;

            //CvInvoke.Imshow("Image", img2);
            //CvInvoke.WaitKey(0);
        }

        private void sobelphoto_Click(object sender, RoutedEventArgs e)
        {
            Image<Gray, byte> img2 = img.Convert<Gray, byte>();
            Image<Gray, Single> img3 = img2.Sobel(1, 0, 5);

            Bitmap bitmap = img3.ToBitmap();

            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = bitmapSource;
            paintSurface.Background = brush;

            //CvInvoke.Imshow("Image", img3);
            //CvInvoke.WaitKey(0);
        }

        private void linearphoto_Click(object sender, RoutedEventArgs e)
        {
            float[,] filterMatrix = {
                {int.Parse(r0c0.Text), int.Parse(r0c1.Text), int.Parse(r0c2.Text)},
                {int.Parse(r1c0.Text), int.Parse(r1c1.Text), int.Parse(r1c2.Text)},
                {int.Parse(r2c0.Text), int.Parse(r2c1.Text), int.Parse(r2c2.Text)}
            };

            Image<Gray, byte> img2 = img.Convert<Gray, byte>();

            ConvolutionKernelF matrixKernel = new ConvolutionKernelF(filterMatrix);

            var dst = new Mat(img2.Size, DepthType.Cv8U, 1);
            CvInvoke.Filter2D(img2, dst, matrixKernel, new System.Drawing.Point(0, 0));

            Bitmap bitmap = dst.ToBitmap();

            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = bitmapSource;
            paintSurface.Background = brush;
        }
        //SAVING TO FILE
        public void SaveToPng(Uri path, Canvas canvas)
        {
            if (path == null) return;
            Transform transform = canvas.LayoutTransform;
            canvas.LayoutTransform = null;
            System.Windows.Size size = new(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            RenderTargetBitmap renderTargetBitmap = new(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32
                );

            renderTargetBitmap.Render(canvas);

            using (FileStream outStream = new(path.LocalPath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                encoder.Save(outStream);

            }
            canvas.LayoutTransform = transform;
        }

        public void SaveToJpg(Uri path, Canvas canvas)
        {
            if (path == null) return;
            Transform transform = canvas.LayoutTransform;
            canvas.LayoutTransform = null;
            System.Windows.Size size = new(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Background = System.Windows.Media.Brushes.White;
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            RenderTargetBitmap renderTargetBitmap = new(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32
                );

            renderTargetBitmap.Render(canvas);

            using (FileStream outStream = new(path.LocalPath, FileMode.Create))
            {
                JpegBitmapEncoder encoder = new();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                encoder.Save(outStream);

            }
            canvas.LayoutTransform = transform;
        }

        private void savepngimg_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new();
            saveDialog.Filter = "Image (*.png)|*.png|Image (*.jpg)|*.jpg";
            saveDialog.FilterIndex = 1;
            if (saveDialog.ShowDialog() == true)
            {
                Uri newFileUri = new(saveDialog.FileName);

                if (saveDialog.FilterIndex == 1)
                {
                    SaveToPng(newFileUri, paintSurface);

                }
                else if (saveDialog.FilterIndex == 2)
                {
                    SaveToJpg(newFileUri, paintSurface);
                }
            }
        }
        //OPACITY SETTINGS
        private void opacitycheck_Checked(object sender, RoutedEventArgs e)
        {
            surfaceOpacity.Opacity = 0;
            opacitycheck.Content = "X";
        }

        private void opacitycheck_Unchecked(object sender, RoutedEventArgs e)
        {
            surfaceOpacity.Opacity = 1;
            opacitycheck.Content = "";
        }
    }
}
