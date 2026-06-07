using System;
using System.IO;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;
using netDxf;

namespace NormalCAD.Controller.Services
{
    public static class DxfService
    {
        public static Database LoadDxf(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Arquivo DXF não encontrado.", filePath);

            var dxfDoc = DxfDocument.Load(filePath);
            if (dxfDoc == null)
                throw new Exception("Falha ao carregar o documento DXF.");

            var db = new Database();

            using (var trans = db.TransactionManager.StartTransaction())
            {
                // 1. Carrega Camadas (Layers)
                if (db.TryGetObject(db.LayerTableId, out var ltObj) && ltObj is LayerTable lt)
                {
                    foreach (var dxfLayer in dxfDoc.Layers)
                    {
                        if (lt.Has(dxfLayer.Name))
                        {
                            // Atualiza cor da camada padrão se necessário
                            var existingId = lt[dxfLayer.Name];
                            var existingRec = lt.GetRecord(existingId);
                            existingRec.Color = MapDxfColor(dxfLayer.Color);
                        }
                        else
                        {
                            var layerRec = new LayerTableRecord(dxfLayer.Name, MapDxfColor(dxfLayer.Color))
                            {
                                IsVisible = dxfLayer.IsVisible
                            };
                            lt.Add(layerRec);
                        }
                    }
                }

                // 2. Carrega Entidades no ModelSpace
                if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
                {
                    var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                    if (db.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                    {
                        // Linhas
                        foreach (netDxf.Entities.Line dxfLine in dxfDoc.Entities.Lines)
                        {
                            var line = new Line(
                                new Point3d(dxfLine.StartPoint.X, dxfLine.StartPoint.Y, dxfLine.StartPoint.Z),
                                new Point3d(dxfLine.EndPoint.X, dxfLine.EndPoint.Y, dxfLine.EndPoint.Z)
                            )
                            {
                                Layer = dxfLine.Layer.Name,
                                Color = MapDxfColor(dxfLine.Color)
                            };
                            btr.AppendEntity(line);
                            trans.AddNewlyCreatedDBObject(line, true);
                        }

                        // Círculos
                        foreach (netDxf.Entities.Circle dxfCircle in dxfDoc.Entities.Circles)
                        {
                            var circle = new Circle(
                                new Point3d(dxfCircle.Center.X, dxfCircle.Center.Y, dxfCircle.Center.Z),
                                dxfCircle.Radius
                            )
                            {
                                Layer = dxfCircle.Layer.Name,
                                Color = MapDxfColor(dxfCircle.Color)
                            };
                            btr.AppendEntity(circle);
                            trans.AddNewlyCreatedDBObject(circle, true);
                        }

                        // Arcos
                        foreach (netDxf.Entities.Arc dxfArc in dxfDoc.Entities.Arcs)
                        {
                            var arc = new Arc(
                                new Point3d(dxfArc.Center.X, dxfArc.Center.Y, dxfArc.Center.Z),
                                dxfArc.Radius,
                                dxfArc.StartAngle,
                                dxfArc.EndAngle
                            )
                            {
                                Layer = dxfArc.Layer.Name,
                                Color = MapDxfColor(dxfArc.Color)
                            };
                            btr.AppendEntity(arc);
                            trans.AddNewlyCreatedDBObject(arc, true);
                        }

                        // Polilinhas leves (LwPolylines) - Importa decompondo em linhas simples
                        foreach (netDxf.Entities.Polyline2D dxfPoly in dxfDoc.Entities.Polylines2D)
                        {
                            if (dxfPoly.Vertexes.Count < 2) continue;

                            for (int i = 0; i < dxfPoly.Vertexes.Count - 1; i++)
                            {
                                var v1 = dxfPoly.Vertexes[i];
                                var v2 = dxfPoly.Vertexes[i + 1];
                                var line = new Line(
                                    new Point3d(v1.Position.X, v1.Position.Y, 0.0),
                                    new Point3d(v2.Position.X, v2.Position.Y, 0.0)
                                )
                                {
                                    Layer = dxfPoly.Layer.Name,
                                    Color = MapDxfColor(dxfPoly.Color)
                                };
                                btr.AppendEntity(line);
                                trans.AddNewlyCreatedDBObject(line, true);
                            }

                            if (dxfPoly.IsClosed)
                            {
                                var vLast = dxfPoly.Vertexes[dxfPoly.Vertexes.Count - 1];
                                var vFirst = dxfPoly.Vertexes[0];
                                var line = new Line(
                                    new Point3d(vLast.Position.X, vLast.Position.Y, 0.0),
                                    new Point3d(vFirst.Position.X, vFirst.Position.Y, 0.0)
                                )
                                {
                                    Layer = dxfPoly.Layer.Name,
                                    Color = MapDxfColor(dxfPoly.Color)
                                };
                                btr.AppendEntity(line);
                                trans.AddNewlyCreatedDBObject(line, true);
                            }
                        }
                    }
                }

                trans.Commit();
            }

            return db;
        }

        public static void SaveDxf(Database db, string filePath)
        {
            var dxfDoc = new DxfDocument();

            // 1. Exporta Camadas
            if (db.TryGetObject(db.LayerTableId, out var ltObj) && ltObj is LayerTable lt)
            {
                foreach (var layerRec in lt)
                {
                    if (layerRec.Name == "0") continue; // Já existe por padrão no netDxf

                    var dxfLayer = new netDxf.Tables.Layer(layerRec.Name)
                    {
                        Color = MapCoreColor(layerRec.Color),
                        IsVisible = layerRec.IsVisible
                    };
                    dxfDoc.Layers.Add(dxfLayer);
                }
            }

            // 2. Exporta Entidades do ModelSpace
            if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
            {
                var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                if (!modelSpaceId.IsNull && db.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                {
                    foreach (var entId in btr.GetEntityIds())
                    {
                        if (!db.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                            continue;

                        netDxf.Entities.EntityObject dxfEnt;

                        if (ent is Line line)
                        {
                            dxfEnt = new netDxf.Entities.Line(
                                new Vector3(line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z),
                                new Vector3(line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z)
                            );
                        }
                        else if (ent is Circle circle)
                        {
                            dxfEnt = new netDxf.Entities.Circle(
                                new Vector3(circle.Center.X, circle.Center.Y, circle.Center.Z),
                                circle.Radius
                            );
                        }
                        else if (ent is Arc arc)
                        {
                            dxfEnt = new netDxf.Entities.Arc(
                                new Vector3(arc.Center.X, arc.Center.Y, arc.Center.Z),
                                arc.Radius,
                                arc.StartAngle,
                                arc.EndAngle
                            );
                        }
                        else
                        {
                            continue; // Entidade desconhecida, ignora
                        }

                        // Configura Camada no DXF
                        if (dxfDoc.Layers.Contains(ent.Layer))
                        {
                            dxfEnt.Layer = dxfDoc.Layers[ent.Layer];
                        }
                        else
                        {
                            var newDxfLayer = new netDxf.Tables.Layer(ent.Layer);
                            dxfDoc.Layers.Add(newDxfLayer);
                            dxfEnt.Layer = newDxfLayer;
                        }

                        // Configura Cor
                        if (ent.Color.IsByLayer)
                        {
                            dxfEnt.Color = AciColor.ByLayer;
                        }
                        else
                        {
                            dxfEnt.Color = MapCoreColor(ent.Color);
                        }

                        dxfDoc.Entities.Add(dxfEnt);
                    }
                }
            }

            dxfDoc.Save(filePath);
        }

        private static EntityColor MapDxfColor(AciColor color)
        {
            if (color == null || color == AciColor.ByLayer)
                return EntityColor.ByLayer;

            var rgb = color.ToColor();
            return new EntityColor(rgb.R, rgb.G, rgb.B);
        }

        private static AciColor MapCoreColor(EntityColor color)
        {
            if (color.IsByLayer)
                return AciColor.ByLayer;

            return new AciColor(color.R, color.G, color.B);
        }
    }
}
