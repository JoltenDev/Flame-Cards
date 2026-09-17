using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CinemachineOrbitalFollow camOrbital;
    [SerializeField] CinemachineInputAxisController camInput;
    [SerializeField] CharacterController controller;

    [Header("Camera Settings")]
    [SerializeField] float moveSpeed;
    [SerializeField] float minZoom;
    [SerializeField] float maxZoom;

    [Header("Inputs")]
    [SerializeField] InputActionReference moveRef;
    [SerializeField] InputActionReference rightClickRef;
    [SerializeField] InputActionReference scrollRef;

    void Start()
    {
        moveRef.action.Enable();
        rightClickRef.action.Enable();
        scrollRef.action.Enable();
        
        scrollRef.action.performed += Zoom;
    }

    void Zoom(InputAction.CallbackContext ctx)
    {
        float direction = ctx.ReadValue<Vector2>().y;
        float radius = camOrbital.Radius;

        radius -= direction;

        if (radius > minZoom && radius < maxZoom)
            camOrbital.Radius = radius;
    }

    void Update()
    {
        Move();
        Rotate();
    }

    void Move()
    {
        Vector2 input = moveRef.action.ReadValue<Vector2>();
        Vector3 forward = camInput.transform.forward.normalized;
        Vector3 right = camInput.transform.right.normalized;

        forward.y = 0;
        right.y = 0;

        Vector3 direction = (forward * input.y + right * input.x).normalized;

        controller.Move(direction * moveSpeed * Time.deltaTime);
    }

    void Rotate()
    {
        if (rightClickRef.action.IsPressed())
        {
            camInput.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;

            return;
        }

        camInput.enabled = false;
        Cursor.lockState = CursorLockMode.None;
    }
}
