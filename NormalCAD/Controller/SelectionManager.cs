using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.View.Controls;
using CoreApp = NormalCAD.Core.ApplicationServices.Application;

namespace NormalCAD.Controller
{
    public class SelectionManager
    {
        private enum SelState { Idle, Picking, Boxing }

        private static string MsgFound => CommandResources.Get("CMD.MSG.FOUND");
        private static string MsgRemoved => CommandResources.Get("CMD.MSG.REMOVED");
        private static string MsgFoundN => CommandResources.Get("CMD.MSG.FOUND_N");
        private static string MsgRemovedN => CommandResources.Get("CMD.MSG.REMOVED_N");
        private static string PromptOppositeCorner => CommandResources.Get("CMD.PROMPT.OPPOSITECORNER");

        private readonly CadController _controller;
        private readonly InputManager _inputManager;

        private SelState _state;
        private Action<PromptEntityResult>? _entityCallback;
        private Action<PromptSelectionResult>? _selectionCallback;
        private bool _isShift;
        private Point _boxStart, _boxEnd;

        public bool IsActive => _state != SelState.Idle;
        public bool IsShiftPressed => _isShift;

        public SelectionManager(CadController controller, InputManager inputManager)
        {
            _controller = controller;
            _inputManager = inputManager;
        }

        public void BeginGetEntity(PromptEntityOptions options, Action<PromptEntityResult> callback)
        {
            _state = SelState.Picking;
            _entityCallback = callback;
            _selectionCallback = null;
            _inputManager.ResetPromptToCommand();
        }

        public void BeginGetSelection(PromptSelectionOptions options, Action<PromptSelectionResult> callback)
        {
            _state = SelState.Boxing;
            _selectionCallback = callback;
            _entityCallback = null;

            var viewport = _controller.Viewport;
            _boxStart = options.BasePoint.HasValue
                ? viewport.WorldToScreen(options.BasePoint.Value)
                : default;
            _boxEnd = _boxStart;
            _isShift = false;

            viewport.SelectionStartPoint = _boxStart;
            viewport.SelectionEndPoint = _boxEnd;
            _inputManager.SetCurrentPrompt("", PromptOppositeCorner);
            viewport.InvalidateVisual();
        }

        public void OnPointerPressed(Point3d worldPt, Point screenPos, bool isShift)
        {
            _isShift = isShift;

            if (_state == SelState.Boxing)
            {
                _boxEnd = screenPos;
                _controller.Viewport.SelectionEndPoint = _boxEnd;
                FinishBox();
                return;
            }

            if (_state == SelState.Picking && _entityCallback != null)
            {
                TryPickOrStartBox(worldPt, screenPos);
            }
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_state != SelState.Boxing) return;

            var viewport = _controller.Viewport;
            _boxEnd = viewport.WorldToScreen(worldPt);
            viewport.SelectionEndPoint = _boxEnd;
            viewport.InvalidateVisual();
        }

        public void Cancel()
        {
            _state = SelState.Idle;
            _entityCallback = null;
            _selectionCallback = null;
            _controller.Viewport.SelectionStartPoint = null;
            _controller.Viewport.SelectionEndPoint = null;
        }

        private void TryPickOrStartBox(Point3d worldPt, Point screenPos)
        {
            var viewport = _controller.Viewport;
            var db = CoreApp.DocumentManager.MdiActiveDocument?.Database;

            ObjectId selectedId = ObjectId.Null;
            bool didHitTest = false;

            if (db != null)
            {
                double bestScreenDist = 10.0;
                double zoom = Math.Max(viewport.Zoom, 0.001);

                if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
                {
                    var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                    if (!modelSpaceId.IsNull && db.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                    {
                        double worldTolerance = bestScreenDist / zoom;
                        var worldMouse = viewport.ScreenToWorld(screenPos);
                        var candidateIds = btr.QueryNearPoint(worldMouse, worldTolerance);

                        foreach (var entId in candidateIds)
                        {
                            if (db.TryGetObject(entId, out var entObj) && entObj is Entity ent)
                            {
                                double screenDist = ent.GetDistanceTo(worldMouse) * zoom;
                                if (screenDist < bestScreenDist)
                                {
                                    bestScreenDist = screenDist;
                                    selectedId = entId;
                                }
                            }
                        }
                        didHitTest = true;
                    }
                }
            }

            if (!didHitTest)
            {
                _entityCallback?.Invoke(new PromptEntityResult(
                    PromptStatus.None, ObjectId.Null, worldPt, ""));
            }
            else if (!selectedId.IsNull)
            {
                _entityCallback?.Invoke(new PromptEntityResult(
                    PromptStatus.OK, selectedId, worldPt, ""));
            }
            else
            {
                _entityCallback?.Invoke(new PromptEntityResult(
                    PromptStatus.None, ObjectId.Null, worldPt, ""));
            }
        }

        private void FinishBox()
        {
            var viewport = _controller.Viewport;
            var db = CoreApp.DocumentManager.MdiActiveDocument?.Database;
            if (db == null)
            {
                _selectionCallback?.Invoke(PromptSelectionResult.Cancelled);
                return;
            }

            var screenRect = GetRect(_boxStart, _boxEnd);
            bool isCrossing = _boxEnd.X < _boxStart.X;

            var toSelect = new ObjectIdCollection();

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
                        if (db.TryGetObject(entId, out var entObj) && entObj is Entity ent)
                        {
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
            }

            _controller.Viewport.SelectionStartPoint = null;
            _controller.Viewport.SelectionEndPoint = null;

            _selectionCallback?.Invoke(PromptSelectionResult.OK(toSelect));
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
