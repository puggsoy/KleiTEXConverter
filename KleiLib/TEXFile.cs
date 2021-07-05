using System;
using System.IO;

namespace KleiLib
{
	public class TEXFile
	{
		public class InvalidTEXFileException : Exception
		{
			public InvalidTEXFileException(string msg) : base(msg) { }
		}

		public enum Platform : uint
		{
			PC = 12,
			XBOX360 = 11,
			PS3 = 10,
			Unknown = 0
		}

		public enum PixelFormat : uint
		{
			DXT1 = 0,
			DXT3 = 1,
			DXT5 = 2,
			ARGB = 4,
			Unknown = 7
		}

		public enum TextureType : uint
		{
			OneD = 1,
			TwoD = 2,
			ThreeD = 3,
			Cubemap = 4
		}

		public readonly char[] KTEXHeader = new char[] { 'K', 'T', 'E', 'X' };

		public struct FileStruct
		{
			public struct HeaderStruct
			{
				public uint Platform;
				public uint PixelFormat;
				public uint TextureType;
				public uint NumMips;
				public uint Flags;
				public uint Remainder;
			}

			public HeaderStruct Header;
			public byte[] Raw;
		}

		public FileStruct File;

		public struct Mipmap
		{
			public ushort Width;
			public ushort Height;
			public ushort Pitch;
			public uint DataSize;
			public byte[] Data;
		}

		public uint OldRemainder;
		public bool IsPreCaveUpdate => OldRemainder == 0x3FFFF;

		public TEXFile() { }

		public TEXFile(Stream stream)
		{
			using (BinaryReader reader = new BinaryReader(stream))
			{
				if (reader.ReadChars(4).Equals(KTEXHeader))
					throw new InvalidTEXFileException("The first 4 bytes do not match 'KTEX'");

				uint header = reader.ReadUInt32();

				File.Header = new FileStruct.HeaderStruct()
				{
					Platform = header & 0xF,
					PixelFormat = (header >> 4) & 0x1F,
					TextureType = (header >> 9) & 0xF,
					NumMips = (header >> 13) & 0x1F,
					Flags = (header >> 18) & 3,
					Remainder = (header >> 20) & 0xFFF
				};

				// For pre-cave updates I guess
				OldRemainder = (header >> 14) & 0x3FFFF;

				File.Raw = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
			}
		}

		public Mipmap GetMainMipmap()
		{
			Mipmap mipmap = new Mipmap();

			using (BinaryReader reader = new BinaryReader(new MemoryStream(File.Raw)))
			{
				mipmap.Width = reader.ReadUInt16();
				mipmap.Height = reader.ReadUInt16();
				mipmap.Pitch = reader.ReadUInt16();
				mipmap.DataSize = reader.ReadUInt32();

				// Skip the other mipmaps' headers
				reader.BaseStream.Seek((File.Header.NumMips - 1) * 10, SeekOrigin.Current);

				mipmap.Data = reader.ReadBytes((int)mipmap.DataSize);
			}

			return mipmap;
		}
	}
}
