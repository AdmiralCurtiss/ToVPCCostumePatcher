using HyoutaPluginBase;
using HyoutaUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToVPCCostumePatcher {
	// questionable if I actually understand this correctly, but hopefully good enough for converting a few files...
	public class Model4 {
		public uint Magic;
		public uint FileSize;
		public uint HeaderSize;
		public uint ElementCount1;
		public uint ElementCount2;
		public List<Model4Element1a> Elements1a;
		public List<Model4Element1b> Elements1b;
		public List<Model4Element2> Elements2;

		public Model4(Stream stream, EndianUtils.Endianness endian) {
			stream.Position = 0;
			Magic = stream.ReadUInt32(endian);
			FileSize = stream.ReadUInt32(endian);
			HeaderSize = stream.ReadUInt32(endian);
			ElementCount1 = stream.ReadUInt32(endian);
			ElementCount2 = stream.ReadUInt32(endian);
			Elements1a = new List<Model4Element1a>();
			for (long i = 0; i < ElementCount1; ++i) {
				Elements1a.Add(new Model4Element1a(stream, endian));
			}
			Elements1b = new List<Model4Element1b>();
			for (long i = 0; i < ElementCount1; ++i) {
				Elements1b.Add(new Model4Element1b(stream, endian));
			}
			Elements2 = new List<Model4Element2>();
			for (long i = 0; i < ElementCount2; ++i) {
				Elements2.Add(new Model4Element2(stream, endian));
			}

			return;
		}

		public DuplicatableStream Serialize(int align, EndianUtils.Endianness endian) {
			MemoryStream ms = new MemoryStream();
			ms.WriteUInt32(Magic, endian);
			ms.WriteUInt32(FileSize, endian);
			ms.WriteUInt32(HeaderSize, endian);
			ms.WriteUInt32(ElementCount1, endian);
			ms.WriteUInt32(ElementCount2, endian);
			for (int i = 0; i < ElementCount1; ++i) {
				ms.WriteUInt32(Elements1a[i].ExtraDataCount, endian);
				ms.WriteUInt32(Elements1a[i].Unknown2, endian);
				ms.WriteUInt32(Elements1a[i].Unknown3, endian);
				ms.WriteUInt32(Elements1a[i].Unknown4, endian);
				ms.WriteUInt32(Elements1a[i].Unknown5, endian);
				ms.WriteUInt32(Elements1a[i].Unknown6, endian);
				ms.WriteUInt32(Elements1a[i].Unknown7, endian);
				ms.WriteUInt32(Elements1a[i].Unknown8, endian);
				foreach (uint v in Elements1a[i].ExtraData) {
					ms.WriteUInt32(v, endian);
				}
			}
			long element1bPos = ms.Position;
			for (int i = 0; i < ElementCount1; ++i) {
				ms.WriteUInt32(Elements1b[i].Unknown1, endian);
				for (int j = 0; j < Elements1b[i].Strings.Length; ++j) {
					ms.WriteUInt32(0); // reserve for later
				}
			}
			for (int i = 0; i < ElementCount2; ++i) {
				foreach (uint v in Elements2[i].Data) {
					ms.WriteUInt32(v, endian);
				}
			}

			// strings
			for (int i = 0; i < ElementCount1; ++i) {
				element1bPos += 4;
				for (int j = 0; j < Elements1b[i].Strings.Length; ++j) {
					if (Elements1b[i].Strings[j] != null) {
						long p = ms.Position;
						ms.WriteAsciiNullterm(Elements1b[i].Strings[j]);
						long q = ms.Position;
						ms.Position = element1bPos;
						ms.WriteUInt32((uint)(p - element1bPos), endian);
						ms.Position = q;
					}
					element1bPos += 4;
				}
			}

			ms.WriteAlign(align);
			ms.Position = 0;
			return ms.CopyToByteArrayStreamAndDispose();
		}
	}

	public class Model4Element1a {
		public uint ExtraDataCount;
		public uint Unknown2;
		public uint Unknown3;
		public uint Unknown4;
		public uint Unknown5;
		public uint Unknown6;
		public uint Unknown7;
		public uint Unknown8;
		public uint[] ExtraData;

		public Model4Element1a(Stream stream, EndianUtils.Endianness endian) {
			ExtraDataCount = stream.ReadUInt32(endian);
			Unknown2 = stream.ReadUInt32(endian);
			Unknown3 = stream.ReadUInt32(endian);
			Unknown4 = stream.ReadUInt32(endian);
			Unknown5 = stream.ReadUInt32(endian);
			Unknown6 = stream.ReadUInt32(endian);
			Unknown7 = stream.ReadUInt32(endian);
			Unknown8 = stream.ReadUInt32(endian);
			ExtraData = stream.ReadUInt32Array(ExtraDataCount * 2, endian);
		}
	}
	public class Model4Element1b {
		public uint Unknown1;
		public string[] Strings;

		public Model4Element1b(Stream stream, EndianUtils.Endianness endian) {
			Unknown1 = stream.ReadUInt32(endian);
			Strings = new string[7];
			for (int i = 0; i < 7; ++i) {
				long p = stream.Position;
				uint v = stream.ReadUInt32(endian);
				if (v != 0) {
					Strings[i] = stream.ReadAsciiNulltermFromLocationAndReset(p + v);
				}
			}
		}
	}
	public class Model4Element2 {
		public uint[] Data; // some or maybe all of these are floats

		public Model4Element2(Stream stream, EndianUtils.Endianness endian) {
			Data = stream.ReadUInt32Array(8, endian);
		}
	}
}
