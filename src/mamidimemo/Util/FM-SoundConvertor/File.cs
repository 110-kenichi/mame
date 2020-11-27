


using System;
using System.Linq;
using System.Collections.Generic;

using static FM_SoundConvertor.Path;



namespace FM_SoundConvertor {
	public class File {
		public static string[] Enum(string Path){
			Path = UnCloak(Path);
			if (System.IO.Directory.Exists(Path)){
				return System.IO.Directory.EnumerateFiles(Path)
					.Select(Entry => Entry.Replace('\\','/'))
					.Where(Entry => (Entry.IndexOf("/.") == -1))
					.OrderBy(Entry => Entry)
					.ToArray();
			}
			return System.Array.Empty<string>();
		}
		
		
		
		public static string[] Enum(string[] aPath){
			var aEnum = new List<string>();
			Array.ForEach(aPath, Path => aEnum.AddRange(Enum(Path)));
			return aEnum.ToArray();
		}
		
		
		
		public static bool Exist(string Path){
			Path = UnCloak(Path);
			return System.IO.File.Exists(Path);
		}
		
		
		
		public static void Delete(string Path){
			try {
				Path = UnCloak(Path);
				System.IO.File.Delete(Path);
			}
			catch (System.IO.DirectoryNotFoundException){}
		}
		
		
		
		public static System.DateTime Date(string Path){
			Path = UnCloak(Path);
			if (Exist(Path)) return System.IO.File.GetLastWriteTime(Path);
			return new System.DateTime(0);
		}
		
		
		
		public static System.DateTime Newer(IEnumerable<string> aPath){
			var aDate = aPath.Select(Path => Date(Path));
			return new System.DateTime(aDate.Max().Ticks);
		}
		
		
		
		public static string[] ReadLine(string Path){
			Path = UnCloak(Path);
			if (Exist(Path)) return System.IO.File.ReadAllLines(Path, System.Text.Encoding.Default);
			return System.Array.Empty<string>();
		}
		
		
		
		public static string ReadText(string Path){
			Path = UnCloak(Path);
			if (Exist(Path)) return System.IO.File.ReadAllText(Path, System.Text.Encoding.Default);
			return System.String.Empty;
		}
		
		
		
		public static byte[] ReadByte(string Path){
			Path = UnCloak(Path);
			if (Exist(Path)) return System.IO.File.ReadAllBytes(Path);
			return System.Array.Empty<byte>();
		}
		
		
		
		public static void WriteLine(string Path, string[] aLine){
			Path = UnCloak(Path);
			System.IO.File.WriteAllLines(Path, aLine, System.Text.Encoding.Default);
		}
		
		
		
		public static void WriteText(string Path, string Text){
			Path = UnCloak(Path);
			System.IO.File.WriteAllText(Path, Text, System.Text.Encoding.Default);
		}
		
		
		
		public static void WriteByte(string Path, byte[] aByte){
			Path = UnCloak(Path);
			System.IO.File.WriteAllBytes(Path, aByte);
		}
		
		
		
		public static long Size(string Path){
			Path = UnCloak(Path);
			if (Exist(Path)) return (new System.IO.FileInfo(Path)).Length;
			return 0;
		}
		
		
		
		public static void Touch(string Path){
			Path = UnCloak(Path);
			System.IO.File.OpenWrite(Path).Close();
			System.IO.File.SetLastWriteTime(Path, System.DateTime.Now);
		}
		
		
		
		public static void Copy(string Src, string Dst){
			Src = UnCloak(Src);
			Dst = UnCloak(Dst);
			if (Exist(Src)) System.IO.File.Copy(Src, Dst, true);
		}
	}
}
