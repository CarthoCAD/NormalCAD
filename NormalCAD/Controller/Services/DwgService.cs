using System;
using System.IO;
using NormalCAD.Core;
using NormalCAD.Controller.Services.Converters;
using ACadSharp;
using ACadSharp.IO;
using CadDocument = ACadSharp.CadDocument;

namespace NormalCAD.Controller.Services
{
    public static class DwgService
    {
        private static readonly ConverterService _converters = new ConverterService();

        public static Database LoadDwg(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Arquivo DWG não encontrado.", filePath);

            CadDocument cadDoc;
            using (var reader = new DwgReader(filePath))
            {
                try
                {
                    cadDoc = reader.Read();
                }
                catch (Exception ex) when (ex is NotSupportedException || ex.Message.Contains("version", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception(
                        "Este arquivo DWG está em uma versão não suportada pelo ACadSharp. " +
                        "Versões suportadas: AutoCAD R14, 2000-2020 (AC1014 a AC1032).", ex);
                }
            }

            if (cadDoc == null)
                throw new Exception("Falha ao carregar o documento DWG.");

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

        public static void SaveDwg(Database db, string filePath)
        {
            var cadDoc = new CadDocument(ACadVersion.AC1032);

            TableParsers.SaveLayers(db, cadDoc, _converters);
            TableParsers.SaveViewports(db, cadDoc, _converters);
            TableParsers.SaveEntities(db, cadDoc, _converters);

            using (var writer = new DwgWriter(filePath, cadDoc))
            {
                writer.Write();
            }
        }
    }
}
