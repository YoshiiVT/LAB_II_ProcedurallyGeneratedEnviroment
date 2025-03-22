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

    private List<DungeonPart> generatedRooms;

    private bool isGenerated = false;

    /*
    private void Awake()
    {
        Instance = this;
    }
    */
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
        //? Generates Dungeon
        for (int i = 0; i < noOfRooms - alternateEntrance.Count; i++)
        {
            if (generatedRooms.Count < 1)
            {
                GameObject generatedRoom = Instantiate(enterance, transform.position, transform.rotation);

                generatedRoom.transform.SetParent(null);

                if (generatedRoom.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                {
                    generatedRooms.Add(dungeonPart);
                }
            }
            else
            {
                bool shouldPlaceHallway = Random.Range(0f, 1f) > 0.5f;
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

                GameObject doorToAlign = Instantiate(door, transform.position, transform.rotation);

                if (shouldPlaceHallway)
                {
                    int randomIndex = Random.Range(0, hallways.Count);
                    GameObject generatedHallway = Instantiate(hallways[randomIndex], transform.position, transform.rotation);
                    generatedHallway.transform.SetParent(null);
                    if (generatedHallway.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                    {
                        if (dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
                        {
                            generatedRooms.Add(dungeonPart);
                            doorToAlign.transform.position = room1Entrypoint.transform.position;
                            doorToAlign.transform.rotation = room1Entrypoint.transform.rotation;
                            AlignRooms(randomGeneratedRoom.transform, generatedHallway.transform, room1Entrypoint, room2Entrypoint);

                            if (HandleIntersection(dungeonPart))
                            {
                                dungeonPart.UnuseEntrypoint(room2Entrypoint);
                                randomGeneratedRoom.UnuseEntrypoint(room1Entrypoint);
                                RetryPlacementLoop(generatedHallway, doorToAlign);
                                continue;
                            }
                        }  
                    }
                }
                else
                {
                    GameObject generatedRoom;

                    if (specialRooms.Count > 0)
                    {
                        bool shouldPlaceSpecialRoom = Random.Range(0f, 1f) > 0.9f;

                        if (shouldPlaceSpecialRoom)
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
                        if(dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
                        {
                            generatedRooms.Add(dungeonPart);
                            doorToAlign.transform.position = room1Entrypoint.transform.position;
                            doorToAlign.transform.rotation = room1Entrypoint.transform.rotation;
                            AlignRooms(randomGeneratedRoom.transform, generatedRoom.transform, room1Entrypoint, room2Entrypoint);

                            if (HandleIntersection(dungeonPart))
                            {
                                dungeonPart.UnuseEntrypoint(room2Entrypoint);
                                randomGeneratedRoom.UnuseEntrypoint(room1Entrypoint);
                                RetryPlacementLoop(generatedRoom, doorToAlign);
                                continue;
                            }
                        }
                    }
                }
                

            }
        }
    }

    private void GenerateAlternateEntrances()
    {
        if (alternateEntrance.Count < 1) return;

        for (int i = 0; i < alternateEntrance.Count; i++)
        {
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
                
                int randomIndex = Random.Range(0, alternateEntrance.Count);
                GameObject generatedRoom = Instantiate(alternateEntrance[randomIndex], transform.position,transform.rotation);

                generatedRoom.transform.SetParent(null);
            }
        }
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
                    doorToPlace.transform.position = room1Entrypoint.transform.position;
                    doorToPlace.transform.rotation = room1Entrypoint.transform.rotation;

                    if (HandleIntersection(dungeonPart))
                    {
                        dungeonPart.UnuseEntrypoint(room2Entrypoint);
                        randomGeneratedRoom.UnuseEntrypoint(room1Entrypoint);
                        retries++;
                    }
                    else
                    {
                        placedSuccessfully = true;
                        generatedRooms.Add(dungeonPart);
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
            if (hit == dungeonPart.collider) continue;

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
        // Rotate room2 so that its entry aligns with room1
        Quaternion rotationToMatch = Quaternion.FromToRotation(room2Entry.forward, -room1Entry.forward);
        room2.rotation = rotationToMatch * room2.rotation;

        // Move room2 so that the entrypoints coincide
        Vector3 offset = room1Entry.position - room2Entry.position;
        room2.position += offset;

        Physics.SyncTransforms();
    }

    public List<DungeonPart> GetGeneratedRooms() => generatedRooms;

    public bool IsGenerated() => isGenerated;
}
