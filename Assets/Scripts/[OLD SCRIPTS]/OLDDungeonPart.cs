using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;

public class OLDDungeonPart : GameBehaviour

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

    private GameObject fillerWall;

    public List<Transform> entrypoints;

    public new Collider collider;

    /// <summary>

    /// Searches through all the entrypoints in the game and finds the closest available one.

    /// </summary>

    /// <param name="entrypoint"></param>

    /// <returns></returns>

    public bool HasAvailableEntrypoint(out Transform entrypoint)

    {

        entrypoint = null;

        float closestDistance = float.MaxValue;

        foreach (Transform entry in entrypoints)

        {

            if (entry.TryGetComponent<EntryPoint>(out EntryPoint entryPoint) && !entryPoint.IsOccupied())

            {

                float distance = Vector3.Distance(transform.position, entry.position);

                if (distance < closestDistance)

                {

                    closestDistance = distance;

                    entrypoint = entry;

                }

            }

        }

        if (entrypoint != null)

        {

            entrypoint.GetComponent<EntryPoint>().SetOccupied(true);

            Debug.Log($"Found available entry point at {entrypoint.position}");

            return true;

        }

        Debug.Log("No available entry points found.");

        return false;

    }

    /// <summary>

    /// Unsets an entry point so it can be used again.

    /// </summary>

    /// <param name="entrypoint"></param>

    public void UnuseEntrypoint(Transform entrypoint)

    {

        if (entrypoint.TryGetComponent<EntryPoint>(out EntryPoint entry))

        {

            entry.SetOccupied(false);

            Debug.Log($"Entry point at {entrypoint.position} freed up.");

        }

    }

    /// <summary>

    /// This function is executed at the end of the generation process.

    /// It searches all the entry points and detects if they are occupied.

    /// If not, it fills them in with walls.

    /// </summary>

    public void FillEmptyDoors()

    {

        entrypoints.ForEach((entry) =>

        {

            if (entry.TryGetComponent(out EntryPoint entryPoint))

            {

                if (!entryPoint.IsOccupied())

                {

                    GameObject wall = Instantiate(fillerWall);

                    wall.transform.position = entry.transform.position;

                    wall.transform.rotation = entry.transform.rotation;

                }

            }

        });

    }

    private void OnDrawGizmos()

    {

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);

    }

}

