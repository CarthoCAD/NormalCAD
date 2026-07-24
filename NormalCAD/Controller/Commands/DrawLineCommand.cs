using System.Collections.Generic;
using System.Threading.Tasks;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawLineCommand : ICadCommand
    {
        private static string PromptFirstPoint => CommandResources.Get("LINE.PROMPT.FIRSTPOINT");
        private static string PromptNextPoint => CommandResources.Get("LINE.PROMPT.NEXTPOINT");
        private static string KeyUndo => CommandResources.Get("LINE.KEY.UNDO");
        private static string KeyClose => CommandResources.Get("LINE.KEY.CLOSE");

        private CadController? _controller;
        private Point3d? _startPoint;
        private Point3d _firstPoint;
        private readonly List<ObjectId> _createdLines = new();
        private int _segmentCount;

        public string Name => "_.LINE";
        public string LocalName => CommandResources.Get("LINE.LOCALNAME");
        public CommandType Type => CommandType.Interactive;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => CommandResources.Get("LINE.ALIAS");

        public Task ActivateAsync(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _segmentCount = 0;
            _createdLines.Clear();
            _startPoint = null;
            RegisterFirstPointPrompt();
            return Task.CompletedTask;
        }

        public void Deactivate()
        {
            if (_controller != null)
            {
                _controller.InputManager.ClearAllRegistrations();
                _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
            }
        }

        private void RegisterFirstPointPrompt()
        {
            _controller!.InputManager.RegisterGetPoint(
                new PromptPointOptions { Message = PromptFirstPoint },
                OnFirstPoint);
        }

        private void OnFirstPoint(PromptPointResult result)
        {
            if (result.Status != PromptStatus.OK) { _controller!.FinishCommand(); return; }

            _startPoint = result.Value;
            _firstPoint = result.Value;
            _segmentCount = 0;
            RegisterNextPointPrompt();
        }

        private void RegisterNextPointPrompt()
        {
            var keywords = _segmentCount >= 2
                ? new[] { KeyUndo, KeyClose }
                : new[] { KeyUndo };

            _controller!.InputManager.RegisterGetPoint(
                new PromptPointOptions
                {
                    Message = PromptNextPoint,
                    BasePoint = _startPoint!.Value,
                    Keywords = keywords
                },
                OnNextPoint);
        }

        private void OnNextPoint(PromptPointResult result)
        {
            if (result.Status == PromptStatus.Keyword)
            {
                if (result.StringResult == KeyClose && _segmentCount >= 2)
                {
                    AddLine(_startPoint!.Value, _firstPoint);
                    _controller!.FinishCommand();
                    return;
                }
                if (result.StringResult == KeyUndo && _segmentCount >= 1)
                {
                    UndoLastSegment();
                    return;
                }
                return;
            }

            if (result.Status != PromptStatus.OK) { _controller!.FinishCommand(); return; }

            AddLine(_startPoint!.Value, result.Value);
            _segmentCount++;
            _startPoint = result.Value;
            RegisterNextPointPrompt();
        }

        private void AddLine(Point3d start, Point3d end)
        {
            var line = new Line(start, end)
            {
                Layer = _controller!.ActiveLayer,
                Color = _controller.ActiveColor
            };
            CadCoreHelper.AddNewEntityToCurrentSpace(line);
            _createdLines.Add(line.ObjectId);
        }

        private void UndoLastSegment()
        {
            if (_createdLines.Count == 0) return;

            var db = Core.ApplicationServices.Application.DocumentManager
                .MdiActiveDocument?.Database;
            if (db == null) return;

            var lastId = _createdLines[^1];
            _createdLines.RemoveAt(_createdLines.Count - 1);

            Point3d removedStartPoint;
            using (Core.ApplicationServices.Application.DocumentManager
                .MdiActiveDocument!.LockDocument())
            using (var trans = db.TransactionManager.StartTransaction())
            {
                if (db.TryGetObject(lastId, out var lastObj) && lastObj is Line lastLine)
                    removedStartPoint = lastLine.StartPoint;
                else
                    removedStartPoint = _startPoint!.Value;

                if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
                {
                    var spaceId = bt[BlockTableRecord.ModelSpace];
                    if (db.TryGetObject(spaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                    {
                        btr.RemoveEntity(lastId);
                    }
                }
                trans.Commit();
            }

            _segmentCount--;
            _startPoint = removedStartPoint;
            RegisterNextPointPrompt();
        }

    }
}
