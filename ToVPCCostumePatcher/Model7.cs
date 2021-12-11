using HyoutaPluginBase;
using HyoutaUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToVPCCostumePatcher {
	public class Model7 {
		public uint[] Values; // seem to be floats mostly actually, but this is fine for now

		public Model7(DuplicatableStream stream, EndianUtils.Endianness endian) {
			if ((stream.Length % 4) != 0) {
				throw new Exception("Invalid format");
			}
			Values = stream.ReadUInt32Array(stream.Length / 4, endian);
		}

		public DuplicatableStream Serialize(int align, EndianUtils.Endianness endian) {
			MemoryStream ms = new MemoryStream();
			foreach (uint v in Values) {
				ms.WriteUInt32(v, endian);
			}
			ms.WriteAlign(align);
			ms.Position = 0;
			return ms.CopyToByteArrayStreamAndDispose();
		}
	}
}
