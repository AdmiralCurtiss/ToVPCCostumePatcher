using HyoutaPluginBase;
using HyoutaUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToVPCCostumePatcher {
	public class ModelInfoFile {
		public int Chara;
		public string Name;
		public string Expl;
		public int FameId;
		public int ItemId;
		public string File;
		public List<(string key, string val)> Chr = new List<(string key, string val)>();

		public DuplicatableStream GenerateStream() {
			MemoryStream ms = new MemoryStream();
			ms.Write(Encoding.UTF8.GetBytes("FORMAT=MODEL\r\n"));
			ms.Write(Encoding.UTF8.GetBytes(string.Format(System.Globalization.CultureInfo.InvariantCulture, "CHARA={0}\r\n", Chara)));
			foreach (string lang in new string[] { "JPN", "ENG", "FRA", "ITA", "DEU", "ESP", "ESN", "BRA", "RUS", "ZHT", "KOR" }) {
				ms.Write(Encoding.UTF8.GetBytes(string.Format(System.Globalization.CultureInfo.InvariantCulture, "NAME_{0}={1}\r\n", lang, Name)));
				ms.Write(Encoding.UTF8.GetBytes(string.Format(System.Globalization.CultureInfo.InvariantCulture, "EXPL_{0}={1}\r\n", lang, Expl)));
			}
			ms.Write(Encoding.UTF8.GetBytes(string.Format(System.Globalization.CultureInfo.InvariantCulture, "FAME_ID={0}\r\n", FameId)));
			ms.Write(Encoding.UTF8.GetBytes(string.Format(System.Globalization.CultureInfo.InvariantCulture, "ITEM_ID={0}\r\n", ItemId)));
			ms.Write(Encoding.UTF8.GetBytes(string.Format(System.Globalization.CultureInfo.InvariantCulture, "FILE={0}\r\n", File)));
			foreach (var chr in Chr) {
				ms.Write(Encoding.UTF8.GetBytes(string.Format(System.Globalization.CultureInfo.InvariantCulture, "CHR_{0}={1}\r\n", chr.key, chr.val)));
			}
			ms.Position = 0;
			return ms.CopyToByteArrayStreamAndDispose();
		}
	}
}
