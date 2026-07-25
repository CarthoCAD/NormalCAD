using System;
using System.Threading.Tasks;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawCircleCommand : ICadCommand
    {
        private static string PromptCenterPoint => CommandResources.Get("CIRCLE.PROMPT.CENTERPOINT");
        private static string PromptRadius => CommandResources.Get("CIRCLE.PROMPT.RADIUS");
        private static string PromptDiameter => CommandResources.Get("CIRCLE.PROMPT.DIAMETER");
        private static string KeyDiameter => CommandResources.Get("CIRCLE.KEY.DIAMETER");
        private static string KeyRadius => CommandResources.Get("CIRCLE.KEY.RADIUS");
        private static string Key3P => CommandResources.Get("CIRCLE.KEY.3P");
        private static string Key2P => CommandResources.Get("CIRCLE.KEY.2P");
        private static string KeyTtr => CommandResources.Get("CIRCLE.KEY.TTR");
        private static string MsgNotImpl => CommandResources.Get("CMD.MSG.NOTIMPL");

        private Point3d? _center;
        private bool _isDiameter;
        private Point3d _lastWorldPoint;

        public string Name => "_.CIRCLE";
        public string LocalName => CommandResources.Get("CIRCLE.LOCALNAME");
        public CommandType Type => CommandType.Interactive;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => CommandResources.Get("CIRCLE.ALIAS");

        public Task ActivateAsync()
        {
            CadController.Current.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _center = null;
            _isDiameter = false;
            CadController.Current.InputManager.RegisterMouseMove(OnMouseMove);
            RegisterCenterPrompt();
            return Task.CompletedTask;
        }

        public void Deactivate()
        {
            CadController.Current.InputManager.ClearAllRegistrations();
            CadController.Current.Viewport.CurrentCursorState = CadCursorState.PickCross;
        }

        private void RegisterCenterPrompt()
        {
            CadController.Current.InputManager.RegisterGetPoint(
                new PromptPointOptions
                {
                    Message = PromptCenterPoint,
                    Keywords = new[] { Key3P, Key2P, KeyTtr }
                },
                OnCenterPoint);
        }

        private void OnCenterPoint(PromptPointResult result)
        {
            if (result.Status == PromptStatus.Keyword)
            {
                CadController.Current.InputManager.SetPromptMessage(MsgNotImpl);
                RegisterCenterPrompt();
                return;
            }
            if (result.Status != PromptStatus.OK) { Finish(); return; }
            _center = result.Value;
            RegisterRadiusPrompt();
        }

        private void RegisterRadiusPrompt()
        {
            CadController.Current.InputManager.RegisterGetDistance(
                new PromptDistanceOptions
                {
                    Message = _isDiameter ? PromptDiameter : PromptRadius,
                    BasePoint = _center,
                    Keywords = new[] { _isDiameter ? KeyRadius : KeyDiameter }
                },
                OnRadius);
        }

        private void OnRadius(PromptDoubleResult result)
        {
            if (result.Status == PromptStatus.Keyword)
            {
                _isDiameter = !_isDiameter;
                RegisterRadiusPrompt();
                return;
            }
            if (result.Status != PromptStatus.OK) { Finish(); return; }

            double radius = _isDiameter ? result.Value / 2.0 : result.Value;

            if (radius > 1e-6)
            {
                var circle = new Circle(_center!.Value, Vector3d.ZAxis, radius)
                {
                    Layer = CadController.Current.ActiveLayer,
                    Color = CadController.Current.ActiveColor
                };
                CadCoreHelper.AddNewEntityToCurrentSpace(circle);
            }

            Finish();
        }

        private void OnMouseMove(Point3d worldPt)
        {
            _lastWorldPoint = worldPt;
            if (!_center.HasValue) return;

            double dist = _center.Value.DistanceTo(worldPt);
            double radius = _isDiameter ? dist / 2.0 : dist;

            if (radius > 1e-6)
            {
                CadController.Current.InputManager.SetPreview("circle",
                    new Circle(_center.Value, Vector3d.ZAxis, radius)
                    {
                        Layer = CadController.Current.ActiveLayer,
                        Color = CadController.Current.ActiveColor
                    });
            }
            CadController.Current.Viewport.InvalidateVisual();
        }

        private void Finish()
        {
            CadController.Current.FinishCommand();
        }
    }
}
