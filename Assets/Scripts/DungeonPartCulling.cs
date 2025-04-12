using UnityEngine;

public class DungeonPartCulling : MonoBehaviour
{
    public GameObject renderDistanceObject;
    private CapsuleCollider capsule;
    private BoxCollider boxCollider;
    private Renderer[] renderers;
    private bool isInside;

    void Start()
    {
        if (renderDistanceObject == null)
        {
            Debug.LogError("RenderDistance object not assigned.");
            enabled = false;
            return;
        }

        capsule = renderDistanceObject.GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            Debug.LogError("RenderDistance object does not have a CapsuleCollider.");
            enabled = false;
            return;
        }

        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("This object does not have a BoxCollider.");
            enabled = false;
            return;
        }

        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        CheckIfOverlapping();
        SetRenderersVisible();
    }

    void CheckIfOverlapping()
    {
        if (boxCollider == null || capsule == null)
        {
            Debug.LogWarning("Colliders not initialized properly.");
            return;
        }

        // Compare world-space bounds
        Bounds boxBounds = boxCollider.bounds;
        Bounds capsuleBounds = capsule.bounds;

        bool overlap = boxBounds.Intersects(capsuleBounds);

        if (overlap != isInside)
        {
            Debug.Log($"Render distance changed: {(overlap ? "Inside" : "Outside")}");
        }

        isInside = overlap;
    }
    void SetRenderersVisible()
    {
        foreach (Renderer rend in renderers)
        {
            rend.enabled = isInside;
        }
    }
}
