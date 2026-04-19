using UnityEngine;

public class SwipeTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    public float trailTime = 0.3f;
    public float startWidth = 0.3f;
    public float endWidth = 0.05f;
    public Color startColor = new Color(1f, 1f, 1f, 0.8f);
    public Color endColor = new Color(1f, 1f, 1f, 0f);

    [Header("Material")]
    public Material trailMaterial;

    private TrailRenderer trail;
    private Camera mainCam;
    private bool isSwiping;

    private void Start()
    {
        mainCam = Camera.main;

        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = trailTime;
        trail.startWidth = startWidth;
        trail.endWidth = endWidth;
        trail.startColor = startColor;
        trail.endColor = endColor;
        trail.numCornerVertices = 5;
        trail.numCapVertices = 5;
        trail.minVertexDistance = 0.05f;
        trail.sortingOrder = 100;

        if (trailMaterial != null)
        {
            trail.material = trailMaterial;
        }
        else
        {
            trail.material = new Material(Shader.Find("Sprites/Default"));
        }

        trail.emitting = false;
        // Start off-screen so stale trail points don't flash
        transform.position = new Vector3(-999, -999, 0);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = GetWorldMousePosition();
            // Teleport without leaving a trail
            trail.Clear();
            transform.position = worldPos;
            trail.emitting = true;
            isSwiping = true;
        }

        if (isSwiping)
        {
            transform.position = GetWorldMousePosition();
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            trail.emitting = false;
            isSwiping = false;
        }
    }

    private Vector3 GetWorldMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCam.transform.position.z);
        return mainCam.ScreenToWorldPoint(mousePos);
    }
}
