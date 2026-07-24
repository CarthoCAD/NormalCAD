using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Resources;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller
{
    internal class IdleState
    {
        private static string MsgFound => CommandResources.Get("CMD.MSG.FOUND");
        private static string MsgRemoved => CommandResources.Get("CMD.MSG.REMOVED");
        private static string MsgFoundN => CommandResources.Get("CMD.MSG.FOUND_N");
        private static string MsgRemovedN => CommandResources.Get("CMD.MSG.REMOVED_N");

        private CadController? _controller;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
            _controller.Viewport.SelectionStartPoint = null;
            _controller.Viewport.SelectionEndPoint = null;
            _controller.InputManager.RegisterGetEntity(
                new PromptEntityOptions(),
                OnEntityPick);
        }

        public void Deactivate()
        {
            if (_controller != null)
            {
                _controller.InputManager.ClearAllRegistrations();
                _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
                _controller.Viewport.SelectionStartPoint = null;
                _controller.Viewport.SelectionEndPoint = null;
            }
        }

        private void RegisterGetEntity()
        {
            _controller!.InputManager.RegisterGetEntity(
                new PromptEntityOptions(),
                OnEntityPick);
            _controller.InputManager.ResetPromptToIdle();
        }

        private void OnEntityPick(PromptEntityResult result)
        {
            if (result.Status == PromptStatus.Cancel) return;

            if (result.Status == PromptStatus.OK)
            {
                ToggleEntitySelection(result.ObjectId);
                RegisterGetEntity();
                return;
            }

            _controller!.InputManager.RegisterGetSelection(
                new PromptSelectionOptions { BasePoint = result.PickedPoint },
                OnBoxSelection);
        }

        private void OnBoxSelection(PromptSelectionResult result)
        {
            if (result.Status != PromptStatus.OK)
            {
                RegisterGetEntity();
                return;
            }

            int changed = 0;
            bool isShift = _controller!.InputManager.IsShiftPressed;

            foreach (var id in result.Value)
            {
                if (isShift)
                {
                    if (_controller.IsSelected(id))
                    {
                        _controller.RemoveFromSelection(id);
                        changed++;
                    }
                }
                else
                {
                    if (!_controller.IsSelected(id))
                    {
                        _controller.AddToSelection(id);
                        changed++;
                    }
                }
            }

            int total = _controller!.SelectedEntityIds.Count;
            string message = string.Format(MsgFoundN, changed, total);
            _controller.InputManager.SetPromptMessage(message);
            _controller.Viewport.InvalidateVisual();

            RegisterGetEntity();
        }

        private void ToggleEntitySelection(ObjectId id)
        {
            if (_controller == null) return;

            bool isShift = _controller.InputManager.IsShiftPressed;

            if (isShift)
            {
                if (_controller.IsSelected(id))
                {
                    _controller.RemoveFromSelection(id);
                    int total = _controller.SelectedEntityIds.Count;
                    _controller.InputManager.SetPromptMessage(
                        string.Format(MsgRemoved, total));
                    _controller.Viewport.InvalidateVisual();
                }
            }
            else
            {
                if (!_controller.IsSelected(id))
                {
                    _controller.AddToSelection(id);
                    int total = _controller.SelectedEntityIds.Count;
                    _controller.InputManager.SetPromptMessage(
                        string.Format(MsgFound, total));
                    _controller.Viewport.InvalidateVisual();
                }
            }
        }
    }
}
