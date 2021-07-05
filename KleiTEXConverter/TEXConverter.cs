using KleiLib;
using System.IO;
using static KleiLib.TEXFile;
using ManagedSquish;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KleiTEXConverter
{
	class TEXConverter
	{
		public Bitmap ConvertToBitmap(Stream stream)
		{
			TEXFile texFile = new TEXFile(stream);

			Mipmap mipmap = texFile.GetMainMipmap();

			byte[] argbData = null;

			switch((TEXFile.PixelFormat)texFile.File.Header.PixelFormat)
			{
				case TEXFile.PixelFormat.DXT1:
					argbData = Squish.DecompressImage(mipmap.Data, mipmap.Width, mipmap.Height, SquishFlags.Dxt1);
					break;
				case TEXFile.PixelFormat.DXT3:
					argbData = Squish.DecompressImage(mipmap.Data, mipmap.Width, mipmap.Height, SquishFlags.Dxt3);
					break;
				case TEXFile.PixelFormat.DXT5:
					argbData = Squish.DecompressImage(mipmap.Data, mipmap.Width, mipmap.Height, SquishFlags.Dxt5);
					break;
				case TEXFile.PixelFormat.ARGB:
					argbData = mipmap.Data;
					break;
				default:
					throw new Exception(string.Format("Unknown pixel format: 0x{0}", texFile.File.Header.PixelFormat.ToString("X")));
			}

			BinaryReader imgReader = new BinaryReader(new MemoryStream(argbData));

			Bitmap bmp = new Bitmap(mipmap.Width, mipmap.Height);

			Rectangle bmpBounds = new Rectangle(0, 0, bmp.Width, bmp.Height);
			BitmapData bmpData = bmp.LockBits(bmpBounds, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			IntPtr ptr = bmpData.Scan0;
			Marshal.Copy(argbData, 0, ptr, argbData.Length);
			bmp.UnlockBits(bmpData);

			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			return bmp;
		}
	}
}
