


using System;
using System.Linq;
using System.Collections.Generic;



namespace FM_SoundConvertor {
	public class Path {
		public static string[] Full(string[] aPath){
			return aPath.Select(Path => Full(Path)).ToArray();
		}
		
		
		
		public static string Full(string Path){
			return System.IO.Path.GetFullPath(Path).Replace('\\','/');
		}
		
		
		
		public static string Directory(string Path){
			return System.IO.Path.GetDirectoryName(Path).Replace('\\','/');
		}
		
		
		
		public static string Base(string Path){
			return System.IO.Path.GetFileNameWithoutExtension(Path);
		}
		
		
		
		public static string Name(string Path){
			return System.IO.Path.GetFileName(Path);
		}
		
		
		
		public static string Extension(string Path){
			return System.IO.Path.GetExtension(Path);
		}
		
		
		
		public static string TrimStart(string Path){
			return Path.TrimStart(':','\\','/');
		}
		
		
		
		public static string TrimEnd(string Path){
			return Path.TrimEnd(':','\\','/');
		}
		
		
		
		public static string Trim(string Path){
			return TrimStart(TrimEnd(Path));
		}
		
		
		
		public static string Combine(string Base, string Path){
			return Full(System.IO.Path.Combine(Base, TrimStart(Path)));
		}
		
		
		
		public static string Combine(string Base, string Path, string Extension){
			return string.Concat(Combine(Base, Path), Extension);
		}
		
		
		
		public static string ChangeDirectory(string From, string To, string Path){
			return Combine(To, Path.Substring(From.Length));
		}
		
		
		
		public static string ChangeExtension(string Path, string Extension){
			return Combine(Directory(Path), Base(Path), Extension);
		}
		
		
		
		public static string Cloak(string Path){
			return "\"" + Trim(Path) + "\"";
		}
		
		
		
		public static string UnCloak(string Path){
			return Path.Trim('\"');
		}
	}
}
