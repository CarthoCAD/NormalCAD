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

        public void Activate()
        {
            CadController.Current.Viewport.CurrentCursorState = CadCursorState.PickCross;
            CadController.Current.Viewport.SelectionStartPoint = null;
            CadController.Current.Viewport.SelectionEndPoint = null;
            CadController.Current.InputManager.RegisterGetEntity(
                new PromptEntityOptions(),
                OnEntityPick);
        }

        public void Deactivate()
        {
            CadController.Current.InputManager.ClearAllRegistrations();
            CadController.Current.Viewport.CurrentCursorState = CadCursorState.PickCross;
            CadController.Current.Viewport.SelectionStartPoint = null;
            CadController.Current.Viewport.SelectionEndPoint = null;
        }

        private void RegisterGetEntity()
        {
            CadController.Current.InputManager.RegisterGetEntity(
                new PromptEntityOptions(),
                OnEntityPick);
            CadController.Current.InputManager.ResetPromptToIdle();
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

            CadController.Current.InputManager.RegisterGetSelection(
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
            bool isShift = CadController.Current.InputManager.IsShiftPressed;

            foreach (var id in result.Value)
            {
                if (isShift)
                {
                    if (CadController.Current.IsSelected(id))
                    {
                        CadController.Current.RemoveFromSelection(id);
                        changed++;
                    }
                }
                else
                {
                    if (!CadController.Current.IsSelected(id))
                    {
                        CadController.Current.AddToSelection(id);
                        changed++;
                    }
                }
            }

            int total = CadController.Current.SelectedEntityIds.Count;
            string message = string.Format(MsgFoundN, changed, total);
            CadController.Current.InputManager.SetPromptMessage(message);
            CadController.Current.Viewport.InvalidateVisual();

            RegisterGetEntity();
        }

        private void ToggleEntitySelection(ObjectId id)
        {
            bool isShift = CadController.Current.InputManager.IsShiftPressed;

            if (isShift)
            {
                if (CadController.Current.IsSelected(id))
                {
                    CadController.Current.RemoveFromSelection(id);
                    int total = CadController.Current.SelectedEntityIds.Count;
                    CadController.Current.InputManager.SetPromptMessage(
                        string.Format(MsgRemoved, total));
                    CadController.Current.Viewport.InvalidateVisual();
                }
            }
            else
            {
                if (!CadController.Current.IsSelected(id))
                {
                    CadController.Current.AddToSelection(id);
                    int total = CadController.Current.SelectedEntityIds.Count;
                    CadController.Current.InputManager.SetPromptMessage(
                        string.Format(MsgFound, total));
                    CadController.Current.Viewport.InvalidateVisual();
                }
            }
        }
    }
}
