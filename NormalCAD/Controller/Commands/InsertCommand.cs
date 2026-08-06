using System;
using System.Threading.Tasks;
using NormalCAD.Core.ApplicationServices;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;

namespace NormalCAD.Controller.Commands
{
    public class InsertCommand : ICadCommand
    {
        private static string PromptBlockName => CommandResources.Get("INSERT.PROMPT.BLOCKNAME");
        private static string PromptPoint => CommandResources.Get("INSERT.PROMPT.POINT");
        private static string PromptScale => CommandResources.Get("INSERT.PROMPT.SCALE");
        private static string PromptRotation => CommandResources.Get("INSERT.PROMPT.ROTATION");
        private static string MsgNotFound => CommandResources.Get("INSERT.MSG.NOTFOUND");

        private enum Phase { BlockName, InsertionPoint, Scale, Rotation }

        private Phase _phase;
        private BlockReference? _previewBlock;
        private Point3d _insertionPoint;

        public string Name => "_.INSERT";
        public string LocalName => CommandResources.Get("INSERT.LOCALNAME");
        public string Alias => CommandResources.Get("INSERT.ALIAS");
        public CommandType Type => CommandType.Interactive;
        public CommandFlags Flags => CommandFlags.None;

        public Task ActivateAsync()
        {
            _phase = Phase.BlockName;
            RegisterBlockNamePrompt();
            return Task.CompletedTask;
        }

        public void Deactivate()
        {
        }

        private void RegisterBlockNamePrompt()
        {
            CadController.Current.InputManager.RegisterGetString(
                new PromptStringOptions { Message = PromptBlockName },
                OnBlockName);
        }

        private void OnBlockName(PromptStringResult result)
        {
            if (result.Status != PromptStatus.OK) { Finish(); return; }

            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) { Finish(); return; }

            var db = doc.Database;
            if (!db.TryGetObject(db.BlockTableId, out var btObj) || btObj is not BlockTable bt)
            { Finish(); return; }

            var id = bt[result.StringResult];
            if (id.IsNull)
            {
                CadController.Current.InputManager.SetPromptMessage(
                    string.Format(MsgNotFound, result.StringResult));
                RegisterBlockNamePrompt();
                return;
            }

            _previewBlock = new BlockReference(Point3d.Origin, id)
            {
                Rotation = 0.0,
                ScaleFactors = new Vector3d(1, 1, 1),
                Layer = CadController.Current.ActiveLayer,
                Color = CadController.Current.ActiveColor
            };

            CadController.Current.InputManager.SetPreview("insert", _previewBlock);
            CadController.Current.InputManager.RegisterMouseMove(OnMouseMove);
            _phase = Phase.InsertionPoint;
            RegisterPointPrompt();
        }

        private void RegisterPointPrompt()
        {
            CadController.Current.InputManager.RegisterGetPoint(
                new PromptPointOptions { Message = PromptPoint },
                OnPoint);
        }

        private void OnPoint(PromptPointResult result)
        {
            if (result.Status != PromptStatus.OK) { Finish(); return; }

            _insertionPoint = result.Value;
            _previewBlock!.Position = _insertionPoint;

            _phase = Phase.Scale;
            CadController.Current.InputManager.RegisterGetDistance(
                new PromptDistanceOptions
                {
                    Message = PromptScale,
                    BasePoint = _insertionPoint
                },
                OnScale);
        }

        private void OnScale(PromptDoubleResult result)
        {
            if (result.Status != PromptStatus.OK) { Finish(); return; }

            _previewBlock!.ScaleFactors = new Vector3d(
                result.Value, result.Value, result.Value);

            _phase = Phase.Rotation;
            CadController.Current.InputManager.RegisterGetAngle(
                new PromptAngleOptions
                {
                    Message = PromptRotation,
                    BasePoint = _insertionPoint
                },
                OnRotation);
        }

        private void OnRotation(PromptDoubleResult result)
        {
            if (result.Status != PromptStatus.OK) { Finish(); return; }

            _previewBlock!.Rotation = result.Value;
            CommitBlock();
            Finish();
        }

        private void OnMouseMove(Point3d worldPt)
        {
            if (_previewBlock == null) return;

            switch (_phase)
            {
                case Phase.InsertionPoint:
                    _previewBlock.Position = worldPt;
                    break;

                case Phase.Scale:
                    double distance = _insertionPoint.DistanceTo(worldPt);
                    if (distance > 1e-9)
                        _previewBlock.ScaleFactors = new Vector3d(
                            distance, distance, distance);
                    break;

                case Phase.Rotation:
                    double angle = Math.Atan2(
                        worldPt.Y - _insertionPoint.Y,
                        worldPt.X - _insertionPoint.X);
                    _previewBlock.Rotation = angle;
                    break;
            }

            CadController.Current.Viewport.InvalidateVisual();
        }

        private void CommitBlock()
        {
            CadCoreHelper.AddNewEntityToCurrentSpace(_previewBlock!);
        }

        private void Finish()
        {
            CadController.Current.FinishCommand();
        }
    }
}
