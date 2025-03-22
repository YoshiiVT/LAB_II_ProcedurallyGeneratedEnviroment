using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class OLDDungeonGenerator : Singleton<OLDDungeonGenerator>
{
    public static OLDDungeonGenerator Instance { get; private set; }

    [SerializeField] private GameObject enterance;
    [SerializeField] private List<GameObject> rooms;
    [SerializeField] private List<GameObject> specialRooms;
    [SerializeField] private List<GameObject> alternateEnterances;
    [SerializeField] private List<GameObject> hallways;
    [SerializeField] private GameObject door;
    [SerializeField] private int noOfRooms = 10;
    [SerializeField] private LayerMask roomsLayermask;
    private List<OLDDungeonPart> generatedRooms;
    private bool isGenerated = false;

    private void Awake() { Instance = this; }

    private void Start()
    {
        generatedRooms = new List<OLDDungeonPart>();
        Generate();
    }

    private void Generate()
    {
        for (int i = 0; i < noOfRooms - alternateEnterances.Count; i++)
        {
            if (generatedRooms.Count < 1)
            {
                GameObject generatedRoom = Instantiate(enterance, transform.position, transform.rotation);
                if (generatedRoom.TryGetComponent<OLDDungeonPart>(out OLDDungeonPart dungeonPart))
                    generatedRooms.Add(dungeonPart);
            }
            else
            {
                PlaceNextRoom();
            }
        }
    }

    public void PlaceNextRoom()
    {
        OLDDungeonPart randomGeneratedRoom = null;
        Transform room1Entrypoint = null;
        int retryIndex = 0;
        const int totalRetries = 10;

        while (randomGeneratedRoom == null && retryIndex < totalRetries)
        {
            int randomLinkRoomIndex = Random.Range(0, generatedRooms.Count);
            OLDDungeonPart roomToTest = generatedRooms[randomLinkRoomIndex];

            Debug.Log($" Checking room: {roomToTest.gameObject.name} for available entry points...");

            if (roomToTest.HasAvailableEntrypoint(out room1Entrypoint))
            {
                randomGeneratedRoom = roomToTest;
                break;
            }

            retryIndex++;
        }

        if (randomGeneratedRoom == null)
        {
            Debug.LogError(" Failed to find a valid entry point - No open entry points in existing rooms.");
            return;
        }

        Debug.Log($" Selected room: {randomGeneratedRoom.gameObject.name}, Entry Point: {room1Entrypoint.position}");

        GameObject newRoom = Instantiate(Random.Range(0f, 1f) > 0.5f ? GetRandomHallway() : GetRandomRoom(), transform.position, transform.rotation);

        if (newRoom.TryGetComponent<OLDDungeonPart>(out OLDDungeonPart dungeonPart) && dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
        {
            if (!WillOverlap(room2Entrypoint.position, newRoom))
            {
                AlignRooms(randomGeneratedRoom.transform, newRoom.transform, room1Entrypoint, room2Entrypoint);
                generatedRooms.Add(dungeonPart);
            }
            else
            {
                Debug.LogWarning($" Room placement failed due to overlap at {room2Entrypoint.position}. Retrying...");
                Destroy(newRoom);
                PlaceNextRoom();
            }
        }
    }

    private GameObject GetRandomRoom() => rooms[Random.Range(0, rooms.Count)];
    private GameObject GetRandomHallway() => hallways[Random.Range(0, hallways.Count)];

    private bool WillOverlap(Vector3 position, GameObject newRoom)
    {
        Collider[] hits = Physics.OverlapBox(position, newRoom.GetComponent<Collider>().bounds.extents, Quaternion.identity, roomsLayermask);
        return hits.Length > 0;
    }

    private void AlignRooms(Transform room1, Transform room2, Transform room1Entry, Transform room2Entry)
    {
        Quaternion rotationOffset = Quaternion.FromToRotation(room2Entry.forward, -room1Entry.forward);
        room2.rotation = rotationOffset * room2.rotation;

        Vector3 positionOffset = room1Entry.position - room2Entry.position;
        room2.position += positionOffset;

        Physics.SyncTransforms();
    }
}