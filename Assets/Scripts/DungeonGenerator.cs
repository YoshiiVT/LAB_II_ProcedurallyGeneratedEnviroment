using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : Singleton<DungeonGenerator>
{
    [SerializeField]
    private GameObject enterance;

    [SerializeField]
    private List<GameObject> rooms;

    [SerializeField]
    private List<GameObject> deadEnd;

    [SerializeField]
    private List<GameObject> alternateEntrance;

    [SerializeField]
    private List<GameObject> hallways;

    [SerializeField]
    private GameObject door;

    [SerializeField]
    private GameObject lockedDoor;

    public int noOfRooms = 10;

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
        Generate();
    }

    public void Generate()
    {
        for (int i = 0; i < noOfRooms - alternateEntrance.Count; i++)
        {
            if (generatedRooms.Count < 1)
            {
                if (enterance != null)
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
                }
                else
                {
                    Debug.LogError("Entrance object is not assigned in the DungeonGenerator.");
                }
            }
            else
            {
                bool shouldPlaceHallway = Random.Range(0f, 10f) > 9f;
                DungeonPart randomGeneratedRoom = null;
                Transform room1Entrypoint = null;
                int totalRetries = 1000;
                int retryIndex = 0;

                Debug.Log($"[GEN] Generated rooms count: {generatedRooms.Count}");

                while (randomGeneratedRoom == null && retryIndex < totalRetries)
                {
                    int randomLinkRoomIndex = Random.Range(0, generatedRooms.Count);
                    DungeonPart roomToTest = generatedRooms[randomLinkRoomIndex];

                    if (roomToTest.HasAvailableEntrypoint(out room1Entrypoint))
                    {
                        randomGeneratedRoom = roomToTest;
                        break;
                    }
                    else
                    {
                        Debug.Log($"[GEN] Retry {retryIndex + 1}: {roomToTest.name} has NO available entrypoint.");
                    }

                    retryIndex++;
                }

                if (randomGeneratedRoom == null)
                {
                    Debug.LogError("Could not find a valid room for hallway placement after retries.");
                    break;
                }

                if (door != null)
                {
                    GameObject doorToAlign = Instantiate(door, transform.position, transform.rotation);

                    if (shouldPlaceHallway)
                    {
                        if (hallways.Count > 0)
                        {
                            int randomIndex = Random.Range(0, hallways.Count);
                            GameObject generatedHallway = Instantiate(hallways[randomIndex], transform.position, transform.rotation);
                            generatedHallway.transform.SetParent(null);

                            if (generatedHallway.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                            {
                                if (dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
                                {
                                    AlignRooms(randomGeneratedRoom.transform, generatedHallway.transform, room1Entrypoint, room2Entrypoint);
                                    doorToAlign.transform.position = room1Entrypoint.position;
                                    doorToAlign.transform.rotation = room1Entrypoint.rotation;

                                    if (!HandleIntersection(dungeonPart))
                                    {
                                        // Only mark as used *after* successful placement
                                        dungeonPart.UseEntrypoint(room2Entrypoint);
                                        randomGeneratedRoom.UseEntrypoint(room1Entrypoint);
                                        generatedRooms.Add(dungeonPart);
                                    }
                                    else
                                    {
                                        Debug.LogWarning("Intersection detected, retrying hallway placement...");
                                        RetryPlacementLoop(generatedHallway, doorToAlign);
                                        continue;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Generated hallway has no valid entry point.");
                                }
                            }
                            else
                            {
                                Debug.LogError("Generated hallway does not have DungeonPart component.");
                            }
                        }
                        else
                        {
                            Debug.LogError("Hallways list is empty.");
                        }
                    }
                    else
                    {
                        if (deadEnd.Count > 0 || rooms.Count > 0)
                        {
                            GameObject generatedRoom;

                            if (deadEnd.Count > 0)
                            {
                                bool shouldPlaceDeadRoom = Random.Range(0f, 1f) > 0.5f;
                                bool canPlaceDeadRoom = false;

                                if (shouldPlaceDeadRoom)
                                {
                                    foreach (DungeonPart otherRoom in generatedRooms)
                                    {
                                        if (otherRoom != randomGeneratedRoom && otherRoom.HasAvailableEntrypoint(out _))
                                        {
                                            canPlaceDeadRoom = true;
                                            break;
                                        }
                                    }
                                }

                                if (shouldPlaceDeadRoom && canPlaceDeadRoom)
                                {
                                    int randomIndex = Random.Range(0, deadEnd.Count);
                                    generatedRoom = Instantiate(deadEnd[randomIndex], transform.position, transform.rotation);
                                }
                                else
                                {
                                    int randomIndex = Random.Range(0, rooms.Count);
                                    generatedRoom = Instantiate(rooms[randomIndex], transform.position, transform.rotation);
                                }
                            }
                            else
                            {
                                int randomIndex = Random.Range(0, rooms.Count);
                                generatedRoom = Instantiate(rooms[randomIndex], transform.position, transform.rotation);
                            }

                            generatedRoom.transform.SetParent(null);

                            if (generatedRoom.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                            {
                                if (dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
                                {
                                    AlignRooms(randomGeneratedRoom.transform, generatedRoom.transform, room1Entrypoint, room2Entrypoint);
                                    doorToAlign.transform.position = room1Entrypoint.position;
                                    doorToAlign.transform.rotation = room1Entrypoint.rotation;

                                    if (!HandleIntersection(dungeonPart))
                                    {
                                        dungeonPart.UseEntrypoint(room2Entrypoint);
                                        randomGeneratedRoom.UseEntrypoint(room1Entrypoint);
                                        generatedRooms.Add(dungeonPart);
                                    }
                                    else
                                    {
                                        Debug.LogWarning("Intersection detected, retrying room placement...");
                                        RetryPlacementLoop(generatedRoom, doorToAlign);
                                        continue;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Generated room has no valid entry point.");
                                }
                            }
                            else
                            {
                                Debug.LogError("Generated room does not have DungeonPart component.");
                            }
                        }
                        else
                        {
                            Debug.LogError("SpecialRooms or Rooms list is empty.");
                        }
                    }
                }
                else
                {
                    Debug.LogError("Door object is not assigned in the DungeonGenerator.");
                }
            }
        }

        // Optional: Fill doors for unused entrypoints
        // FillEmptyEntrances();
    }

    private void RetryPlacementLoop(GameObject itemToPlace, GameObject doorToPlace)
    {
        int maxRetries = 100;
        int retries = 0;
        bool placedSuccessfully = false;

        while (!placedSuccessfully && retries < maxRetries)
        {
            DungeonPart randomGeneratedRoom = null;
            Transform room1Entrypoint = null;

            int totalRetries = 100;
            int retryIndex = 0;

            while (randomGeneratedRoom == null && retryIndex < totalRetries)
            {
                int randomLinkRoomIndex = Random.Range(0, generatedRooms.Count);
                DungeonPart roomToTest = generatedRooms[randomLinkRoomIndex];
                if (roomToTest.HasAvailableEntrypoint(out room1Entrypoint))
                {
                    randomGeneratedRoom = roomToTest;
                    break;
                }
                retryIndex++;
            }

            if (itemToPlace.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
            {
                if (dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
                {
                    AlignRooms(randomGeneratedRoom.transform, itemToPlace.transform, room1Entrypoint, room2Entrypoint);
                    doorToPlace.transform.position = room1Entrypoint.position;
                    doorToPlace.transform.rotation = room1Entrypoint.rotation;

                    if (!HandleIntersection(dungeonPart))
                    {
                        dungeonPart.UseEntrypoint(room2Entrypoint);
                        randomGeneratedRoom.UseEntrypoint(room1Entrypoint);
                        Debug.Log($"[GEN] Unused entrypoints on retry: {randomGeneratedRoom.name} and {dungeonPart.name}");
                        generatedRooms.Add(dungeonPart);
                        placedSuccessfully = true;
                    }
                    else
                    {
                        dungeonPart.UnuseEntrypoint(room2Entrypoint);
                        randomGeneratedRoom.UnuseEntrypoint(room1Entrypoint);
                        retries++;
                    }
                }
            }
        }

        if (!placedSuccessfully)
        {
            Debug.LogWarning("Failed to place room after max retries.");
            Destroy(itemToPlace);
            Destroy(doorToPlace);
        }
    }

    private void FillEmptyEntrances()
    {
        generatedRooms.ForEach(room => room.FillEmptyDoors());
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

                GameObject roomPrefab = null;

                // Count unoccupied entrypoints within RenderDistance
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
                    else
                    {
                        Debug.LogError("RenderDistance does not have a CapsuleCollider.");
                    }
                }

                // Decide what type of room to place
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
                }

                if (roomPrefab == null)
                {
                    Debug.LogError("Room not chosen.");
                    continue;
                }

                GameObject newRoom = Instantiate(roomPrefab, transform.position, transform.rotation);
                newRoom.transform.SetParent(null);

                if (!newRoom.TryGetComponent<DungeonPart>(out DungeonPart newPart))
                {
                    Debug.LogError("Generated room has no DungeonPart component.");
                    Destroy(newRoom);
                    continue;
                }

                if (!newPart.HasAvailableEntrypoint(out Transform newEntry))
                {
                    Debug.LogError("Generated room has no available entrypoint.");
                    Destroy(newRoom);
                    continue;
                }

                AlignRooms(part.transform, newRoom.transform, entry, newEntry);

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
                    else
                    {
                        Debug.Log($"Skipped door placement at {entry.position} – one already exists.");
                    }
                }
                else
                {
                    Debug.LogWarning("Door prefab not assigned in DungeonGenerator.");
                }

                if (!HandleIntersection(newPart))
                {
                    part.UseEntrypoint(entry);
                    newPart.UseEntrypoint(newEntry);
                    generatedRooms.Add(newPart);
                }
                else
                {
                    Debug.Log("Intersection detected, destroying room.");
                    Destroy(newRoom);

                    // Mark original entrypoint as occupied
                    entryComp.SetOccupied();

                    // Destroy any existing door at this entrypoint
                    Collider[] overlapping = Physics.OverlapSphere(entry.position, 0.2f);
                    foreach (Collider col2 in overlapping)
                    {
                        if (col2.CompareTag("Door"))
                        {
                            Destroy(col2.gameObject);
                        }
                    }

                    // Place locked door at the original entrypoint
                    if (lockedDoor != null)
                    {
                        Instantiate(lockedDoor, entry.position, entry.rotation, part.transform);
                    }
                }
            }
        }
    }

}

