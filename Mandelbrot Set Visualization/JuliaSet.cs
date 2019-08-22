using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ld4_Luznovs_Mandelbrot_Set
{
	class MandelbrotSet
	{
		static readonly object monitor = new object();

		private MandelbrotSet() { }

		public static WriteableBitmap Create(int width, int height, int iterationCount, double xMin = -2d, double xMax = 0.75d, double yMin = -1.375d, double yMax = 1.375d)
		{
			int[,] iterations = new int[width, height];

			int[] nums = Enumerable.Range(0, width).ToArray();
			Parallel.ForEach(nums, x =>
			{
				for (int y = 0; y < height; y++)
				{
					double xScale = mapD(x, 0, width, minX, maxX);
					double yScale = mapD(y, 0, height, minY, maxY);

					Complex z0 = new Complex(0, 0);

					int i = 0;
					for (; i < iterationCount; i++)
					{
						if (Complex.Abs(z) > 2d)
						{
							break;
						}
						z = Complex.Pow(z, 2) + z0;
					}
					iterations[x, y] = i;

				}
			});

			WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);

			int stride = writeableBitmap.Format.BitsPerPixel / 8;

			byte[] colors = new byte[width * height * stride];

			int index = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int gray = 255 * iterations[x, y] / iterationCount;

					colors[index++] = Convert.ToByte(gray);
					colors[index++] = Convert.ToByte(gray);
					colors[index++] = Convert.ToByte(gray);
				}
			}

			writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), colors, stride * width, 0, 0);
			writeableBitmap.Freeze();
			return writeableBitmap;
		}
	}
}
