using System;
using System.IO;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Controller.Services.Converters;
using NormalCAD.Resources;
using ACadSharp;
using ACadSharp.IO;
using CadDocument = ACadSharp.CadDocument;

namespace NormalCAD.Controller.Services
{
    public static class FileService
    {
        private static string ErrorNotFound => DialogResources.Get("FILESERVICE.ERROR.NOTFOUND");
        private static string ErrorUnsupported => DialogResources.Get("FILESERVICE.ERROR.UNSUPPORTED");
        private static string ErrorLoadFailed => DialogResources.Get("FILESERVICE.ERROR.LOADFAILED");
        private static string ErrorDwgVersion => DialogResources.Get("FILESERVICE.ERROR.DWGVERSION");

        private static readonly ConverterService _converters = new ConverterService();

        public static Database Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(ErrorNotFound, filePath);

            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            CadDocument cadDoc = ext switch
            {
                ".dxf" => ReadDxf(filePath),
                ".dwg" => ReadDwg(filePath),
                _ => throw new ArgumentException(string.Format(ErrorUnsupported, ext))
            };

            if (cadDoc == null)
                throw new Exception(ErrorLoadFailed);

            var db = new Database();

            using (var trans = db.TransactionManager.StartTransaction())
            {
                TableParsers.LoadLayers(cadDoc, db, _converters);
                TableParsers.LoadViewports(cadDoc, db, _converters);
                TableParsers.LoadEntities(cadDoc, db, trans, _converters);

                trans.Commit();
            }

            return db;
        }

        public static void Save(Database db, string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            CadDocument cadDoc = ext switch
            {
                ".dxf" => new CadDocument(),
                ".dwg" => new CadDocument(ACadVersion.AC1032),
                _ => throw new ArgumentException(string.Format(ErrorUnsupported, ext))
            };

            TableParsers.SaveLayers(db, cadDoc, _converters);
            TableParsers.SaveViewports(db, cadDoc, _converters);
            TableParsers.SaveEntities(db, cadDoc, _converters);

            switch (ext)
            {
                case ".dxf":
                    using (var writer = new DxfWriter(filePath, cadDoc, false))
                        writer.Write();
                    break;
                case ".dwg":
                    using (var writer = new DwgWriter(filePath, cadDoc))
                        writer.Write();
                    break;
            }
        }

        private static CadDocument ReadDxf(string filePath)
        {
            using (var reader = new DxfReader(filePath))
                return reader.Read();
        }

        private static CadDocument ReadDwg(string filePath)
        {
            using (var reader = new DwgReader(filePath))
            {
                try
                {
                    return reader.Read();
                }
                catch (Exception ex) when (ex is NotSupportedException || ex.Message.Contains("version", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception(ErrorDwgVersion, ex);
                }
            }
        }
    }
}
