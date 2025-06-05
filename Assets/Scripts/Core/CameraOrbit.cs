using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float sensitivity = 3f;
    public float minY = 10f;
    public float maxY = 80f;

    private Vector2 lookDelta;
    private Vector3 angles;
    private GameInput input;

    void Awake()
    {
		input = new GameInput();
        input.Player.Look.performed += ctx => lookDelta = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += _ => lookDelta = Vector2.zero;
    }

    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }

    void Start()
    {
        angles = transform.eulerAngles;
    }

    void Update()
    {
        angles.x -= lookDelta.y * sensitivity * Time.deltaTime;
        angles.y += lookDelta.x * sensitivity * Time.deltaTime;

        angles.x = Mathf.Clamp(angles.x, minY, maxY);
        transform.rotation = Quaternion.Euler(angles);

        if (target != null)
            transform.position = target.position - transform.forward * 5f;
    }
}
