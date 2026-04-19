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
    private bool startNextFrame;

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
        transform.position = new Vector3(-999, -999, 0);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Stop emitting, teleport, and clear so no segment is drawn from the old position
            trail.emitting = false;
            transform.position = GetWorldMousePosition();
            trail.Clear();
            startNextFrame = true;
            isSwiping = true;
        }

        // Enable emitting one frame after the teleport so the TrailRenderer
        // doesn't connect from the previous release position
        if (startNextFrame && !Input.GetMouseButtonDown(0))
        {
            trail.emitting = true;
            startNextFrame = false;
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
