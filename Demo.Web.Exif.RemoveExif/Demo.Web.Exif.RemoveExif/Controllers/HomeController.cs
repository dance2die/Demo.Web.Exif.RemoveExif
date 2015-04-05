using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Demo.Web.Exif.RemoveExif.Controllers
{
	public class HomeController : Controller
	{
		// GET: Home
		public ActionResult Index()
		{
			ViewBag.Title = "Home/Index";

			return View((object)null);
		}

		[HttpPost]
		public ActionResult Index(IEnumerable<HttpPostedFileBase> files)
		{
			//foreach (var file in files)
			//{
			//	if (file.ContentLength > 0)
			//	{
			//		var fileName = Path.GetFileName(file.FileName);
			//		var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
			//		file.SaveAs(path);
			//	}
			//}
			//return RedirectToAction("Index");

			var file = files.FirstOrDefault();

			if (file.ContentLength <= 0) return View(file);

			var fileName = Path.GetFileName(file.FileName);
			var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
			//file.SaveAs(path);

			//using (Bitmap bitmap = new Bitmap(file.InputStream))
			//{
			//	bitmap.Save(path);
			//}

			var patcher = new JpegPatcher();
			using (FileStream fs = new FileStream(path, FileMode.Create))
			{
				patcher.PatchAwayExif(file.InputStream, fs);
			}

			return View(file);
		}
	}

	// http://stackoverflow.com/a/1252097/4035
	// http://techmikael.blogspot.com/2009/07/removing-exif-data-continued.html
	public class JpegPatcher
	{
		public Stream PatchAwayExif(Stream inStream, Stream outStream)
		{
			byte[] jpegHeader = new byte[2];
			jpegHeader[0] = (byte)inStream.ReadByte();
			jpegHeader[1] = (byte)inStream.ReadByte();
			if (jpegHeader[0] == 0xff && jpegHeader[1] == 0xd8) //check if it's a jpeg file
			{
				SkipAppHeaderSection(inStream);
			}
			outStream.WriteByte(0xff);
			outStream.WriteByte(0xd8);

			int readCount;
			byte[] readBuffer = new byte[4096];
			while ((readCount = inStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
				outStream.Write(readBuffer, 0, readCount);

			return outStream;
		}

		private void SkipAppHeaderSection(Stream inStream)
		{
			byte[] header = new byte[2];
			header[0] = (byte)inStream.ReadByte();
			header[1] = (byte)inStream.ReadByte();

			while (header[0] == 0xff && (header[1] >= 0xe0 && header[1] <= 0xef))
			{
				int exifLength = inStream.ReadByte();
				exifLength = exifLength << 8;
				exifLength |= inStream.ReadByte();

				for (int i = 0; i < exifLength - 2; i++)
				{
					inStream.ReadByte();
				}
				header[0] = (byte)inStream.ReadByte();
				header[1] = (byte)inStream.ReadByte();
			}
			inStream.Position -= 2; //skip back two bytes
		}
	}
}