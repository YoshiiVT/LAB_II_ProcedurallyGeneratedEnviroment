using System.Collections.Generic;
using UnityEngine;

public class DungeonPart : MonoBehaviour
{
    public enum DungeonPartType
    {
        Room,
        Hallway
    }

    [SerializeField]
    private LayerMask roomsLayermask;

    [SerializeField]
    private DungeonPartType dungeonPartType;

    [SerializeField]
    private GameObject fillerwall;

    public List<Transform> entrypoints;

    public new Collider collider;

    private void Awake()
    {
        if (collider == null)
        {
            collider = GetComponent<Collider>();
        }
    }

    /// <summary>
    /// Finds a random available entrypoint but does NOT mark it as used.
    /// </summary>
    public bool HasAvailableEntrypoint(out Transform entrypoint)
    {
        entrypoint = null;

        if (entrypoints == null || entrypoints.Count == 0)
            return false;

        // Special case: only one entry
        if (entrypoints.Count == 1)
        {
            Transform entry = entrypoints[0];
            if (entry.TryGetComponent<EntryPoint>(out EntryPoint res) && !res.IsOccupied())
            {
                entrypoint = entry;
                return true;
            }
            return false;
        }

        // Collect all unoccupied entrypoints
        List<Transform> unoccupied = new List<Transform>();
        foreach (var ep in entrypoints)
        {
            if (ep.TryGetComponent<EntryPoint>(out EntryPoint entryPt) && !entryPt.IsOccupied())
            {
                unoccupied.Add(ep);
            }
        }

        if (unoccupied.Count == 0)
            return false;

        // Pick one at random
        entrypoint = unoccupied[Random.Range(0, unoccupied.Count)];
        return true;
    }

    /// <summary>
    /// Marks the given entrypoint as used.
    /// </summary>
    public void UseEntrypoint(Transform entrypoint)
    {
        if (entrypoint != null && entrypoint.TryGetComponent<EntryPoint>(out EntryPoint entry))
        {
            entry.SetOccupied(true);
        }
    }

    /// <summary>
    /// Reverses entrypoint usage, used on failed placements.
    /// </summary>
    public void UnuseEntrypoint(Transform entrypoint)
    {
        if (entrypoint != null && entrypoint.TryGetComponent<EntryPoint>(out EntryPoint entry))
        {
            entry.SetOccupied(false);
        }
    }

    /// <summary>
    /// Fills unconnected entrypoints with wall objects.
    /// </summary>
    public void FillEmptyDoors()
    {
        foreach (var entry in entrypoints)
        {
            if (entry.TryGetComponent(out EntryPoint entryPoint))
            {
                if (!entryPoint.IsOccupied())
                {
                    GameObject wall = Instantiate(fillerwall);
                    wall.transform.position = entry.transform.position;
                    wall.transform.rotation = entry.transform.rotation;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (collider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
        }
    }
}