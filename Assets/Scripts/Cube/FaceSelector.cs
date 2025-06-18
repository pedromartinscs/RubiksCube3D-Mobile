using UnityEngine;
using UnityEngine.InputSystem;

public class FaceSelector : MonoBehaviour
{
    public LayerMask faceLayer;
    public float dragThreshold = 0.1f;

    private Camera mainCam;
    private GameInput input;
    private Vector2 startPos;
    private GameObject selectedCubelet;
    private string hitFaceName;

    void Awake()
    {
		input = new GameInput();
        mainCam = Camera.main;
        input.Player.Look.started += ctx => OnPointerDown(ctx);
        input.Player.Look.canceled += ctx => OnPointerUp(ctx);
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void OnPointerDown(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, faceLayer))
        {
            selectedCubelet = hit.collider.transform.root.gameObject;
            hitFaceName = hit.collider.name; // e.g., Face_Up
            startPos = screenPos;
        }
    }

    void OnPointerUp(InputAction.CallbackContext ctx)
    {
        if (selectedCubelet == null) return;

        Vector2 endPos = Mouse.current.position.ReadValue();
        Vector2 delta = endPos - startPos;

        if (delta.magnitude < dragThreshold)
        {
            selectedCubelet = null;
            return;
        }

        // Determine swipe direction
        bool swipeDir = GetSwipeClockwise(hitFaceName, delta);

        Object.FindFirstObjectByType<CubeManager>().RotateFace(hitFaceName, swipeDir);

        selectedCubelet = null;
    }
	
	bool GetSwipeClockwise(string faceName, Vector2 delta)
	{
		if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) // Horizontal swipe
		{
			return faceName switch
			{
				"Face_Up"      => delta.x > 0,  // Right swipe = clockwise
				"Face_Down"    => delta.x < 0,  // Left swipe = clockwise (flipped)
				"Face_Forward" => delta.x < 0,  // Left swipe = clockwise
				"Face_Back"    => delta.x > 0,  // Right swipe = clockwise
				_              => true
			};
		}
		else // Vertical swipe
		{
			return faceName switch
			{
				"Face_Left"    => delta.y < 0,  // Down swipe = clockwise
				"Face_Right"   => delta.y > 0,  // Up swipe = clockwise
				_              => true
			};
		}
	}
}
