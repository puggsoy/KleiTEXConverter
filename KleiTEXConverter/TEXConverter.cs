/*
Copyright (C) 2021  puggsoy

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


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

			byte[] rgbaData = null;

			switch((TEXFile.PixelFormat)texFile.File.Header.PixelFormat)
			{
				case TEXFile.PixelFormat.DXT1:
					rgbaData = Squish.DecompressImage(mipmap.Data, mipmap.Width, mipmap.Height, SquishFlags.Dxt1);
					break;
				case TEXFile.PixelFormat.DXT3:
					rgbaData = Squish.DecompressImage(mipmap.Data, mipmap.Width, mipmap.Height, SquishFlags.Dxt3);
					break;
				case TEXFile.PixelFormat.DXT5:
					rgbaData = Squish.DecompressImage(mipmap.Data, mipmap.Width, mipmap.Height, SquishFlags.Dxt5);
					break;
				case TEXFile.PixelFormat.ARGB:
					rgbaData = mipmap.Data;
					break;
				default:
					throw new Exception(string.Format("Unknown pixel format: 0x{0}", texFile.File.Header.PixelFormat.ToString("X")));
			}

			byte[] bgraData = new byte[rgbaData.Length];

			// bgraData will be read as little-endian ARGB
			for (int i = 0; i < bgraData.Length; i += 4)
			{
				bgraData[i] = rgbaData[i + 2];
				bgraData[i + 1] = rgbaData[i + 1];
				bgraData[i + 2] = rgbaData[i];
				bgraData[i + 3] = rgbaData[i + 3];
			}

			Bitmap bmp = new Bitmap(mipmap.Width, mipmap.Height);

			Rectangle bmpBounds = new Rectangle(0, 0, bmp.Width, bmp.Height);
			BitmapData bmpData = bmp.LockBits(bmpBounds, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			IntPtr ptr = bmpData.Scan0;
			Marshal.Copy(bgraData, 0, ptr, bgraData.Length);
			bmp.UnlockBits(bmpData);

			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			return bmp;
		}
	}
}
