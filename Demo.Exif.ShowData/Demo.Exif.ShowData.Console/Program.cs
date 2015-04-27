using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExifNET;

namespace Demo.Exif.ShowData.Console
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var files = GetFiles();
			foreach (var file in files)
			{
				System.Console.WriteLine("========== File: {0} ==========", file);
				DisplayFileExif(file);
			}
		}

		private static void DisplayFileExif(string file)
		{
			using (var image = Image.FromFile(file))
			{
				var exif = new ExifNET.Exif(image.PropertyItems);
				DisplayExif(exif);
			}
		}

		private static void DisplayExif(ExifNET.Exif exif)
		{
			foreach (var property in exif.GetType().GetProperties())
			{
				System.Console.WriteLine("\t{0}={1}", property.Name, property.GetValue(exif, null));
			}
		}

		private static IEnumerable<string> GetFiles()
		{
			return Directory.EnumerateFiles(@".\Contents", "*.jpg", SearchOption.TopDirectoryOnly);
		}
	}
}
