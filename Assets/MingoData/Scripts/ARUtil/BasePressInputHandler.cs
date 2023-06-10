using UnityEngine;
using UnityEngine.InputSystem;

namespace MingoData.Scripts.ARUtil
{
    public abstract class BasePressInputHandler : MonoBehaviour
    {
        private InputAction pressAction;
        private InputAction dragAction;
        private Vector2 startDragPosition;

        protected virtual void Awake()
        {
            pressAction = new InputAction("touch", binding: "<Pointer>/press");

            pressAction.started += ctx =>
            {
                if (ctx.control.device is not Pointer device)
                    return;
                OnPressBegan(device.position.ReadValue());
                dragAction.Enable(); // Start dragging when pressing starts
                startDragPosition = device.position.ReadValue();
            };

            pressAction.performed += ctx =>
            {
                if (ctx.control.device is Pointer device)
                {
                    OnPress(device.position.ReadValue());
                }
            };

            pressAction.canceled += _ =>
            {
                OnPressCancel();
                dragAction.Disable(); // Stop dragging when pressing is cancelled
            };

            dragAction = new InputAction("drag", InputActionType.PassThrough);
            dragAction.AddCompositeBinding("Dpad")
                    .With("Up", "<Pointer>/delta/y")
                    .With("Down", "<Pointer>/delta/-y") // Negative Y delta for Down
                    .With("Left", "<Pointer>/delta/-x") // Negative X delta for Left
                    .With("Right", "<Pointer>/delta/x");

            dragAction.performed += ctx =>
            {
                if (ctx.control.device is Pointer device)
                {
                    OnDrag(device.delta.ReadValue());
                }
            };

            dragAction.canceled += ctx =>
            {
                if (ctx.control.device is Pointer device)
                {
                    OnDragEnd(device.position.ReadValue());
                    if (IsSwipeUp(startDragPosition, device.position.ReadValue()))
                    {
                        OnSwipeUp();
                    }
                }
            };
        }

        protected virtual void OnEnable()
        {
            pressAction.Enable();
            dragAction.Enable();
        }

        protected virtual void OnDisable()
        {
            pressAction.Disable();
            dragAction.Disable();
        }

        protected virtual void OnDestroy()
        {
            pressAction.Dispose();
            dragAction.Dispose();
        }

        private static bool IsSwipeUp(Vector2 start, Vector2 end)
        {
            return (end.y - start.y) > 0; // If the end position is higher than the start, it's a swipe up
        }

        protected virtual void OnSwipeUp() { }
        protected virtual void OnDrag(Vector2 delta) { }
        protected virtual void OnDragEnd(Vector2 endPosition) { }
        protected virtual void OnPress(Vector3 position) { }
        protected virtual void OnPressBegan(Vector3 position) { }
        protected virtual void OnPressCancel() { }
    }
}
