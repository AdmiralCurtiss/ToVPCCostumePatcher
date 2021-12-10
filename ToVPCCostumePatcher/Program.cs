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
		static DuplicatableStream BuildYUR_C201(FPS4 chara_svo) {
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
				printProgressToConsole: true
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
				printProgressToConsole: true
			);
			return new DuplicatableByteArrayStream(TLZC.Compress(yur201.CopyToByteArrayAndDispose(), 2));
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

		static void Main(string[] args) {
			bool injectedAnything = false;
			using (var chara_svo = new FPS4(@"c:\__tov\chara.svo")) {
				DuplicatableStream yur201 = null;
				if (!ContainsFile(chara_svo, "YUR_C201.DAT")) {
					yur201 = BuildYUR_C201(chara_svo);
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

				using (var fs = new FileStream(@"c:\__tov\chara_new.svo", FileMode.Create)) {
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
	}
}
