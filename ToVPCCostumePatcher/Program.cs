using HyoutaPluginBase;
using HyoutaUtils;
using HyoutaTools.Tales.tlzc;
using HyoutaTools.Tales.Vesperia.FPS4;
using System;
using System.Collections.Generic;
using System.IO;
using HyoutaUtils.Streams;
using System.Linq;

namespace ToVPCCostumePatcher {
	class Program {
		public static (DuplicatableStream data, DuplicatableStream info) BuildYUR_C201(FPS4 chara_svo) {
			Console.WriteLine("Building YUR_C201...");

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

			DetectDuplicateFiles(ref yur201_0_files);

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
			DetectDuplicateFiles(ref yur201_files);

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

		public static (DuplicatableStream data, DuplicatableStream info) BuildFRE_C500(FPS4 chara_svo, string data64path, FPS4 fre500_ps3, string texreplacepath) {
			Console.WriteLine("Building FRE_C500...");

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

			ConvertModelPart4(fre500_0_ps3, fre500_0_files, 14, 14);
			ConvertTxmTxv(fre500_0_ps3, fre500_0_files, 18, 19, 18, 19, texreplacepath);

			DetectDuplicateFiles(ref fre500_0_files);

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
			DetectDuplicateFiles(ref costume_archive_files);

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

		private static void ConvertModelPart4(FPS4 ps3fps4, List<PackFileInfo> pcfiles, int idx_ps3, int idx_pc) {
			var m4 = new Model4(ps3fps4.GetChildByIndex(idx_ps3).AsFile.DataStream, EndianUtils.Endianness.BigEndian);
			pcfiles[idx_pc].DataStream = m4.Serialize(0x80, EndianUtils.Endianness.LittleEndian);
			pcfiles[idx_pc].Length = pcfiles[idx_pc].DataStream.Length;
		}

		private static void ConvertModelPart7(FPS4 ps3fps4, List<PackFileInfo> pcfiles, int idx_ps3, int idx_pc) {
			var m7 = new Model7(ps3fps4.GetChildByIndex(idx_ps3).AsFile.DataStream, EndianUtils.Endianness.BigEndian);
			pcfiles[idx_pc].DataStream = m7.Serialize(0x80, EndianUtils.Endianness.LittleEndian);
			pcfiles[idx_pc].Length = pcfiles[idx_pc].DataStream.Length;
		}

		private static void FakeConvertModelPart6(FPS4 ps3fps4, List<PackFileInfo> pcfiles, int idx_ps3, int idx_pc, uint string_block_start) {
			MemoryStream ms = ps3fps4.GetChildByIndex(idx_ps3).AsFile.DataStream.CopyToMemoryAndDispose();
			FakeConvertModelPart6BigToLittle(ms, string_block_start);
			ms.Position = 0;
			pcfiles[idx_pc].DataStream = ms.CopyToByteArrayStreamAndDispose();
			pcfiles[idx_pc].Length = pcfiles[idx_pc].DataStream.Length;
		}

		private static void FakeConvertModelPart6BigToLittle(MemoryStream ms, uint string_block_start) {
			ms.Position = 0xc;
			uint count = ms.ReadUInt32(EndianUtils.Endianness.BigEndian);

			List<uint> b16_offsets = new List<uint>();
			for (int i = 0; i < count; ++i) {
				ms.Position = 0x24 + 0x30 * count + 0x3c * i + 0x18;
				uint p = (uint)ms.Position;
				b16_offsets.Add(p + ms.ReadUInt32(EndianUtils.Endianness.BigEndian));
			}
			b16_offsets.Add(string_block_start);

			uint cutoff_32_16 = b16_offsets[0];
			ms.Position = 0;
			for (uint i = 0; i < cutoff_32_16; i += 4) {
				uint v = ms.PeekUInt32(EndianUtils.Endianness.BigEndian);
				ms.WriteUInt32(v, EndianUtils.Endianness.LittleEndian);
			}
			for (int i = 1; i < b16_offsets.Count; ++i) {
				uint start = b16_offsets[i - 1] + 4;
				uint end = b16_offsets[i];
				uint init = ms.PeekUInt32(EndianUtils.Endianness.BigEndian);
				ms.WriteUInt32(init, EndianUtils.Endianness.LittleEndian);
				for (uint j = start; j < end; j += 2) {
					ushort v = ms.PeekUInt16(EndianUtils.Endianness.BigEndian);
					ms.WriteUInt16(v, EndianUtils.Endianness.LittleEndian);
				}
			}
		}

		private static void FakeConvertModelPart0(FPS4 ps3fps4, List<PackFileInfo> pcfiles, int idx_ps3, int idx_pc, uint[] swap16s) {
			MemoryStream ms = ps3fps4.GetChildByIndex(idx_ps3).AsFile.DataStream.CopyToMemoryAndDispose();
			ByteSwap32With16Exceptions(ms, swap16s);
			ms.Position = 0;
			pcfiles[idx_pc].DataStream = ms.CopyToByteArrayStreamAndDispose();
			pcfiles[idx_pc].Length = pcfiles[idx_pc].DataStream.Length;
		}

		private static void ByteSwap32With16Exceptions(MemoryStream ms, uint[] swap16s) {
			var swap16set = swap16s.ToHashSet();
			long len = ms.Length;
			for (uint i = 0; i < len; i += 4) {
				if (swap16set.Contains(i)) {
					ushort v0 = ms.PeekUInt16(EndianUtils.Endianness.BigEndian);
					ms.WriteUInt16(v0, EndianUtils.Endianness.LittleEndian);
					ushort v1 = ms.PeekUInt16(EndianUtils.Endianness.BigEndian);
					ms.WriteUInt16(v1, EndianUtils.Endianness.LittleEndian);
				} else {
					uint v = ms.PeekUInt32(EndianUtils.Endianness.BigEndian);
					ms.WriteUInt32(v, EndianUtils.Endianness.LittleEndian);
				}
			}
		}

		private static void FakeConvertModelPart3(FPS4 ps3fps4, List<PackFileInfo> pcfiles, int idx_ps3, int idx_pc, uint swapstopstart, uint swapstopend) {
			MemoryStream ms = ps3fps4.GetChildByIndex(idx_ps3).AsFile.DataStream.CopyToMemoryAndDispose();
			ByteSwap32WithoutRange(ms, swapstopstart, swapstopend);
			ms.Position = 0;
			pcfiles[idx_pc].DataStream = ms.CopyToByteArrayStreamAndDispose();
			pcfiles[idx_pc].Length = pcfiles[idx_pc].DataStream.Length;
		}

		private static void ByteSwap32WithoutRange(MemoryStream ms, uint swapstopstart, uint swapstopend) {
			long len = ms.Length;
			for (uint i = 0; i < len; i += 4) {
				if (i >= swapstopstart && i < swapstopend) {
					ms.Position += 4;
				} else {
					uint v = ms.PeekUInt32(EndianUtils.Endianness.BigEndian);
					ms.WriteUInt32(v, EndianUtils.Endianness.LittleEndian);
				}
			}
		}

		private static void ConvertTxmTxv(FPS4 ps3fps4, List<PackFileInfo> pcfiles, int txmidx_ps3, int txvidx_ps3, int txmidx_pc, int txvidx_pc, string replacefolder) {
			var txm = new HyoutaTools.Tales.Vesperia.Texture.TXM(ps3fps4.GetChildByIndex(txmidx_ps3).AsFile.DataStream);
			var txv = new HyoutaTools.Tales.Vesperia.Texture.TXV(txm, ps3fps4.GetChildByIndex(txvidx_ps3).AsFile.DataStream, false);

			List<byte[]> textures = new List<byte[]>();
			foreach (HyoutaTools.Tales.Vesperia.Texture.TXVSingle ts in txv.textures) {
				foreach (var tex in ts.GetDiskWritableStreams()) {
					Console.WriteLine("Converting " + tex.name + "...");

					byte[] replace = null;
					if (replacefolder != null) {
						string inpath = Path.Combine(replacefolder, tex.name);
						if (File.Exists(inpath)) {
							Console.WriteLine("Injecting " + inpath + "...");
							replace = File.ReadAllBytes(inpath);
						} else {
							Console.WriteLine("No file at " + inpath + ", using original texture.");
						}
					}

					if (replace != null) {
						MemoryStream ms = new MemoryStream();
						ms.WriteUInt32((uint)replace.Length, EndianUtils.Endianness.BigEndian);
						ms.Write(replace);
						textures.Add(ms.CopyToByteArrayAndDispose());
					} else {
						MemoryStream ms = new MemoryStream();
						ms.WriteUInt32((uint)tex.data.Length, EndianUtils.Endianness.BigEndian);
						tex.data.Position = 0;
						StreamUtils.CopyStream(tex.data, ms, tex.data.Length);
						textures.Add(ms.CopyToByteArrayAndDispose());
					}
				}
			}

			List<uint> offsets = new List<uint>();
			MemoryStream new_txv = new MemoryStream();
			for (int i = 0; i < textures.Count; ++i) {
				byte[] tex = textures[i];
				bool skip = false;
				for (int j = 0; j < i; ++j) {
					if (textures[j].Length == tex.Length && ArrayUtils.IsByteArrayPartEqual(textures[j], 0, tex, 0, tex.Length)) {
						// dupe texture, only write once
						offsets.Add(offsets[j]);
						skip = true;
						break;
					}
				}
				if (!skip) {
					offsets.Add((uint)new_txv.Position);
					new_txv.Write(tex);
				}
			}
			new_txv.Position = 0;

			MemoryStream new_txm = ps3fps4.GetChildByIndex(txmidx_ps3).AsFile.DataStream.CopyToMemoryAndDispose();
			for (int i = 0; i < offsets.Count; ++i) {
				new_txm.Position = 0x2c + i * 0x1c;
				new_txm.WriteUInt32(offsets[i], EndianUtils.Endianness.BigEndian);
			}
			new_txm.Position = 0;

			pcfiles[txmidx_pc].DataStream = new_txm.CopyToByteArrayStreamAndDispose();
			pcfiles[txmidx_pc].Length = pcfiles[txmidx_pc].DataStream.Length;
			pcfiles[txvidx_pc].DataStream = new_txv.CopyToByteArrayStreamAndDispose();
			pcfiles[txvidx_pc].Length = pcfiles[txvidx_pc].DataStream.Length;
		}

		private static void DetectDuplicateFiles(ref List<PackFileInfo> costume_archive_files) {
			costume_archive_files = FPS4.DetectDuplicates(costume_archive_files);
		}

		public static (DuplicatableStream data, DuplicatableStream info) BuildKAR_C210(FPS4 chara_svo, string data64path, FPS4 kar210_ps3, string texreplacepath) {
			Console.WriteLine("Building KAR_C210...");

			using var kar210_0_ps3 = new FPS4(kar210_ps3.GetChildByIndex(0).AsFile.DataStream);
			using var kar101 = new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(chara_svo.GetChildByName("KAR_C101.DAT").AsFile.DataStream.CopyToByteArrayAndDispose())));
			using var kar101_0 = new FPS4(kar101.GetChildByIndex(0).AsFile.DataStream);

			List<PackFileInfo> kar210_0_files = new List<PackFileInfo>();
			for (int j = 0; j < 2; ++j) {
				for (int i = 60; i < 70; ++i) {
					var child = kar101_0.Files[i];
					if (child.ShouldSkip || child.FileSize == null) {
						throw new Exception("not implemented");
					}
					string path = child.Metadata[0].Value;
					kar210_0_files.Add(new PackFileInfo() {
						Name = child.FileName,
						Length = child.FileSize.Value,
						DataStream = kar101_0.GetChildByIndex(i).AsFile.DataStream,
						RelativePath = path
					});
				}
			}
			for (int i = 0; i < 10; ++i) {
				kar210_0_files[i].RelativePath = kar210_0_ps3.Files[i].Metadata[0].Value;
			}
			for (int i = 10; i < 20; ++i) {
				kar210_0_files[i].RelativePath = kar210_0_ps3.Files[i].Metadata[0].Value;
			}

			ConvertModelPart4(kar210_0_ps3, kar210_0_files, 4, 4);
			ConvertModelPart4(kar210_0_ps3, kar210_0_files, 14, 14);
			ConvertModelPart7(kar210_0_ps3, kar210_0_files, 7, 7);
			ConvertModelPart7(kar210_0_ps3, kar210_0_files, 17, 17);
			ConvertTxmTxv(kar210_0_ps3, kar210_0_files, 8, 9, 8, 9, texreplacepath);
			ConvertTxmTxv(kar210_0_ps3, kar210_0_files, 18, 19, 18, 19, texreplacepath);
			FakeConvertModelPart6(kar210_0_ps3, kar210_0_files, 6, 6, 0x6b9c);
			FakeConvertModelPart6(kar210_0_ps3, kar210_0_files, 16, 16, 0x12ba0);
			FakeConvertModelPart0(kar210_0_ps3, kar210_0_files, 0, 0, new uint[] { 0x128, 0x15c, 0x174, 0x190, 0x1a8, 0x1b0 });
			FakeConvertModelPart3(kar210_0_ps3, kar210_0_files, 3, 3, 0x3bc, 0x460);

			DetectDuplicateFiles(ref kar210_0_files);

			var kar210_0 = new MemoryStream();
			FPS4.Pack(
				kar210_0_files,
				kar210_0,
				kar101_0.ContentBitmask,
				kar101_0.Endian,
				kar101_0.Unknown2,
				null,
				kar101_0.ArchiveName,
				0,
				0x8,
				metadata: "p",
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: kar101_0.Files[kar101_0.Files.Count - 1].Location,
				printProgressToConsole: false
			);
			kar210_0.Position = 0;
			var kar210_0b = kar210_0.CopyToByteArrayStreamAndDispose();

			List<PackFileInfo> costume_archive_files = new List<PackFileInfo>();
			costume_archive_files.Add(new PackFileInfo() { Length = kar210_0b.Length, DataStream = kar210_0b });
			costume_archive_files.Add(new PackFileInfo() { Length = kar101.Files[1].FileSize.Value, DataStream = kar101.GetChildByIndex(1).AsFile.DataStream });
			costume_archive_files.Add(new PackFileInfo() { Length = kar101.Files[2].FileSize.Value, DataStream = kar101.GetChildByIndex(2).AsFile.DataStream });
			costume_archive_files.Add(new PackFileInfo() { Length = kar101.Files[3].FileSize.Value, DataStream = kar101.GetChildByIndex(3).AsFile.DataStream });
			DetectDuplicateFiles(ref costume_archive_files);

			var costume_archive_stream = new MemoryStream();
			FPS4.Pack(
				costume_archive_files,
				costume_archive_stream,
				kar101.ContentBitmask,
				kar101.Endian,
				kar101.Unknown2,
				null,
				"KAR_C210",
				0,
				4,
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: kar101.Files[kar101.Files.Count - 1].Location,
				printProgressToConsole: false
			);

			var info = new ModelInfoFile() {
				Chara = 3,
				Name = "keroro conversion",
				Expl = "keroro conversion",
				FameId = 141,
				ItemId = 1593,
				File = "KAR_C210.dat",
			};
			info.Chr.Add(("NAME", "KAR_C210"));
			info.Chr.Add(("BASE", "KAR_C000"));
			info.Chr.Add(("BONE", "KAR_C000_BONE"));
			List<string> uniqueFilenames = new List<string>();
			for (int i = 0; i < kar210_0_files.Count; ++i) {
				if (!uniqueFilenames.Contains(kar210_0_files[i].RelativePath)) {
					uniqueFilenames.Add(kar210_0_files[i].RelativePath);
				}
			}
			for (int i = 0; i < uniqueFilenames.Count; ++i) {
				info.Chr.Add((i.ToString(System.Globalization.CultureInfo.InvariantCulture), uniqueFilenames[i]));
			}

			return (new DuplicatableByteArrayStream(TLZC.Compress(costume_archive_stream.CopyToByteArrayAndDispose(), 4)), info.GenerateStream());
		}

		public static (DuplicatableStream data, DuplicatableStream info) BuildEST_C500(FPS4 chara_svo, string data64path, FPS4 est500_ps3, string texreplacepath) {
			Console.WriteLine("Building EST_C500...");

			using var est500_0_ps3 = new FPS4(est500_ps3.GetChildByIndex(0).AsFile.DataStream);
			using var est000 = new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(chara_svo.GetChildByName("EST_C000.DAT").AsFile.DataStream.CopyToByteArrayAndDispose())));
			using var est000_0 = new FPS4(est000.GetChildByIndex(0).AsFile.DataStream);
			using var est501 = new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(new DuplicatableFileStream(Path.Combine(data64path, "DLC/DLCDATA/EST_C501.dat")).CopyToByteArrayAndDispose())));
			using var est501_0 = new FPS4(est501.GetChildByIndex(0).AsFile.DataStream);

			List<PackFileInfo> est500_0_files = new List<PackFileInfo>();
			for (int i = 0; i < 10; ++i) {
				var child = est000_0.Files[i];
				if (child.ShouldSkip || child.FileSize == null) {
					throw new Exception("not implemented");
				}
				string path = child.Metadata[0].Value;
				est500_0_files.Add(new PackFileInfo() {
					Name = child.FileName,
					Length = child.FileSize.Value,
					DataStream = est000_0.GetChildByIndex(i).AsFile.DataStream,
					RelativePath = path
				});
			}
			for (int i = 60; i < 70; ++i) {
				var child = est501_0.Files[i];
				if (child.ShouldSkip || child.FileSize == null) {
					throw new Exception("not implemented");
				}
				string path = child.Metadata[0].Value;
				est500_0_files.Add(new PackFileInfo() {
					Name = child.FileName,
					Length = child.FileSize.Value,
					DataStream = est501_0.GetChildByIndex(i).AsFile.DataStream,
					RelativePath = path
				});
			}
			for (int i = 10; i < 60; ++i) {
				var child = est501_0.Files[i];
				if (child.ShouldSkip || child.FileSize == null) {
					throw new Exception("not implemented");
				}
				string path = child.Metadata[0].Value;
				est500_0_files.Add(new PackFileInfo() {
					Name = child.FileName,
					Length = child.FileSize.Value,
					DataStream = est501_0.GetChildByIndex(i).AsFile.DataStream,
					RelativePath = path
				});
			}
			for (int i = 10; i < 20; ++i) {
				est500_0_files[i].RelativePath = est500_0_ps3.Files[i].Metadata[0].Value;
			}

			ConvertModelPart4(est500_0_ps3, est500_0_files, 14, 14);
			ConvertTxmTxv(est500_0_ps3, est500_0_files, 18, 19, 18, 19, texreplacepath);
			FakeConvertModelPart0(est500_0_ps3, est500_0_files, 10, 10, new uint[] { 0x194, 0x1c8, 0x1fc, 0x230, 0x280 });
			FakeConvertModelPart3(est500_0_ps3, est500_0_files, 13, 13, 0x58c, 0x660);
			FakeConvertModelPart6(est500_0_ps3, est500_0_files, 16, 16, 0x8668);

			DetectDuplicateFiles(ref est500_0_files);

			var est500_0 = new MemoryStream();
			FPS4.Pack(
				est500_0_files,
				est500_0,
				est501_0.ContentBitmask,
				est501_0.Endian,
				est501_0.Unknown2,
				null,
				est501_0.ArchiveName,
				0,
				0x8,
				metadata: "p",
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: est501_0.Files[est501_0.Files.Count - 1].Location,
				printProgressToConsole: false
			);
			est500_0.Position = 0;
			var est500_0b = est500_0.CopyToByteArrayStreamAndDispose();

			List<PackFileInfo> costume_archive_files = new List<PackFileInfo>();
			costume_archive_files.Add(new PackFileInfo() { Length = est500_0b.Length, DataStream = est500_0b });
			costume_archive_files.Add(new PackFileInfo() { Length = est501.Files[1].FileSize.Value, DataStream = est501.GetChildByIndex(1).AsFile.DataStream });
			costume_archive_files.Add(new PackFileInfo() { Length = est501.Files[2].FileSize.Value, DataStream = est501.GetChildByIndex(2).AsFile.DataStream });
			costume_archive_files.Add(new PackFileInfo() { Length = est501.Files[3].FileSize.Value, DataStream = est501.GetChildByIndex(3).AsFile.DataStream });
			DetectDuplicateFiles(ref costume_archive_files);

			var costume_archive_stream = new MemoryStream();
			FPS4.Pack(
				costume_archive_files,
				costume_archive_stream,
				est501.ContentBitmask,
				est501.Endian,
				est501.Unknown2,
				null,
				"EST_C500",
				0,
				4,
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: est501.Files[est501.Files.Count - 1].Location,
				printProgressToConsole: false
			);

			var info = new ModelInfoFile() {
				Chara = 2,
				Name = "estelle abyss conversion",
				Expl = "estelle abyss conversion",
				FameId = 91,
				ItemId = 1583,
				File = "EST_C500.dat",
			};
			info.Chr.Add(("NAME", "EST_C500"));
			info.Chr.Add(("BASE", "EST_C000"));
			info.Chr.Add(("BONE", "EST_C000_BONE"));
			List<string> uniqueFilenames = new List<string>();
			for (int i = 0; i < est500_0_files.Count; ++i) {
				if (!uniqueFilenames.Contains(est500_0_files[i].RelativePath)) {
					uniqueFilenames.Add(est500_0_files[i].RelativePath);
				}
			}
			for (int i = 0; i < uniqueFilenames.Count; ++i) {
				info.Chr.Add((i.ToString(System.Globalization.CultureInfo.InvariantCulture), uniqueFilenames[i]));
			}
			info.Chr.Add(("KK_BONE0", "4124"));
			info.Chr.Add(("KK_GDT0", "KK_RING00_EST"));

			return (new DuplicatableByteArrayStream(TLZC.Compress(costume_archive_stream.CopyToByteArrayAndDispose(), 4)), info.GenerateStream());
		}

		public static (DuplicatableStream data, DuplicatableStream info) BuildYUR_C500(FPS4 chara_svo, string data64path, FPS4 yur500_ps3, string texreplacepath) {
			Console.WriteLine("Building YUR_C500...");

			using var yur500_0_ps3 = new FPS4(yur500_ps3.GetChildByIndex(0).AsFile.DataStream);
			using var yur501 = new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(new DuplicatableFileStream(Path.Combine(data64path, "DLC/DLCDATA/YUR_C501.dat")).CopyToByteArrayAndDispose())));
			using var yur501_0 = new FPS4(yur501.GetChildByIndex(0).AsFile.DataStream);

			List<PackFileInfo> yur500_0_files = new List<PackFileInfo>();
			for (int j = 0; j < 2; ++j) {
				for (int i = 0; i < 40; ++i) {
					if ((i >= 10 && i < 30) == (j == 0)) continue;
					var child = yur501_0.Files[i];
					if (child.ShouldSkip || child.FileSize == null) {
						throw new Exception("not implemented");
					}
					string path = child.Metadata[0].Value;
					yur500_0_files.Add(new PackFileInfo() {
						Name = child.FileName,
						Length = child.FileSize.Value,
						DataStream = yur501_0.GetChildByIndex(i).AsFile.DataStream,
						RelativePath = path
					});
				}
			}
			for (int i = 10; i < 20; ++i) {
				yur500_0_files[i].RelativePath = yur500_0_ps3.Files[i].Metadata[0].Value;
			}
			for (int i = 30; i < 40; ++i) {
				yur500_0_files[i].RelativePath = yur500_0_ps3.Files[i].Metadata[0].Value;
			}

			FakeConvertModelPart0(yur500_0_ps3, yur500_0_files, 10, 10, new uint[] { 0x164, 0x198, 0x1b8, 0x210 });
			ConvertModelPart4(yur500_0_ps3, yur500_0_files, 14, 14);
			ConvertModelPart4(yur500_0_ps3, yur500_0_files, 34, 34);
			FakeConvertModelPart6(yur500_0_ps3, yur500_0_files, 16, 16, 0x1c48);
			FakeConvertModelPart6(yur500_0_ps3, yur500_0_files, 36, 36, 0x7d70);
			ConvertModelPart7(yur500_0_ps3, yur500_0_files, 17, 17);
			ConvertTxmTxv(yur500_0_ps3, yur500_0_files, 18, 19, 18, 19, texreplacepath);
			ConvertTxmTxv(yur500_0_ps3, yur500_0_files, 38, 39, 38, 39, texreplacepath);

			DetectDuplicateFiles(ref yur500_0_files);

			var yur500_0 = new MemoryStream();
			FPS4.Pack(
				yur500_0_files,
				yur500_0,
				yur501_0.ContentBitmask,
				yur501_0.Endian,
				yur501_0.Unknown2,
				null,
				yur501_0.ArchiveName,
				0,
				0x8,
				metadata: "p",
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: yur501_0.Files[yur501_0.Files.Count - 1].Location,
				printProgressToConsole: false
			);
			yur500_0.Position = 0;
			var yur500_0b = yur500_0.CopyToByteArrayStreamAndDispose();

			List<PackFileInfo> costume_archive_files = new List<PackFileInfo>();
			costume_archive_files.Add(new PackFileInfo() { Length = yur500_0b.Length, DataStream = yur500_0b });
			costume_archive_files.Add(new PackFileInfo() { Length = yur501.Files[1].FileSize.Value, DataStream = yur501.GetChildByIndex(1).AsFile.DataStream });
			costume_archive_files.Add(new PackFileInfo() { Length = yur501.Files[2].FileSize.Value, DataStream = yur501.GetChildByIndex(2).AsFile.DataStream });
			costume_archive_files.Add(new PackFileInfo() { Length = yur501.Files[3].FileSize.Value, DataStream = yur501.GetChildByIndex(3).AsFile.DataStream });
			DetectDuplicateFiles(ref costume_archive_files);

			var costume_archive_stream = new MemoryStream();
			FPS4.Pack(
				costume_archive_files,
				costume_archive_stream,
				yur501.ContentBitmask,
				yur501.Endian,
				yur501.Unknown2,
				null,
				"YUR_C500",
				0,
				4,
				alignmentFirstFile: 0x80,
				setSectorSizeSameAsFileSize: true,
				lastEntryPtrOverride: yur501.Files[yur501.Files.Count - 1].Location,
				printProgressToConsole: false
			);

			var info = new ModelInfoFile() {
				Chara = 1,
				Name = "yuri abyss conversion",
				Expl = "yuri abyss conversion",
				FameId = 41,
				ItemId = 1573,
				File = "YUR_C500.dat",
			};
			info.Chr.Add(("NAME", "YUR_C500"));
			info.Chr.Add(("BASE", "YUR_C000"));
			info.Chr.Add(("BONE", "YUR_C000_BONE"));
			List<string> uniqueFilenames = new List<string>();
			for (int i = 0; i < yur500_0_files.Count; ++i) {
				if (!uniqueFilenames.Contains(yur500_0_files[i].RelativePath)) {
					uniqueFilenames.Add(yur500_0_files[i].RelativePath);
				}
			}
			for (int i = 0; i < uniqueFilenames.Count; ++i) {
				info.Chr.Add((i.ToString(System.Globalization.CultureInfo.InvariantCulture), uniqueFilenames[i]));
			}
			info.Chr.Add(("KK_BONE0", "4124"));
			info.Chr.Add(("KK_GDT0", "KK_RING00_YUR"));

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
					DetectDuplicateFiles(ref files);
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
				Console.WriteLine("Generates custom DLC costume files.");
				Console.WriteLine();
				Console.WriteLine("1st argument: Path to Data64 directory.");
				Console.WriteLine("2nd argument (optional): Path to DLCDATA directory of PS3 version. Files should be already decrypted, and named like 'KAR_C210.edat.unedat'.");
				Console.WriteLine("3rd argument (optional): Folder containing replacement textures.");
				return;
			}

			if (args.Length >= 2 && args[0] == "--inject-into-chara.svo") {
				MakeModifiedCharaSvo(args[1], args[1] + ".new");
				return;
			}

			string data64path = args[0];
			string ps3dlcpath = args.Length >= 2 ? args[1] : null;
			string texreplacepath = args.Length >= 3 ? args[2] : null;
			using (var chara_svo = new FPS4(Path.Combine(data64path, "chara.svo"))) {
				var yur201 = BuildYUR_C201(chara_svo);
				using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCDATA/YUR_C201.dat"), FileMode.Create)) {
					StreamUtils.CopyStream(yur201.data, fs);
				}
				using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCINFO/YUR_C201.dat"), FileMode.Create)) {
					StreamUtils.CopyStream(yur201.info, fs);
				}

				if (ps3dlcpath != null) {
					string est_c500_path = Path.Combine(ps3dlcpath, "EST_C500.edat.unedat");
					if (File.Exists(est_c500_path)) {
						var est500 = BuildEST_C500(chara_svo, data64path, new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(new DuplicatableFileStream(est_c500_path).CopyToByteArrayAndDispose()))), texreplacepath);
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCDATA/EST_C500.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(est500.data, fs);
						}
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCINFO/EST_C500.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(est500.info, fs);
						}
					}

					string fre_c500_path = Path.Combine(ps3dlcpath, "FRE_C500.edat.unedat");
					if (File.Exists(fre_c500_path)) {
						var fre500 = BuildFRE_C500(chara_svo, data64path, new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(new DuplicatableFileStream(fre_c500_path).CopyToByteArrayAndDispose()))), texreplacepath);
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCDATA/FRE_C500.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(fre500.data, fs);
						}
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCINFO/FRE_C500.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(fre500.info, fs);
						}
					}

					string yur_c500_path = Path.Combine(ps3dlcpath, "YUR_C500.edat.unedat");
					if (File.Exists(yur_c500_path)) {
						var yur500 = BuildYUR_C500(chara_svo, data64path, new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(new DuplicatableFileStream(yur_c500_path).CopyToByteArrayAndDispose()))), texreplacepath);
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCDATA/YUR_C500.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(yur500.data, fs);
						}
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCINFO/YUR_C500.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(yur500.info, fs);
						}
					}

					string kar_c210_path = Path.Combine(ps3dlcpath, "KAR_C210.edat.unedat");
					if (File.Exists(kar_c210_path)) {
						var kar210 = BuildKAR_C210(chara_svo, data64path, new FPS4(new DuplicatableByteArrayStream(TLZC.Decompress(new DuplicatableFileStream(kar_c210_path).CopyToByteArrayAndDispose()))), texreplacepath);
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCDATA/KAR_C210.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(kar210.data, fs);
						}
						using (var fs = new FileStream(Path.Combine(data64path, "DLC/DLCINFO/KAR_C210.dat"), FileMode.Create)) {
							StreamUtils.CopyStream(kar210.info, fs);
						}
					}
				}
			}
		}
	}
}
