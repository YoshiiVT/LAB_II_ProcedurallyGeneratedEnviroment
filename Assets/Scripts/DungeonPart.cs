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
    public bool HasAvailableEntrypoint(out Transform entrypoint, out EntrypointSize entrypointSize)
    {
        entrypoint = null;
        entrypointSize = EntrypointSize.Small;

        if (entrypoints == null || entrypoints.Count == 0)
            return false;

        // Special case: only one entry
        if (entrypoints.Count == 1)
        {
            Transform entry = entrypoints[0];
            if (entry.TryGetComponent<EntryPoint>(out EntryPoint res) && !res.IsOccupied())
            {
                entrypoint = entry;
                entrypointSize = res.entrypointSize;
                return true;
            }
            return false;
        }

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

        entrypoint = unoccupied[Random.Range(0, unoccupied.Count)];

        if (entrypoint.TryGetComponent<EntryPoint>(out EntryPoint chosenEP))
        {
            entrypointSize = chosenEP.entrypointSize;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Marks the given entrypoint as used.
    /// </summary>
    public void UseEntrypoint(Transform entrypoint)
    {
        if (entrypoint != null && entrypoint.TryGetComponent<EntryPoint>(out EntryPoint entry))
        {
            entry.SetOccupied(true);
            Debug.Log($"[GEN] Entrypoint at {entrypoint.name} in {entrypoint.transform.parent?.name} marked as OCCUPIED.");
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
            Debug.Log($"[GEN] Unusing entrypoint at {entrypoint.name} in {entrypoint.transform.parent?.name}");
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (var entry in entrypoints)
        {
            if (entry == null) continue;
            Gizmos.DrawRay(entry.position, entry.forward * 1.5f);
        }
    }
}