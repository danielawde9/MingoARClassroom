using UnityEngine;
using UnityEngine.InputSystem;

namespace MingoData.Scripts.ARUtil
{
    public abstract class BasePressInputHandler : MonoBehaviour
    {
        private InputAction pressAction;
        private InputAction dragAction;
        private Vector2 startDragPosition;
        private float previousDistance;

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
                if (ctx.control.device is not Pointer device)
                    return;
                OnDragEnd(device.position.ReadValue());
                if (IsSwipeUp(startDragPosition, device.position.ReadValue()))
                {
                    OnSwipeUp();
                }
            };
            //
            // m_PinchAction = new InputAction("pinch", binding: "<Touchscreen>/touch");
            // m_PinchAction.performed += ctx =>
            // {
            //     Debug.Log("Pinch action triggered");
            //
            //     if (ctx.control.device is not Touchscreen device)
            //         return;
            //     TouchControl touch0 = device.touches[0];
            //     TouchControl touch1 = device.touches[1];
            //     if (!touch0.isInProgress || !touch1.isInProgress) // Only react when two fingers are touching
            //         return;
            //     float currentDistance = Vector2.Distance(touch0.position.ReadValue(), touch1.position.ReadValue());
            //     if (previousDistance > 0)
            //     {
            //         if (currentDistance > previousDistance)
            //         {
            //             OnPinchOut();
            //         }
            //         else if (currentDistance < previousDistance)
            //         {
            //             OnPinchIn();
            //         }
            //     }
            //     previousDistance = currentDistance;
            // };
            // m_PinchAction.canceled += _ =>
            // {
            //     previousDistance = 0;
            // };
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
