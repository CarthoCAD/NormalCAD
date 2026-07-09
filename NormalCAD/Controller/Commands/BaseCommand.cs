using Avalonia;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class BaseCommand : ICadCommand
    {
        private static string MsgFound => CommandResources.Get("CMD.MSG.FOUND");
        private static string MsgRemoved => CommandResources.Get("CMD.MSG.REMOVED");
        private static string MsgFoundN => CommandResources.Get("CMD.MSG.FOUND_N");
        private static string MsgRemovedN => CommandResources.Get("CMD.MSG.REMOVED_N");
        private static string PromptOppositeCorner => CommandResources.Get("CMD.PROMPT.OPPOSITECORNER");

        private CadController? _controller;
        private bool _isSelectingBox;

        public string Name => "*BASECOMMAND";
        public string LocalName => CommandResources.Get("CMD.LOCALNAME");
        public string Alias => "";
        public bool IsInternal => true;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _isSelectingBox = false;
            if (_controller?.Viewport != null)
            {
                _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
                _controller.Viewport.SelectionStartPoint = null;
                _controller.Viewport.SelectionEndPoint = null;
            }
        }

        public void Deactivate()
        {
            if (_controller?.Viewport != null)
            {
                _controller.Viewport.SelectionStartPoint = null;
                _controller.Viewport.SelectionEndPoint = null;
            }
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            if (_controller == null) return;

            var viewport = _controller.Viewport;
            var db = _controller.Database;
            var mouseScreenPos = e.GetPosition(viewport);
            bool isShift = (e.KeyModifiers & KeyModifiers.Shift) != 0;

            if (_isSelectingBox)
            {
                viewport.SelectionEndPoint = mouseScreenPos;
                int found = PerformSelectionBox(viewport, isShift);
                FinishSelection(found, isShift);
                return;
            }

            ObjectId selectedId = ObjectId.Null;
            double bestScreenDist = 10.0;
            double zoom = Math.Max(viewport.Zoom, 0.001);

            if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
            {
                var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                if (!modelSpaceId.IsNull && db.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                {
                    double worldTolerance = bestScreenDist / zoom;
                    var worldMouse = viewport.ScreenToWorld(mouseScreenPos);
                    var candidateIds = btr.QueryNearPoint(worldMouse, worldTolerance);

                    foreach (var entId in candidateIds)
                    {
                        if (!db.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                            continue;

                        double screenDist = ent.GetDistanceTo(worldMouse) * zoom;
                        if (screenDist < bestScreenDist)
                        {
                            bestScreenDist = screenDist;
                            selectedId = entId;
                        }
                    }
                }
            }

            if (!selectedId.IsNull)
            {
                if (isShift)
                {
                    if (_controller.IsSelected(selectedId))
                        _controller.RemoveFromSelection(selectedId);
                }
                else
                {
                    if (!_controller.IsSelected(selectedId))
                        _controller.AddToSelection(selectedId);
                }

                int total = _controller.SelectedEntityIds.Count;
                string message = isShift
                    ? string.Format(MsgRemoved, total)
                    : string.Format(MsgFound, total);

                _controller.InputManager.SetCurrentPrompt(LocalName);
                _controller.InputManager.SetPromptMessage(message);
                viewport.InvalidateVisual();
            }
            else
            {
                _isSelectingBox = true;
                viewport.SelectionStartPoint = mouseScreenPos;
                viewport.SelectionEndPoint = mouseScreenPos;
                _controller.InputManager.SetCurrentPrompt(LocalName, PromptOppositeCorner);
                viewport.InvalidateVisual();
            }
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_controller == null || !_isSelectingBox) return;

            var viewport = _controller.Viewport;
            viewport.SelectionEndPoint = viewport.WorldToScreen(worldPt);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
        }

        private void FinishSelection(int found, bool isShift)
        {
            _isSelectingBox = false;
            _controller!.InputManager.SetCurrentPrompt(LocalName);

            int total = _controller.SelectedEntityIds.Count;
            string message = isShift
                ? string.Format(MsgRemovedN, found, total)
                : string.Format(MsgFoundN, found, total);

            _controller.InputManager.SetPromptMessage(message);
            _controller.Viewport.SelectionStartPoint = null;
            _controller.Viewport.SelectionEndPoint = null;
            _controller.Viewport.InvalidateVisual();
        }

        private int PerformSelectionBox(CadViewport viewport, bool isShift)
        {
            if (!viewport.SelectionStartPoint.HasValue || !viewport.SelectionEndPoint.HasValue) return 0;

            var p1 = viewport.SelectionStartPoint.Value;
            var p2 = viewport.SelectionEndPoint.Value;

            var screenRect = GetRect(p1, p2);
            bool isCrossing = p2.X < p1.X;

            var db = _controller!.Database;
            var toSelect = new List<ObjectId>();

            if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
            {
                var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                if (!modelSpaceId.IsNull && db.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                {
                    var worldTL = viewport.ScreenToWorld(screenRect.TopLeft);
                    var worldBR = viewport.ScreenToWorld(screenRect.BottomRight);
                    var worldBounds = Extents3d.FromPoints(worldTL, worldBR);
                    var candidateIds = btr.QueryExtents(worldBounds);

                    foreach (var entId in candidateIds)
                    {
                        if (!db.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                            continue;

                        if (isCrossing)
                        {
                            if (EntityIntersectsRect(viewport, screenRect, ent))
                                toSelect.Add(entId);
                        }
                        else
                        {
                            if (EntityInsideRect(viewport, screenRect, ent))
                                toSelect.Add(entId);
                        }
                    }
                }
            }

            int changed = 0;
            if (isShift)
            {
                foreach (var entId in toSelect)
                {
                    if (_controller.IsSelected(entId))
                    {
                        _controller.RemoveFromSelection(entId);
                        changed++;
                    }
                }
            }
            else
            {
                foreach (var entId in toSelect)
                {
                    if (!_controller.IsSelected(entId))
                    {
                        _controller.AddToSelection(entId);
                        changed++;
                    }
                }
            }

            return changed;
        }

        private static Rect GetRect(Point p1, Point p2)
        {
            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double w = Math.Abs(p1.X - p2.X);
            double h = Math.Abs(p1.Y - p2.Y);
            return new Rect(x, y, w, h);
        }

        private static bool EntityInsideRect(CadViewport viewport, Rect screenRect, Entity ent)
        {
            foreach (var pt in ent.GetGripPoints())
            {
                if (!screenRect.Contains(viewport.WorldToScreen(pt)))
                    return false;
            }
            return true;
        }

        private static bool EntityIntersectsRect(CadViewport viewport, Rect screenRect, Entity ent)
        {
            foreach (var pt in ent.GetGripPoints())
            {
                if (screenRect.Contains(viewport.WorldToScreen(pt)))
                    return true;
            }

            var tl = viewport.ScreenToWorld(screenRect.TopLeft);
            var tr = viewport.ScreenToWorld(screenRect.TopRight);
            var br = viewport.ScreenToWorld(screenRect.BottomRight);
            var bl = viewport.ScreenToWorld(screenRect.BottomLeft);

            var rectPoly = new Polyline(4) { Closed = true };
            rectPoly.AddVertexAt(0, Point2d.FromPoint3d(tl), 0.0, 0.0, 0.0);
            rectPoly.AddVertexAt(1, Point2d.FromPoint3d(tr), 0.0, 0.0, 0.0);
            rectPoly.AddVertexAt(2, Point2d.FromPoint3d(br), 0.0, 0.0, 0.0);
            rectPoly.AddVertexAt(3, Point2d.FromPoint3d(bl), 0.0, 0.0, 0.0);

            var points = new Point3dCollection();
            ent.IntersectWith(rectPoly, Intersect.OnBothOperands, points);
            return points.Count > 0;
        }
    }
}
