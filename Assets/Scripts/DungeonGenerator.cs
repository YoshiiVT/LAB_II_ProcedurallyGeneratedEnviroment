using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : Singleton<DungeonGenerator>
{
    [SerializeField]
    private GameObject enterance;

    [SerializeField]
    private List<GameObject> rooms;

    [SerializeField]
    private List<GameObject> specialRooms;

    [SerializeField]
    private List<GameObject> alternateEntrance;

    [SerializeField]
    private List<GameObject> hallways;

    [SerializeField]
    private GameObject door;

    public int noOfRooms = 10;

    [SerializeField]
    private LayerMask roomsLayermask;

    [SerializeField]
    private GameObject renderDistance;

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
                int totalRetries = 300;
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
                    else
                    {
                        Debug.Log($"Retry #{retryIndex + 1}: Room {roomToTest.name} has no available entry point.");
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
                        if (specialRooms.Count > 0 || rooms.Count > 0)
                        {
                            GameObject generatedRoom;

                            if (specialRooms.Count > 0)
                            {
                                bool shouldPlaceSpecialRoom = Random.Range(0f, 1f) > 0.5f;
                                bool canPlaceSpecialRoom = false;

                                if (shouldPlaceSpecialRoom)
                                {
                                    foreach (DungeonPart otherRoom in generatedRooms)
                                    {
                                        if (otherRoom != randomGeneratedRoom && otherRoom.HasAvailableEntrypoint(out _))
                                        {
                                            canPlaceSpecialRoom = true;
                                            break;
                                        }
                                    }
                                }

                                if (shouldPlaceSpecialRoom && canPlaceSpecialRoom)
                                {
                                    int randomIndex = Random.Range(0, specialRooms.Count);
                                    generatedRoom = Instantiate(specialRooms[randomIndex], transform.position, transform.rotation);
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
        Collider[] hits = Physics.OverlapBox(dungeonPart.collider.bounds.center, dungeonPart.collider.bounds.size / 2, Quaternion.identity, roomsLayermask);

        foreach (Collider hit in hits)
        {
            if (hit != dungeonPart.collider)
            {
                didIntersect = true;
                break;
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
        if (renderDistance == null)
        {
            Debug.LogError("RenderDistance is not assigned.");
            return;
        }

        CapsuleCollider capsule = renderDistance.GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            Debug.LogError("RenderDistance does not have a CapsuleCollider.");
            return;
        }

        Vector3 point1 = renderDistance.transform.position + capsule.center + Vector3.up * capsule.height / 2;
        Vector3 point2 = renderDistance.transform.position + capsule.center - Vector3.up * capsule.height / 2;

        Collider[] hits = Physics.OverlapCapsule(point1, point2, capsule.radius, roomsLayermask);

        foreach (Collider col in hits)
        {
            DungeonPart part = col.GetComponent<DungeonPart>();
            if (part == null) continue;

            foreach (Transform entry in part.entrypoints)
            {
                if (!entry.TryGetComponent<EntryPoint>(out EntryPoint entryComp)) continue;
                if (entryComp.IsOccupied()) continue;

                GameObject roomPrefab = rooms.Count > 0 ? rooms[Random.Range(0, rooms.Count)] : null;
                if (roomPrefab == null)
                {
                    Debug.LogError("No room prefabs assigned.");
                    return;
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
                }
            }
        }
    }
}
