using HyoutaPluginBase;
using HyoutaUtils;
using HyoutaTools.Tales.tlzc;
using HyoutaTools.Tales.Vesperia.FPS4;
using System;
using System.Collections.Generic;
using System.IO;
using HyoutaUtils.Streams;

namespace ToVPCCostumePatcher {
	class Program {
		public static (DuplicatableStream data, DuplicatableStream info) BuildYUR_C201(FPS4 chara_svo) {
			// grab other yuri costume as a base
			using var yur200 = new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(chara_svo.GetChildByName("YUR_C200.DAT").AsFile.DataStream.CopyToByteArrayAndDispose())));

			// grab cutscene file that contains this costume
			using var ep1320 = new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(chara_svo.GetChildByName("EP_1320_060.DAT").AsFile.DataStream.CopyToByteArrayAndDispose())));
			using var ep1320_0 = new FPS4(ep1320.GetChildByIndex(0).AsFile.DataStream);

			List<PackFileInfo> yur201_0_files = new List<PackFileInfo>();
			for (int i = 0; i < ep1320_0.Files.Count - 1; ++i) {
				var child = ep1320_0.Files[i];
				if (child.ShouldSkip || child.FileSize == null) {
					throw new Exception("not implemented");
				}
				string path = child.Metadata[0].Value;
				if (path.StartsWith("ENPC")) {
					continue;
				}
				if (path.StartsWith("PC/FRE_C0")) {
					continue;
				}
				if (path.StartsWith("PC/FRE_C2") && !path.Contains("TOWEL")) {
					continue;
				}

				yur201_0_files.Add(new PackFileInfo() {
					Name = child.FileName,
					Length = child.FileSize.Value,
					DataStream = ep1320_0.GetChildByIndex(i).AsFile.DataStream,
					RelativePath = path
				});
			}

			var yur201_0 = new MemoryStream();
			FPS4.Pack(
				yur201_0_files,
				yur201_0,
				ep1320_0.ContentBitmask,
				ep1320_0.Endian,
				ep1320_0.Unknown2,
				null,
				ep1320_0.ArchiveName,
				0,
				0x8,
				metadata: "p",
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: ep1320_0.Files[ep1320_0.Files.Count - 1].Location,
				printProgressToConsole: false
			);
			yur201_0.Position = 0;
			var yur201_0b = yur201_0.CopyToByteArrayStreamAndDispose();

			List<PackFileInfo> yur201_files = new List<PackFileInfo>();
			yur201_files.Add(new PackFileInfo() { Length = yur201_0b.Length, DataStream = yur201_0b });
			yur201_files.Add(new PackFileInfo() { Length = yur200.Files[1].FileSize.Value, DataStream = yur200.GetChildByIndex(1).AsFile.DataStream });
			yur201_files.Add(new PackFileInfo() { Length = yur200.Files[2].FileSize.Value, DataStream = yur200.GetChildByIndex(2).AsFile.DataStream });
			yur201_files.Add(new PackFileInfo() { Length = yur200.Files[3].FileSize.Value, DataStream = yur200.GetChildByIndex(3).AsFile.DataStream });

			var yur201 = new MemoryStream();
			FPS4.Pack(
				yur201_files,
				yur201,
				yur200.ContentBitmask,
				yur200.Endian,
				yur200.Unknown2,
				null,
				"YUR_C201",
				0,
				1,
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: yur200.Files[yur200.Files.Count - 1].Location,
				printProgressToConsole: false
			);

			var info = new ModelInfoFile() {
				Chara = 1,
				Name = "towel",
				Expl = "towel",
				FameId = 49,
				ItemId = 1581,
				File = "YUR_C201.dat",
			};
			info.Chr.Add(("NAME", "YUR_C201"));
			info.Chr.Add(("BASE", "YUR_C000"));
			info.Chr.Add(("BONE", "YUR_C000_BONE"));
			List<string> uniqueFilenames = new List<string>();
			for (int i = 0; i < yur201_0_files.Count; ++i) {
				if (!uniqueFilenames.Contains(yur201_0_files[i].RelativePath)) {
					uniqueFilenames.Add(yur201_0_files[i].RelativePath);
				}
			}
			for (int i = 0; i < uniqueFilenames.Count; ++i) {
				info.Chr.Add((i.ToString(System.Globalization.CultureInfo.InvariantCulture), uniqueFilenames[i]));
			}
			info.Chr.Add(("KK_BONE0", "4124"));
			info.Chr.Add(("KK_GDT0", "KK_RING00_YUR"));

			return (new DuplicatableByteArrayStream(TLZC.Compress(yur201.CopyToByteArrayAndDispose(), 4)), info.GenerateStream());
		}

		public static (DuplicatableStream data, DuplicatableStream info) BuildFRE_C500(FPS4 chara_svo, string data64path, FPS4 fre500_ps3) {
			using var fre500_0_ps3 = new FPS4(fre500_ps3.GetChildByIndex(0).AsFile.DataStream);

			// FRE_C500 is very similar to FRE_C501, so instead of converting all of the PS3 file, use the existing data as best as possible and replace the few differences
			using var fre501 = new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(new DuplicatableFileStream(Path.Combine(data64path, "DLC/DLCDATA/FRE_C501.dat")).CopyToByteArrayAndDispose())));
			using var fre501_0 = new FPS4(fre501.GetChildByIndex(0).AsFile.DataStream);

			List<PackFileInfo> fre500_0_files = new List<PackFileInfo>();
			for (int i = 0; i < fre501_0.Files.Count - 1; ++i) {
				var child = fre501_0.Files[i];
				if (child.ShouldSkip || child.FileSize == null) {
					throw new Exception("not implemented");
				}
				string path = child.Metadata[0].Value;
				fre500_0_files.Add(new PackFileInfo() {
					Name = child.FileName,
					Length = child.FileSize.Value,
					DataStream = fre501_0.GetChildByIndex(i).AsFile.DataStream,
					RelativePath = path
				});
			}
			for (int i = 10; i < 20; ++i) {
				fre500_0_files[i].RelativePath = fre500_0_ps3.Files[i].Metadata[0].Value;
			}

			{
				var fre500_0_hair_4 = new Model4(fre500_0_ps3.GetChildByIndex(14).AsFile.DataStream, EndianUtils.Endianness.BigEndian);
				fre500_0_files[14].DataStream = fre500_0_hair_4.Serialize(0x80, EndianUtils.Endianness.LittleEndian);
				fre500_0_files[14].Length = fre500_0_files[14].DataStream.Length;
			}

			{
				var txm = new HyoutaTools.Tales.Vesperia.Texture.TXM(fre500_0_ps3.GetChildByIndex(18).AsFile.DataStream);
				var txv = new HyoutaTools.Tales.Vesperia.Texture.TXV(txm, fre500_0_ps3.GetChildByIndex(19).AsFile.DataStream, false);
				List<uint> offsets = new List<uint>();
				MemoryStream new_txv = new MemoryStream();
				foreach (HyoutaTools.Tales.Vesperia.Texture.TXVSingle ts in txv.textures) {
					foreach (var tex in ts.GetDiskWritableStreams()) {
						tex.data.Position = 0;
						offsets.Add((uint)new_txv.Position);
						new_txv.WriteUInt32((uint)tex.data.Length, EndianUtils.Endianness.BigEndian);
						StreamUtils.CopyStream(tex.data, new_txv, tex.data.Length);
					}
				}
				new_txv.Position = 0;

				MemoryStream new_txm = fre500_0_ps3.GetChildByIndex(18).AsFile.DataStream.CopyToMemoryAndDispose();
				new_txm.Position = 0x48;
				new_txm.WriteUInt32(offsets[1], EndianUtils.Endianness.BigEndian);
				new_txm.Position = 0;

				fre500_0_files[18].DataStream = new_txm.CopyToByteArrayStreamAndDispose();
				fre500_0_files[18].Length = fre500_0_files[18].DataStream.Length;
				fre500_0_files[19].DataStream = new_txv.CopyToByteArrayStreamAndDispose();
				fre500_0_files[19].Length = fre500_0_files[19].DataStream.Length;
			}

			var fre500_0 = new MemoryStream();
			FPS4.Pack(
				fre500_0_files,
				fre500_0,
				fre501_0.ContentBitmask,
				fre501_0.Endian,
				fre501_0.Unknown2,
				null,
				fre501_0.ArchiveName,
				0,
				0x8,
				metadata: "p",
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: fre501_0.Files[fre501_0.Files.Count - 1].Location,
				printProgressToConsole: false
			);
			fre500_0.Position = 0;
			var fre500_0b = fre500_0.CopyToByteArrayStreamAndDispose();

			List<PackFileInfo> costume_archive_files = new List<PackFileInfo>();
			costume_archive_files.Add(new PackFileInfo() { Length = fre500_0b.Length, DataStream = fre500_0b });
			costume_archive_files.Add(new PackFileInfo() { Length = fre501.Files[1].FileSize.Value, DataStream = fre501.GetChildByIndex(1).AsFile.DataStream });
			costume_archive_files.Add(new PackFileInfo() { Length = fre501.Files[2].FileSize.Value, DataStream = fre501.GetChildByIndex(2).AsFile.DataStream });
			costume_archive_files.Add(new PackFileInfo() { Length = fre501.Files[3].FileSize.Value, DataStream = fre501.GetChildByIndex(3).AsFile.DataStream });

			var costume_archive_stream = new MemoryStream();
			FPS4.Pack(
				costume_archive_files,
				costume_archive_stream,
				fre501.ContentBitmask,
				fre501.Endian,
				fre501.Unknown2,
				null,
				"FRE_C500",
				0,
				4,
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: fre501.Files[fre501.Files.Count - 1].Location,
				printProgressToConsole: false
			);

			var info = new ModelInfoFile() {
				Chara = 8,
				Name = "flynn abyss conversion",
				Expl = "flynn abyss conversion",
				FameId = 391,
				ItemId = 1943,
				File = "FRE_C500.dat",
			};
			info.Chr.Add(("NAME", "FRE_C500"));
			info.Chr.Add(("BASE", "FRE_C000"));
			info.Chr.Add(("BONE", "FRE_C000_BONE"));
			List<string> uniqueFilenames = new List<string>();
			for (int i = 0; i < fre500_0_files.Count; ++i) {
				if (!uniqueFilenames.Contains(fre500_0_files[i].RelativePath)) {
					uniqueFilenames.Add(fre500_0_files[i].RelativePath);
				}
			}
			for (int i = 0; i < uniqueFilenames.Count; ++i) {
				info.Chr.Add((i.ToString(System.Globalization.CultureInfo.InvariantCulture), uniqueFilenames[i]));
			}
			info.Chr.Add(("KK_BONE0", "4536"));
			info.Chr.Add(("KK_GDT0", "W_SWO_FRE_C001"));
			info.Chr.Add(("KK_BONE1", "4535"));
			info.Chr.Add(("KK_GDT1", "W_SWO_F_05_01"));

			return (new DuplicatableByteArrayStream(TLZC.Compress(costume_archive_stream.CopyToByteArrayAndDispose(), 4)), info.GenerateStream());
		}

		private static bool ContainsFile(FPS4 fps4, string filename) {
			for (int i = 0; i < fps4.Files.Count - 1; ++i) {
				if (fps4.Files[i].FileName == filename) {
					return true;
				}
			}
			return false;
		}

		private static void InjectNewFile(List<PackFileInfo> files, string filename, DuplicatableStream stream) {
			var p = new PackFileInfo() {
				Name = filename,
				Length = stream.Length,
				DataStream = stream,
			};
			for (int i = 0; i < files.Count; ++i) {
				if (files[i].Name.CompareTo(filename) > 0) {
					files.Insert(i, p);
					return;
				}
			}
			files.Add(p);
		}

		public static void MakeModifiedCharaSvo(string chara_svo_path, string new_chara_svo_path) {
			bool injectedAnything = false;
			using (var chara_svo = new FPS4(chara_svo_path)) {
				DuplicatableStream yur201 = null;
				if (!ContainsFile(chara_svo, "YUR_C201.DAT")) {
					yur201 = BuildYUR_C201(chara_svo).data;
				}

				List<PackFileInfo> files = new List<PackFileInfo>();
				for (int i = 0; i < chara_svo.Files.Count - 1; ++i) {
					var child = chara_svo.Files[i];
					if (child.ShouldSkip || child.FileSize == null) {
						throw new Exception("not implemented");
					}
					files.Add(new PackFileInfo() {
						Name = child.FileName,
						Length = child.FileSize.Value,
						DataStream = chara_svo.GetChildByIndex(i).AsFile.DataStream,
					});
				}

				if (yur201 != null) {
					InjectNewFile(files, "YUR_C201.DAT", yur201);
					injectedAnything = true;
				}

				if (!injectedAnything) {
					return;
				}

				using (var fs = new FileStream(new_chara_svo_path, FileMode.Create)) {
					FPS4.Pack(
						files,
						fs,
						chara_svo.ContentBitmask,
						chara_svo.Endian,
						chara_svo.Unknown2,
						null,
						chara_svo.ArchiveName,
						0,
						0x80,
						fileLocationMultiplier: chara_svo.FileLocationMultiplier,
						alignmentFirstFile: 0x800,
						setSectorSizeSameAsFileSize: true,
						lastEntryPtrOverride: chara_svo.Files[chara_svo.Files.Count - 1].Location,
						printProgressToConsole: true
					);
				}
			}
		}

		static void Main(string[] args) {
			if (args.Length == 0) {
				Console.WriteLine("Single argument: Path to Data64 directory.");
				Console.WriteLine("Generates custom DLC costume files.");
				return;
			}

			if (args.Length >= 2 && args[0] == "--inject-into-chara.svo") {
				MakeModifiedCharaSvo(args[1], args[1] + ".new");
				return;
			}

			string data64path = args[0];
			string ps3dlcpath = args.Length >= 2 ? args[1] : null;
			using (var chara_svo = new FPS4(Path.Combine(data64path, "chara.svo"))) {
				var yur201 = BuildYUR_C201(chara_svo);
				using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCDATA/YUR_C201.dat"), FileMode.Create)) {
					StreamUtils.CopyStream(yur201.data, fs);
				}
				using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCINFO/YUR_C201.dat"), FileMode.Create)) {
					StreamUtils.CopyStream(yur201.info, fs);
				}

				if (ps3dlcpath != null) {
					string fre_c500_path = Path.Combine(ps3dlcpath, "FRE_C500.edat.unedat");
					if (File.Exists(fre_c500_path)) {
						var fre500 = BuildFRE_C500(chara_svo, data64path, new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(new DuplicatableFileStream(fre_c500_path).CopyToByteArrayAndDispose()))));
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCDATA/FRE_C500.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(fre500.data, fs);
						}
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCINFO/FRE_C500.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(fre500.info, fs);
						}
					}
				}
			}
		}
	}
}
