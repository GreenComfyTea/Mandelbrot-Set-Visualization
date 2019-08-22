using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MandelbrotSetVisualization
{
	class MandelbrotSet
	{
		private WriteableBitmap writeableBitmap;

		public WriteableBitmap WriteableBitmap { get => writeableBitmap; set => writeableBitmap = value; }

		public MandelbrotSet(int width, int height, int iterationCount, double RealMin = -3.1d, double RealMax = 1.8d, double ImaginaryMin = -1.2d, double ImaginaryMax = 1.2d)
		{
			writeableBitmap = Create(width, height, iterationCount, RealMin, RealMax, ImaginaryMin, ImaginaryMax);
		}

		private WriteableBitmap Create(int width, int height, int iterationCount, double RealMin, double RealMax, double ImaginaryMin, double ImaginaryMax)
		{
			double RealFactor = (RealMax - RealMin) / (width - 1);
			double ImaginaryFactor = (ImaginaryMax - ImaginaryMin) / (height - 1);

			WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
			int stride = writeableBitmap.Format.BitsPerPixel / 8;
			byte[] colors = new byte[width * height * stride];

			int[] nums = Enumerable.Range(0, width).ToArray();
			Parallel.ForEach(nums, x =>
			{
				for (int y = 0; y < height; y++)
				{
					bool inside = false;
					int iteration = 0;

					Complex c = new Complex(RealMin + x * RealFactor, ImaginaryMax - y * ImaginaryFactor);
					Complex z = new Complex(c);
					if (Math.Sqrt(Math.Pow(c.Real - 0.25d, 2) + Math.Pow(c.Imaginary, 2)) <= 0.5d - 0.5d * Math.Cos(Math.Atan2(c.Imaginary, c.Real - 0.25d)))
					{
						inside = true;
					}
					else
					{
						for (; iteration < iterationCount; iteration++)
						{
							if (Math.Pow(z.Real, 2) + Math.Pow(z.Imaginary, 2) > 4d)
							{
								break;
							}

							z = Complex.Pow(z, 2) + c;
						}
					}

					double value = ((double) (iteration + 1 - Math.Log(Math.Log(Complex.Abs(z))) / Math.Log(2))) / (double) iterationCount;

					Color color = Colors.Black;
					if (!inside)
					{
						color = HsvToRgb(value * 360d, 1d, value < 1d ? 1d : 0d);
					}

					colors[(x + y * width) * stride] = color.R;
					colors[(x + y * width) * stride + 1] = color.G;
					colors[(x + y * width) * stride + 2] = color.B;
				}
			});

			writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), colors, stride * width, 0, 0);
			writeableBitmap.Freeze();
			return writeableBitmap;
		}

		private static Color HsvToRgb(double h, double S, double V)
		{
			byte v = Convert.ToByte(V * 255);

			double H = h;
			while (H < 0)
			{
				H += 360;
			};
			while (H >= 360)
			{
				H -= 360;
			};

			if (V <= 0)
			{
				return Color.FromRgb(0, 0, 0);
			}
			else if (S <= 0)
			{
				return Color.FromRgb(v, v, v);
			}
			else
			{
				double hf = H / 60.0;
				int i = (int) Math.Floor(hf);
				double f = hf - i;

				byte pv = Convert.ToByte(V * (1 - S) * 255);
				byte qv = Convert.ToByte(V * (1 - S * f) * 255);
				byte tv = Convert.ToByte(V * (1 - S * (1 - f)) * 255);
				
				switch (i)
				{
					case 0:		return Color.FromRgb(v, tv, pv);
					case 1:		return Color.FromRgb(qv, v, pv);
					case 2:		return Color.FromRgb(pv, v, tv);
					case 3:		return Color.FromRgb(pv, qv, v);
					case 4:		return Color.FromRgb(tv, pv, v);
					case 5:		return Color.FromRgb(v, pv, qv);
					case 6:		return Color.FromRgb(v, tv, pv);
					case -1:	return Color.FromRgb(v, pv, qv);
					default:	return Color.FromRgb(v, v, v);
				}
			}
		}
	}
}
