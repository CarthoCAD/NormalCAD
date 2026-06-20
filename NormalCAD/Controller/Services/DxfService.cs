using System;
using System.IO;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Controller.Services.Converters;
using ACadSharp;
using ACadSharp.IO;
using CadDocument = ACadSharp.CadDocument;

namespace NormalCAD.Controller.Services
{
    public static class DxfService
    {
        private static readonly ConverterService _converters = new ConverterService();

        public static Database LoadDxf(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Arquivo DXF não encontrado.", filePath);

            CadDocument cadDoc;
            using (var reader = new DxfReader(filePath))
            {
                cadDoc = reader.Read();
            }

            if (cadDoc == null)
                throw new Exception("Falha ao carregar o documento DXF.");

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

        public static void SaveDxf(Database db, string filePath)
        {
            var cadDoc = new CadDocument();

            TableParsers.SaveLayers(db, cadDoc, _converters);
            TableParsers.SaveViewports(db, cadDoc, _converters);
            TableParsers.SaveEntities(db, cadDoc, _converters);

            using (var writer = new DxfWriter(filePath, cadDoc, false))
            {
                writer.Write();
            }
        }
    }
}
