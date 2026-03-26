using UnityEngine;
using UnityEngine.InputSystem;
using Factory.Core;

public class TopDownCameraController : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float panSpeed = 10f;
    [SerializeField] private float rotationSpeed = 50f;

    private Vector2 dragOrigin;
    private Vector3 pivotPoint;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Zoom avec la molette
        float scroll = Mouse.current.scroll.y.ReadValue();
        if (Mathf.Abs(scroll) > 0.1f)
        {
            float sensitivity = SettingsManager.Instance?.settings.mouseSensitivity ?? 1f;
            float invert = (SettingsManager.Instance?.settings.invertY ?? false) ? -1f : 1f;
            Vector3 zoomDirection = transform.forward * scroll * zoomSpeed * sensitivity * invert * Time.deltaTime;
            Vector3 newPosition = transform.position + zoomDirection;
            // Limite le zoom
            if (newPosition.y > minZoom && newPosition.y < maxZoom)
            {
                transform.position = newPosition;
            }
        }

        // Déplacement avec le clic du milieu
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            dragOrigin = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 delta = currentMousePosition - dragOrigin;

            // Utilise les vecteurs right et forward de la caméra (en ignorant Y)
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            forward.y = 0; // Ignore la composante Y pour le déplacement avant/arrière
            right.y = 0;  // Ignore la composante Y pour le déplacement droite/gauche

            // Normalise les vecteurs pour éviter les distorsions
            right.Normalize();
            forward.Normalize();

            // Calcule le déplacement en fonction du delta de la souris
            float sensitivity = SettingsManager.Instance?.settings.mouseSensitivity ?? 1f;
            float invertY = (SettingsManager.Instance?.settings.invertY ?? false) ? -1f : 1f;
            Vector3 move = (-delta.x * right + (-delta.y * invertY) * forward) * panSpeed * sensitivity * Time.deltaTime;
            transform.Translate(move, Space.World);

            dragOrigin = currentMousePosition;
        }

        // Rotation autour du point central de l'écran avec Q et E
        if (Keyboard.current.qKey.isPressed)
        {
            pivotPoint = GetWorldPositionAtScreenCenter();
            transform.RotateAround(pivotPoint, Vector3.up, rotationSpeed * Time.deltaTime);
        }
        else if (Keyboard.current.eKey.isPressed)
        {
            pivotPoint = GetWorldPositionAtScreenCenter();
            transform.RotateAround(pivotPoint, Vector3.up, -rotationSpeed * Time.deltaTime);
        }
    }

    // Récupère la position du monde au centre de l'écran
    private Vector3 GetWorldPositionAtScreenCenter()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}