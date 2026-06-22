using System;
using System.IO;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Controller.Services.Converters;
using ACadSharp;
using ACadSharp.IO;
using CadDocument = ACadSharp.CadDocument;

namespace NormalCAD.Controller.Services
{
    public static class FileService
    {
        private static readonly ConverterService _converters = new ConverterService();

        public static Database Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Arquivo não encontrado.", filePath);

            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            CadDocument cadDoc = ext switch
            {
                ".dxf" => ReadDxf(filePath),
                ".dwg" => ReadDwg(filePath),
                _ => throw new ArgumentException($"Formato de arquivo não suportado: {ext}")
            };

            if (cadDoc == null)
                throw new Exception("Falha ao carregar o documento.");

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
                _ => throw new ArgumentException($"Formato de arquivo não suportado: {ext}")
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
                    throw new Exception(
                        "Este arquivo DWG está em uma versão não suportada pelo ACadSharp. " +
                        "Versões suportadas: AutoCAD R14, 2000-2020 (AC1014 a AC1032).", ex);
                }
            }
        }
    }
}
