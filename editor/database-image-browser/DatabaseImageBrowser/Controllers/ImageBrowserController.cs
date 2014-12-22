﻿namespace DatabaseImageBrowser.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Models;
    using Kendo.Mvc.UI;

    public class ImageBrowserController : Controller, IImageBrowserController
    {
        private const int ThumbnailHeight = 80;
        private const int ThumbnailWidth = 80;

        public ActionResult Image(string path)
        {
            var files = new FilesRepository();
            var image = files.ImageByPath(path);
            if (image != null)
            {
                const string contentType = "image/png";
                return File(image.Image1, contentType);
            }
            throw new HttpException(404, "File Not Found");
        }

        public ActionResult Create(string path, FileBrowserEntry entry)
        {
            var files = new FilesRepository();
            var folder = files.GetFolderByPath(path);
            if (folder != null)
            {
                files.CreateDirectory(folder, entry.Name);
                return Json(new object[0]);
            }
            throw new HttpException(403, "Forbidden");
        }

        public ActionResult Destroy(string path, FileBrowserEntry entry)
        {
            var files = new FilesRepository();
            if (entry.EntryType == FileBrowserEntryType.File)
            {
                var image = files.ImageByPath(Path.Combine(path, entry.Name));
                if (image != null)
                {
                    files.Delete(image);
                    return Json(new object[0]);
                }
            } else 
            {
                var folder = files.GetFolderByPath(Path.Combine(path, entry.Name));
                if (folder != null)
                {
                    files.Delete(folder);
                    return Json(new object[0]);
                }
            } 
            throw new HttpException(404, "File Not Found");
        }

        public JsonResult Read(string path)
        {
            var files = new FilesRepository();

            var folders = files.Folders(path);

            var images = files.Images(path);

            return Json(folders.Concat(images));
        }

        public ActionResult Thumbnail(string path)
        {
            //TODO:Add security checks

            var files = new FilesRepository();
            var image = files.ImageByPath(path);
            if (image != null)
            {
                var desiredSize = new ImageSize { Width = ThumbnailWidth, Height = ThumbnailHeight };

                const string contentType = "image/png";

                var thumbnailCreator = new ThumbnailCreator(new FitImageResizer());

                using (var stream = new MemoryStream(image.Image1.ToArray()))
                {
                    return File(thumbnailCreator.Create(stream, desiredSize, contentType), contentType);
                }
            }
            throw new HttpException(404, "File Not Found");
        }

        public ActionResult Upload(string path, HttpPostedFileBase file)
        {
            if (file != null)
            {
                var files = new FilesRepository();
                var parentFolder = files.GetFolderByPath(path);
                if (parentFolder != null)
                {
                    files.SaveImage(parentFolder, file);
                    return Json(
                        new FileBrowserEntry
                        {
                            Name = Path.GetFileName(file.FileName),
                            Size = file.ContentLength
                        }
                    , "text/plain");
                }
            }
            throw new HttpException(404, "File Not Found");
        }
    }
}