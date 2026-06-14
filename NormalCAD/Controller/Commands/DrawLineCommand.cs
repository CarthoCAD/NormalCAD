using Avalonia.Input;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawLineCommand : ICadCommand
    {
        private CadController? _controller;
        private Point3d? _startPoint;

        public string Name => "Desenhar Linha";

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _startPoint = null;
        }

        public void Deactivate()
        {
            if (_controller != null)
            {
                _controller.Viewport.ActiveCommandPreview = null;
                _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
            }
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            if (_controller == null) return;

            if (!_startPoint.HasValue)
            {
                _startPoint = worldPt;
            }
            else
            {
                using (var trans = _controller.Database.TransactionManager.StartTransaction())
                {
                    if (_controller.Database.TryGetObject(_controller.Database.BlockTableId, out var btObj) && btObj is BlockTable bt)
                    {
                        var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                        if (_controller.Database.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                        {
                            var line = new Line(_startPoint.Value, worldPt)
                            {
                                Layer = _controller.ActiveLayer,
                                Color = _controller.ActiveColor
                            };
                            btr.AppendEntity(line);
                            trans.AddNewlyCreatedDBObject(line, true);
                        }
                    }
                    trans.Commit();
                }

                _controller.NotifyDatabaseChanged();

                // AutoCAD-like chain drawing: o fim da linha anterior vira o início da próxima
                _startPoint = worldPt;
            }
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_controller == null || !_startPoint.HasValue) return;

            _controller.Viewport.ActiveCommandPreview = new Line(_startPoint.Value, worldPt)
            {
                Layer = _controller.ActiveLayer,
                Color = _controller.ActiveColor
            };
        }

        public void OnKeyDown(KeyEventArgs e)
        {
        }
    }
}
