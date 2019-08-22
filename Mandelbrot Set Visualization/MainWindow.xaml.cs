using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MandelbrotSetVisualization
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly double normalTime = 0.00000000055d;

		private int iterationCount;

		private double xMin;
		private double xMax;
		private double yMin;
		private double yMax;

		private bool isMandelbrotSetCreated;

		private Point first;
		private Point second;

		private Selection selection;

		bool withBounds = false;

		public MainWindow()
		{
			InitializeComponent();

			Loaded += delegate
			{
				selection = new Selection();
				selection.AspectRatio = canvas.ActualWidth / canvas.ActualHeight;
			};

			RenderOptions.SetBitmapScalingMode(canvasImage, BitmapScalingMode.NearestNeighbor);
			RenderOptions.SetEdgeMode(canvasImage, EdgeMode.Aliased);

			isMandelbrotSetCreated = false;

			ResetBounds();

			iterationCountTextBox.Text = GetOptimalIterationCount();
		}

		private string GetOptimalIterationCount()
		{
			return Convert.ToInt32(1d / (normalTime * Environment.ProcessorCount * Width * Height)).ToString();
		}

		private void ResetBounds()
		{
			xMin = -3.1d;
			xMax = 1.8d;

			yMin = -1.2d;
			yMax = 1.2d;
		}

		private void Start(object sender, RoutedEventArgs e)
		{
			withBounds = false;
			ProcessAsync();
		}

		private void StartWithBounds(object sender, RoutedEventArgs e)
		{
			withBounds = true;
			ProcessAsync();
		}

		private async void ProcessAsync()
		{
			try
			{
				ReadInput();
				if (withBounds)
				{
					ReadBounds();
				}
				else
				{
					ResetBounds();
				}
			}
			catch (Exception)
			{
				ConsoleBox.Text = "Error!";
				return;
			}

			Stopwatch stopwatch = Stopwatch.StartNew();

			isMandelbrotSetCreated = false;

			StartButton.IsEnabled = false;
			StartWithBoundsButton.IsEnabled = false;
			ConsoleBox.Text = "Processing...";

			canvasImage.Source = (await Task.Run(() => new MandelbrotSet((int)canvas.ActualWidth, (int)canvas.ActualHeight, iterationCount, xMin, xMax, yMin, yMax))).WriteableBitmap;

			if (canvas.Children.Contains(selection.Rectangle))
			{
				canvas.Children.Remove(selection.Rectangle);
			}

			StartButton.IsEnabled = true;
			StartWithBoundsButton.IsEnabled = true;
			isMandelbrotSetCreated = true;

			stopwatch.Stop();
			ConsoleBox.Text = "Done! Elapsed Time: " + stopwatch.Elapsed;
		}

		private void ReadInput()
		{
			int iterationCount = int.Parse(iterationCountTextBox.Text);

			if (iterationCount <= 0)
			{
				throw new Exception();
				
			}

			this.iterationCount = iterationCount;
		}

		private void ReadBounds()
		{
			double xMin = Double.Parse(XminTextBox.Text.Replace(',', '.'));
			double xMax = Double.Parse(XmaxTextBox.Text.Replace(',', '.'));
			double yMin = Double.Parse(YminTextBox.Text.Replace(',', '.'));
			double yMax = Double.Parse(YmaxTextBox.Text.Replace(',', '.'));

			if (xMax < xMin || yMax < yMin)
			{
				throw new Exception();
			}

			this.xMin = xMin;
			this.xMax = xMax;
			this.yMin = yMin;
			this.yMax = yMax;
		}

		private void SetBounds()
		{
			double xMinTemp = 0d;
			double xMaxTemp = 0d;
			double yMinTemp = 0d;
			double yMaxTemp = 0d;

			try
			{
				xMinTemp = AffineTransformation(selection.Position.X, 0, canvas.ActualWidth, xMin, xMax);
				xMaxTemp = AffineTransformation(selection.Position.X + selection.Rectangle.Width, 0, canvas.ActualWidth, xMin, xMax);
				yMinTemp = AffineTransformation(selection.Position.Y + selection.Rectangle.Height, 0, canvas.ActualHeight, yMax, yMin);
				yMaxTemp = AffineTransformation(selection.Position.Y, 0, canvas.ActualHeight, yMax, yMin);
			}
			catch(Exception)
			{
				ConsoleBox.Text = "Error!";
				return;
			}

			XminTextBox.Text = xMinTemp.ToString();
			XmaxTextBox.Text = xMaxTemp.ToString();
			YminTextBox.Text = yMinTemp.ToString();
			YmaxTextBox.Text = yMaxTemp.ToString();
		}

		private void MousePressed(object sender, MouseButtonEventArgs e)
		{
			if (isMandelbrotSetCreated)
			{
				first = e.GetPosition(canvas);
				if (canvas.Children.Contains(selection.Rectangle))
				{
					canvas.Children.Remove(selection.Rectangle);
				}
				canvas.Children.Add(selection.Rectangle);
				canvas.CaptureMouse();
			}
		}

		private void MouseReleased(object sender, MouseButtonEventArgs e)
		{
			if (isMandelbrotSetCreated && canvas.IsMouseCaptured)
			{
				canvas.ReleaseMouseCapture();
				second = e.GetPosition(canvas);
				selection.Update(first, second);
				SetBounds();
			}
		}

		private void MouseMoved(object sender, MouseEventArgs e)
		{
			if (isMandelbrotSetCreated && canvas.IsMouseCaptured)
			{
				second = e.GetPosition(canvas);
				selection.Update(first, second);

				Canvas.SetLeft(selection.Rectangle, selection.Position.X);
				Canvas.SetTop(selection.Rectangle, selection.Position.Y);
			}
		}

		private double AffineTransformation(double value, double oldMin, double oldMax, double newMin, double newMax)
		{
			if(value < oldMin || value > oldMax)
			{
				throw new Exception("Incorrect arguments");
			}

			return ((value - oldMin) * ((newMax - newMin) / (oldMax - oldMin))) + newMin;
		}
	}
}
