using System;
using System.Threading.Tasks;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;

namespace NormalCAD.Controller.Commands
{
    public class DrawArcCommand : ICadCommand
    {
        private static string PromptStartPoint => CommandResources.Get("ARC.PROMPT.STARTPOINT");
        private static string PromptCenter => CommandResources.Get("ARC.PROMPT.CENTER");
        private static string PromptSecondPoint => CommandResources.Get("ARC.PROMPT.SECONDPOINT");
        private static string PromptEndPoint => CommandResources.Get("ARC.PROMPT.ENDPOINT");
        private static string PromptStartPointC => CommandResources.Get("ARC.PROMPT.STARTPOINT_C");
        private static string PromptEndPointC => CommandResources.Get("ARC.PROMPT.ENDPOINT_C");
        private static string KeyCenter => CommandResources.Get("ARC.KEY.CENTER");
        private static string KeyEnd => CommandResources.Get("ARC.KEY.END");

        private Point3d _startPt, _midPt;
        private enum ArcState { StartPoint, SecondPoint, EndPoint, Center_Center, Center_Start, Center_End }
        private ArcState _state;

        public string Name => "_.ARC";
        public string LocalName => CommandResources.Get("ARC.LOCALNAME");
        public CommandType Type => CommandType.Interactive;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => CommandResources.Get("ARC.ALIAS");

        public Task ActivateAsync()
        {
            CadController.Current.InputManager.RegisterMouseMove(OnMouseMove);
            _state = ArcState.StartPoint;
            CadController.Current.InputManager.RegisterGetPoint(
                new PromptPointOptions { Message = PromptStartPoint, Keywords = new[] { KeyCenter } },
                OnStep);
            return Task.CompletedTask;
        }

        public void Deactivate()
        {
        }

        private void OnStep(PromptPointResult result)
        {
            if (result.Status == PromptStatus.Keyword)
            {
                if (_state == ArcState.StartPoint && result.StringResult == KeyCenter)
                {
                    _state = ArcState.Center_Center;
                    CadController.Current.InputManager.RegisterGetPoint(
                        new PromptPointOptions { Message = PromptCenter },
                        OnStep);
                    return;
                }
                if (_state == ArcState.SecondPoint && result.StringResult == KeyCenter)
                {
                    _state = ArcState.Center_Center;
                    CadController.Current.InputManager.RegisterGetPoint(
                        new PromptPointOptions { Message = PromptCenter },
                        OnStep);
                    return;
                }
                if (_state == ArcState.SecondPoint && result.StringResult == KeyEnd)
                {
                    _state = ArcState.EndPoint;
                    CadController.Current.InputManager.RegisterGetPoint(
                        new PromptPointOptions { Message = PromptEndPoint },
                        OnStep);
                    return;
                }
                return;
            }

            if (result.Status != PromptStatus.OK) { CadController.Current.FinishCommand(); return; }

            switch (_state)
            {
                case ArcState.StartPoint:
                    _startPt = result.Value;
                    _state = ArcState.SecondPoint;
                    CadController.Current.InputManager.RegisterGetPoint(
                        new PromptPointOptions
                        {
                            Message = PromptSecondPoint,
                            Keywords = new[] { KeyCenter, KeyEnd },
                            BasePoint = _startPt
                        },
                        OnStep);
                    break;

                case ArcState.SecondPoint:
                    _midPt = result.Value;
                    _state = ArcState.EndPoint;
                    CadController.Current.InputManager.RegisterGetPoint(
                        new PromptPointOptions
                        {
                            Message = PromptEndPoint,
                            BasePoint = _midPt,
                            UseBasePoint = false
                        },
                        OnStep);
                    break;

                case ArcState.EndPoint:
                    CreateArc3Point(_startPt, _midPt, result.Value);
                    CadController.Current.FinishCommand();
                    break;

                case ArcState.Center_Center:
                    _startPt = result.Value;
                    _state = ArcState.Center_Start;
                    CadController.Current.InputManager.RegisterGetPoint(
                        new PromptPointOptions
                        {
                            Message = PromptStartPointC,
                            BasePoint = _startPt
                        },
                        OnStep);
                    break;

                case ArcState.Center_Start:
                    _midPt = result.Value;
                    _state = ArcState.Center_End;
                    CadController.Current.InputManager.RegisterGetPoint(
                        new PromptPointOptions { Message = PromptEndPointC },
                        OnStep);
                    break;

                case ArcState.Center_End:
                    CreateArcCenterStartEnd(_startPt, _midPt, result.Value);
                    CadController.Current.FinishCommand();
                    break;
            }
        }

        private void CreateArc3Point(Point3d p1, Point3d p2, Point3d p3)
        {
            if (!TryComputeArc3Point(p1, p2, p3, out var center, out var radius,
                    out var startAngle, out var endAngle))
                return;

            var arc = new Arc(center, radius, startAngle, endAngle)
            {
                Layer = CadController.Current.ActiveLayer,
                Color = CadController.Current.ActiveColor
            };
            CadCoreHelper.AddNewEntityToCurrentSpace(arc);
        }

        private void CreateArcCenterStartEnd(Point3d center, Point3d start, Point3d end)
        {
            double radius = center.DistanceTo(start);
            if (radius < 1e-6) return;

            double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
            double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);

            var arc = new Arc(center, radius, startAngle, endAngle)
            {
                Layer = CadController.Current.ActiveLayer,
                Color = CadController.Current.ActiveColor
            };
            CadCoreHelper.AddNewEntityToCurrentSpace(arc);
        }

        private static bool TryComputeArc3Point(Point3d p1, Point3d p2, Point3d p3,
            out Point3d center, out double radius, out double startAngle, out double endAngle)
        {
            center = Point3d.Origin;
            radius = 0;
            startAngle = 0;
            endAngle = 0;

            if (AreNearlyCollinear(p1, p2, p3))
                return false;

            (center, radius) = ComputeArcFrom3Points(p1, p2, p3);
            if (radius < 1e-6)
                return false;

            double a1 = NormalizeAngle(Math.Atan2(p1.Y - center.Y, p1.X - center.X));
            double a2 = NormalizeAngle(Math.Atan2(p2.Y - center.Y, p2.X - center.X));
            double a3 = NormalizeAngle(Math.Atan2(p3.Y - center.Y, p3.X - center.X));

            if (IsAngleBetween(a2, a1, a3))
            {
                startAngle = a1;
                endAngle = a3;
            }
            else
            {
                startAngle = a3;
                endAngle = a1;
            }

            return true;
        }

        private static (Point3d center, double radius) ComputeArcFrom3Points(
            Point3d p1, Point3d p2, Point3d p3)
        {
            double d = 2 * (p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y));

            double ux = ((p1.X * p1.X + p1.Y * p1.Y) * (p2.Y - p3.Y)
                       + (p2.X * p2.X + p2.Y * p2.Y) * (p3.Y - p1.Y)
                       + (p3.X * p3.X + p3.Y * p3.Y) * (p1.Y - p2.Y)) / d;
            double uy = ((p1.X * p1.X + p1.Y * p1.Y) * (p3.X - p2.X)
                       + (p2.X * p2.X + p2.Y * p2.Y) * (p1.X - p3.X)
                       + (p3.X * p3.X + p3.Y * p3.Y) * (p2.X - p1.X)) / d;

            var center = new Point3d(ux, uy, 0);
            double radius = center.DistanceTo(p1);
            return (center, radius);
        }

        private static bool AreNearlyCollinear(Point3d p1, Point3d p2, Point3d p3)
        {
            double d = 2 * (p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y));
            return Math.Abs(d) < 1e-9;
        }

        private static double NormalizeAngle(double angle)
        {
            double result = angle % (2 * Math.PI);
            if (result < 0) result += 2 * Math.PI;
            return result;
        }

        private static bool IsAngleBetween(double angle, double start, double end)
        {
            if (start <= end)
                return angle >= start && angle <= end;
            return angle >= start || angle <= end;
        }

        private void OnMouseMove(Point3d worldPt)
        {

            if (_state == ArcState.EndPoint)
            {
                if (TryComputeArc3Point(_startPt, _midPt, worldPt, out var center,
                        out var radius, out var startAngle, out var endAngle))
                {
                    CadController.Current.InputManager.SetPreview("arc",
                        new Arc(center, radius, startAngle, endAngle)
                        {
                            Layer = CadController.Current.ActiveLayer,
                            Color = CadController.Current.ActiveColor
                        });
                }
                else
                {
                    CadController.Current.InputManager.RemovePreview("arc");
                }
            }
            else if (_state == ArcState.Center_End)
            {
                double radius = _startPt.DistanceTo(_midPt);
                if (radius > 1e-6)
                {
                    double startAngle = Math.Atan2(_midPt.Y - _startPt.Y, _midPt.X - _startPt.X);
                    double endAngle = Math.Atan2(worldPt.Y - _startPt.Y, worldPt.X - _startPt.X);
                    CadController.Current.InputManager.SetPreview("arc",
                        new Arc(_startPt, radius, startAngle, endAngle)
                        {
                            Layer = CadController.Current.ActiveLayer,
                            Color = CadController.Current.ActiveColor
                        });
                }
            }
            CadController.Current.Viewport.InvalidateVisual();
        }
    }
}
