using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonPart : GameBehaviour
{
   public enum DungeonPartType
    {
        //Enum assigns numeric value to words
        Room, //Value = 0
        Hallway //Value = 1
        //Another example 'Value = 2
    }

    [SerializeField]
    private LayerMask roomsLayermask; //Defined before in DungeonGenerator.cs

    [SerializeField]
    private DungeonPartType dungeonPartType; //Apparently "Irrelevant"

    [SerializeField]
    private GameObject fillerWall; //This fills out empty enterance points

    public List<Transform> entrypoints; //This list will contain all the entrypoints generated in the dungeon.

    public new Collider collider; //This ill tell the generator if a room is colliding with another room

    /// <summary>
    /// Searches through all the entrypoints in the game and brings back a list of ones that are not occupied
    /// </summary>
    /// <param name="entrypoint"></param>
    /// <returns></returns>
    public bool HasAvailableEntrypoint(out Transform entrypoint)
    {
        Transform resultingEntry = null;
        bool result  = false;

        int totalRetries = 10;
        int retryIndex = 1;

        if (entrypoints.Count == 1) //This script will only run if there is only one entrypoint
        {
            Transform entry = entrypoints[0];
            if (entry.TryGetComponent<EntryPoint>(out EntryPoint res))
            {
                if (res.IsOccupied())
                {
                    result = false;
                    resultingEntry = null;
                }
                else
                {
                    result = true;
                    resultingEntry = entry;
                    res.SetOccupied();
                }
                entrypoint = resultingEntry;
                return result;
                //this checks if the one entrypoint is occupied and returns a result
            }
        }

        while (resultingEntry == null && retryIndex < totalRetries) //This is if there are multiple entries to the room
        {

            int randomEntryIndex = Random.Range(0, entrypoints.Count);

            Transform entry = entrypoints[randomEntryIndex];

            if (entry.TryGetComponent<EntryPoint>(out EntryPoint entryPoint))
            {
                if (!entryPoint.IsOccupied())
                {
                    resultingEntry = entry;
                    result = true;
                    entryPoint.SetOccupied();
                    break;
                }
            }
            retryIndex++;
        }

        entrypoint = resultingEntry;
        return result;
        // Same as before, except it checks multiple entries to see if they are occupied.
    }

    private object GetClosestObject(Transform transform, List<Transform> entrypoints)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// This function is run when the script detects overlapping rooms to stop by unusing then reusing the entrypoint.
    /// </summary>
    /// <param name="entrypoint"></param>
    public void UnuseEntrypoint(Transform entrypoint)
    {
        if (entrypoint.TryGetComponent<EntryPoint>(out EntryPoint entry))
        {
            entry.SetOccupied(false);
        }
    }

    /// <summary>
    /// This is done at the end of the generation process
    /// This function searches all the entry points and detects if they are occupied.
    /// If not it fills it in.
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

