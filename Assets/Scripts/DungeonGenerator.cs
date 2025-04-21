using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DungeonGenerator : Singleton<DungeonGenerator>
{
    [SerializeField]
    private GameObject enterance;

    [SerializeField]
    private List<GameObject> rooms;

    [SerializeField]
    private List<GameObject> deadEnd;

    [SerializeField]
    private List<GameObject> hallways;

    [SerializeField]
    private GameObject door;

    [SerializeField]
    private GameObject lockedDoor;

    //public int noOfRooms = 10;

    [SerializeField]
    private LayerMask roomsLayermask;

    [SerializeField]
    private GameObject RoomGenerationDistance;

    [SerializeField]
    private GameObject RenderDistance;

    private List<DungeonPart> generatedRooms;

    private bool isGenerated = false;

    private void Start()
    {
        generatedRooms = new List<DungeonPart>();
        StartGeneration();
    }

    public void StartGeneration()
    {
        GameObject generatedRoom = Instantiate(enterance, transform.position, transform.rotation);
        generatedRoom.transform.SetParent(null);

        if (generatedRoom.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
        {
            generatedRooms.Add(dungeonPart);
        }
        else
        {
            Debug.LogError("Generated room does not have DungeonPart component.");
        }
        StartCoroutine(GenerateDelay());
    }

    private IEnumerator GenerateDelay()
    {
        yield return new WaitForSeconds(0.1f);
        GenerateInEntrypoints();
    }

    private bool HandleIntersection(DungeonPart dungeonPart)
    {
        bool didIntersect = false;

        // Original overlap box check to see if the room overlaps with other rooms
        Collider[] hits = Physics.OverlapBox(
            dungeonPart.collider.bounds.center,
            dungeonPart.collider.bounds.size / 2,
            Quaternion.identity,
            roomsLayermask
        );

        foreach (Collider hit in hits)
        {
            if (hit != dungeonPart.collider)
            {
                didIntersect = true;
                break;
            }
        }

        // New: Check if any entrypoint is blocked by a wall
        foreach (Transform entry in dungeonPart.entrypoints)
        {
            if (entry.TryGetComponent<EntryPoint>(out EntryPoint entryComp) && !entryComp.IsOccupied())
            {
                Collider entryCol = entry.GetComponent<Collider>();
                if (entryCol == null) continue;

                Vector3 capsuleStart = entryCol.bounds.center + entry.transform.forward * 0.01f;
                Vector3 capsuleEnd = entryCol.bounds.center - entry.transform.forward * 0.01f;
                float radius = entryCol.bounds.extents.magnitude * 0.9f;

                Collider[] wallHits = Physics.OverlapCapsule(capsuleStart, capsuleEnd, radius);
                foreach (Collider wallHit in wallHits)
                {
                    if (wallHit.CompareTag("Wall"))
                    {
                        entryComp.SetOccupied();

                        // Spawn locked door
                        if (lockedDoor != null)
                        {
                            GameObject doorInstance = Instantiate(
                                lockedDoor,
                                entry.position,
                                entry.rotation,
                                dungeonPart.transform // parent it under the room
                            );
                        }

                        Debug.Log($"EntryPoint at {entry.position} is blocked by a wall. Marked as occupied.");
                        break;
                    }
                }
            }
        }

        return didIntersect;
    }

    private void AlignRooms(Transform room1, Transform room2, Transform room1Entry, Transform room2Entry)
    {
        // Rotate room2 so its entry faces room1's entry
        Quaternion targetRotation = Quaternion.LookRotation(-room1Entry.forward, Vector3.up);
        Quaternion currentRotation = Quaternion.LookRotation(room2Entry.forward, Vector3.up);

        Quaternion rotationOffset = targetRotation * Quaternion.Inverse(currentRotation);
        room2.rotation = rotationOffset * room2.rotation;

        // Align position
        Vector3 offset = room1Entry.position - room2Entry.position;
        room2.position += offset;

        Physics.SyncTransforms();
    }


    public List<DungeonPart> GetGeneratedRooms() => generatedRooms;

    public bool IsGenerated() => isGenerated;

    public void GenerateInEntrypoints()
    {
        if (RoomGenerationDistance == null)
        {
            Debug.LogError("RoomGenerationDistance is not assigned.");
            return;
        }

        CapsuleCollider capsule = RoomGenerationDistance.GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            Debug.LogError("RoomGenerationDistance does not have a CapsuleCollider.");
            return;
        }

        Vector3 point1 = RoomGenerationDistance.transform.position + capsule.center + Vector3.up * capsule.height / 2;
        Vector3 point2 = RoomGenerationDistance.transform.position + capsule.center - Vector3.up * capsule.height / 2;

        Collider[] hits = Physics.OverlapCapsule(point1, point2, capsule.radius, roomsLayermask);

        foreach (Collider col in hits)
        {
            DungeonPart part = col.GetComponent<DungeonPart>();
            if (part == null) continue;

            foreach (Transform entry in part.entrypoints)
            {
                if (!entry.TryGetComponent<EntryPoint>(out EntryPoint entryComp)) continue;
                if (entryComp.IsOccupied()) continue;

                EntrypointSize requiredSize = entryComp.entrypointSize;

                GameObject roomPrefab = null;

                int unoccupiedEntryCount = 0;
                if (RenderDistance != null)
                {
                    CapsuleCollider renderCapsule = RenderDistance.GetComponent<CapsuleCollider>();
                    if (renderCapsule != null)
                    {
                        Vector3 renderPoint1 = RenderDistance.transform.position + renderCapsule.center + Vector3.up * renderCapsule.height / 2;
                        Vector3 renderPoint2 = RenderDistance.transform.position + renderCapsule.center - Vector3.up * renderCapsule.height / 2;

                        Collider[] nearbyHits = Physics.OverlapCapsule(renderPoint1, renderPoint2, renderCapsule.radius, roomsLayermask);

                        foreach (Collider nearbyCol in nearbyHits)
                        {
                            DungeonPart dp = nearbyCol.GetComponent<DungeonPart>();
                            if (dp == null) continue;

                            foreach (Transform ep in dp.entrypoints)
                            {
                                if (ep.TryGetComponent<EntryPoint>(out EntryPoint epComp) && !epComp.IsOccupied())
                                {
                                    unoccupiedEntryCount++;
                                }
                            }
                        }
                    }
                }

                float roll = Random.value;

                if (roll < 0.33f && hallways.Count > 0)
                {
                    roomPrefab = hallways[Random.Range(0, hallways.Count)];
                }
                else if (roll < 0.66f && deadEnd.Count > 0 && unoccupiedEntryCount >= 2)
                {
                    roomPrefab = deadEnd[Random.Range(0, deadEnd.Count)];
                }
                else if (rooms.Count > 0)
                {
                    roomPrefab = rooms[Random.Range(0, rooms.Count)];
                }
                else
                {
                    Debug.LogError("No room prefabs available in any list.");
                    continue;
                }

                // Retry loop for matching entrypoint size
                int maxTries = 10;
                bool successfullyPlaced = false;

                for (int i = 0; i < maxTries; i++)
                {
                    GameObject newRoom = Instantiate(roomPrefab, transform.position, transform.rotation);
                    newRoom.transform.SetParent(null);

                    if (!newRoom.TryGetComponent<DungeonPart>(out DungeonPart newPart))
                    {
                        Debug.LogError("Generated room has no DungeonPart component.");
                        Destroy(newRoom);
                        break;
                    }

                    if (!newPart.HasAvailableEntrypoint(out Transform newEntry, out EntrypointSize newSize))
                    {
                        Debug.LogWarning("Generated room has no available entrypoint.");
                        Destroy(newRoom);
                        break;
                    }

                    if (newSize != requiredSize)
                    {
                        Destroy(newRoom);
                        continue; // Retry with same prefab or new selection if you randomize again
                    }

                    AlignRooms(part.transform, newRoom.transform, entry, newEntry);

                    if (HandleIntersection(newPart))
                    {
                        Debug.Log("Intersection detected, destroying room.");
                        Destroy(newRoom);

                        // Mark original entrypoint as occupied
                        entryComp.SetOccupied();

                        // Destroy any door already there
                        Collider[] overlapping = Physics.OverlapSphere(entry.position, 0.2f);
                        foreach (Collider col2 in overlapping)
                        {
                            if (col2.CompareTag("Door"))
                            {
                                Destroy(col2.gameObject);
                            }
                        }

                        if (lockedDoor != null)
                        {
                            Instantiate(lockedDoor, entry.position, entry.rotation, part.transform);
                        }

                        break;
                    }

                    // Place door
                    if (door != null)
                    {
                        float checkRadius = 0.1f;
                        Collider[] doorCheck = Physics.OverlapSphere(entry.position, checkRadius);

                        bool doorAlreadyExists = false;
                        foreach (Collider doorCol in doorCheck)
                        {
                            if (doorCol.CompareTag("Door"))
                            {
                                doorAlreadyExists = true;
                                break;
                            }
                        }

                        if (!doorAlreadyExists)
                        {
                            GameObject doorToAlign = Instantiate(door, entry.position, entry.rotation);
                            doorToAlign.transform.SetParent(null);
                        }
                    }

                    part.UseEntrypoint(entry);
                    newPart.UseEntrypoint(newEntry);
                    generatedRooms.Add(newPart);
                    successfullyPlaced = true;
                    break;
                }

                if (!successfullyPlaced)
                {
                    Debug.LogWarning($"Failed to place a room with matching entrypoint size at {entry.position}");
                }
            }
        }
    }

    private bool TryGenerateRoomAt(EntryPoint entryComp, DungeonPart part, Transform entry)
    {
        EntrypointSize requiredSize = entryComp.entrypointSize;
        GameObject roomPrefab = null;

        int unoccupiedEntryCount = 0;
        if (RenderDistance != null)
        {
            CapsuleCollider renderCapsule = RenderDistance.GetComponent<CapsuleCollider>();
            if (renderCapsule != null)
            {
                Vector3 renderPoint1 = RenderDistance.transform.position + renderCapsule.center + Vector3.up * renderCapsule.height / 2;
                Vector3 renderPoint2 = RenderDistance.transform.position + renderCapsule.center - Vector3.up * renderCapsule.height / 2;

                Collider[] nearbyHits = Physics.OverlapCapsule(renderPoint1, renderPoint2, renderCapsule.radius, roomsLayermask);

                foreach (Collider nearbyCol in nearbyHits)
                {
                    DungeonPart dp = nearbyCol.GetComponent<DungeonPart>();
                    if (dp == null) continue;

                    foreach (Transform ep in dp.entrypoints)
                    {
                        if (ep.TryGetComponent<EntryPoint>(out EntryPoint epComp) && !epComp.IsOccupied())
                        {
                            unoccupiedEntryCount++;
                        }
                    }
                }
            }
        }

        float roll = Random.value;
        if (roll < 0.33f && hallways.Count > 0)
            roomPrefab = hallways[Random.Range(0, hallways.Count)];
        else if (roll < 0.66f && deadEnd.Count > 0 && unoccupiedEntryCount >= 2)
            roomPrefab = deadEnd[Random.Range(0, deadEnd.Count)];
        else if (rooms.Count > 0)
            roomPrefab = rooms[Random.Range(0, rooms.Count)];
        else
            return false;

        // Retry loop
        int maxTries = 10;
        for (int i = 0; i < maxTries; i++)
        {
            GameObject newRoom = Instantiate(roomPrefab, transform.position, transform.rotation);
            newRoom.transform.SetParent(null);

            if (!newRoom.TryGetComponent<DungeonPart>(out DungeonPart newPart))
            {
                Destroy(newRoom);
                break;
            }

            if (!newPart.HasAvailableEntrypoint(out Transform newEntry, out EntrypointSize newSize))
            {
                Destroy(newRoom);
                break;
            }

            if (newSize != requiredSize)
            {
                Destroy(newRoom);
                continue;
            }

            AlignRooms(part.transform, newRoom.transform, entry, newEntry);

            if (HandleIntersection(newPart))
            {
                Destroy(newRoom);
                entryComp.SetOccupied();

                Collider[] overlapping = Physics.OverlapSphere(entry.position, 0.2f);
                foreach (Collider col2 in overlapping)
                {
                    if (col2.CompareTag("Door"))
                        Destroy(col2.gameObject);
                }

                if (lockedDoor != null)
                    Instantiate(lockedDoor, entry.position, entry.rotation, part.transform);

                return false;
            }

            // Place door
            if (door != null)
            {
                Collider[] doorCheck = Physics.OverlapSphere(entry.position, 0.1f);
                bool doorAlreadyExists = false;
                foreach (Collider doorCol in doorCheck)
                {
                    if (doorCol.CompareTag("Door"))
                    {
                        doorAlreadyExists = true;
                        break;
                    }
                }

                if (!doorAlreadyExists)
                {
                    GameObject doorToAlign = Instantiate(door, entry.position, entry.rotation);
                    doorToAlign.transform.SetParent(null);
                }
            }

            part.UseEntrypoint(entry);
            newPart.UseEntrypoint(newEntry);
            generatedRooms.Add(newPart);
            return true;
        }

        return false;
    }

    public void TryFillingEntrypointsUntilStuck()
    {
        StartCoroutine(FillEntrypointsLoop());
    }

    private IEnumerator FillEntrypointsLoop()
    {
        // Recheck every 0.2 seconds until all entrypoints are filled
        bool madeProgress;

        do
        {
            madeProgress = false;

            if (RoomGenerationDistance == null)
            {
                Debug.LogError("RoomGenerationDistance is not assigned.");
                yield break;
            }

            CapsuleCollider capsule = RoomGenerationDistance.GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                Debug.LogError("RoomGenerationDistance does not have a CapsuleCollider.");
                yield break;
            }

            Vector3 point1 = RoomGenerationDistance.transform.position + capsule.center + Vector3.up * capsule.height / 2;
            Vector3 point2 = RoomGenerationDistance.transform.position + capsule.center - Vector3.up * capsule.height / 2;

            Collider[] hits = Physics.OverlapCapsule(point1, point2, capsule.radius, roomsLayermask);

            foreach (Collider col in hits)
            {
                if (col == null) continue;

                DungeonPart part = col.GetComponent<DungeonPart>();
                if (part == null || part.gameObject == null) continue;

                foreach (Transform entry in part.entrypoints)
                {
                    if (entry == null) continue;

                    if (!entry.TryGetComponent<EntryPoint>(out EntryPoint entryComp)) continue;
                    if (entryComp.IsOccupied()) continue;

                    if (TryGenerateRoomAt(entryComp, part, entry))
                    {
                        madeProgress = true;
                        yield return new WaitForSeconds(0.05f); // small delay to allow physics to sync
                    }
                }
            }

            yield return new WaitForSeconds(0.2f); // wait before checking again

        } while (madeProgress);

        Debug.Log("All reachable entrypoints filled.");
    }
}

