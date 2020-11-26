


using System;
using System.Linq;
using System.Collections.Generic;

using static FM_SoundConvertor.Path;



namespace FM_SoundConvertor {
	public class Directory {
		public static string[] Enum(string Path){
			Path = UnCloak(Path);
			if (System.IO.Directory.Exists(Path)){
				return System.IO.Directory.EnumerateDirectories(Path, "*", System.IO.SearchOption.AllDirectories)
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
			return System.IO.Directory.Exists(Path);
		}
		
		
		
		public static void Delete(string Path){
			try {
				Path = UnCloak(Path);
				System.IO.Directory.Delete(Path, true);
			}
			catch (System.IO.DirectoryNotFoundException){}
		}
		
		
		
		public static System.DateTime Date(string Path){
			Path = UnCloak(Path);
			if (Exist(Path)) return System.IO.Directory.GetLastWriteTime(Path);
			return new System.DateTime(0);
		}
		
		
		
		public static System.DateTime Newer(IEnumerable<string> aPath){
			var aDate = aPath.Select(Path => Date(Path));
			return new System.DateTime(aDate.Max().Ticks);
		}
		
		
		
		public static string Parent(string Path){
			Path = UnCloak(Path);
			return System.IO.Directory.GetParent(Path).FullName;
		}
		
		
		
		public static void Create(string Path){
			Path = UnCloak(Path);
			System.IO.Directory.CreateDirectory(Path);
		}
	}
}
