using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BasePressInputHandler : MonoBehaviour
{
    protected InputAction m_PressAction;
    protected InputAction m_DragAction;

    protected virtual void Awake()
    {
        m_PressAction = new InputAction("touch", binding: "<Pointer>/press");

        m_PressAction.started += ctx =>
        {
            if (ctx.control.device is Pointer device)
            {
                OnPressBegan(device.position.ReadValue());
                m_DragAction.Enable(); // Start dragging when pressing starts
            }
        };

        m_PressAction.performed += ctx =>
        {
            if (ctx.control.device is Pointer device)
            {
                OnPress(device.position.ReadValue());
            }
        };

        m_PressAction.canceled += _ =>
        {
            OnPressCancel();
            m_DragAction.Disable(); // Stop dragging when pressing is cancelled
        };

        m_DragAction = new InputAction("drag", InputActionType.PassThrough);
        m_DragAction.AddCompositeBinding("Dpad")
            .With("Up", "<Pointer>/delta/y")
            .With("Down", "<Pointer>/delta/-y") // Negative Y delta for Down
            .With("Left", "<Pointer>/delta/-x") // Negative X delta for Left
            .With("Right", "<Pointer>/delta/x");

        m_DragAction.performed += ctx =>
        {
            if (ctx.control.device is Pointer device)
            {
                OnDrag(device.delta.ReadValue());
            }
        };

        m_DragAction.canceled += _ => OnDragEnd();
    }

    protected virtual void OnDrag(Vector2 delta) { }

    protected virtual void OnEnable()
    {
        m_PressAction.Enable();
    }

    protected virtual void OnDisable()
    {
        m_PressAction.Disable();
        m_DragAction.Disable();
    }

    protected virtual void OnDragEnd() { }

    protected virtual void OnDestroy()
    {
        m_PressAction.Dispose();
        m_DragAction.Dispose();
    }

    protected virtual void OnPress(Vector3 position) { }

    protected virtual void OnPressBegan(Vector3 position) { }

    protected virtual void OnPressCancel() { }
}
