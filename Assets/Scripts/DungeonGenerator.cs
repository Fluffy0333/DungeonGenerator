using System.Collections;
using System.Collections.Generic;
using System;
using NaughtyAttributes;
using UnityEngine;
[RequireComponent(typeof(DeleteRooms))]
[RequireComponent(typeof(MarchingSquare))]
[RequireComponent(typeof(Recursion))]
public class DungeonGenerator : MonoBehaviour
{
    public RectInt selectedRoom = new(0, 0, 100, 150);
    public List<RectInt> toDoRooms;
    public List<RectInt> doneRooms;
    public List<RectInt> doorsList;
    public List<RectInt> checkedRooms;
    public List<Connections> connections = new();
    public GameObject floor;
    public int minSize = 7;
    public enum SpawnType { automatic, manual, slow };
    public SpawnType spawnType;
    public float cutSpeed = 0.1f;
    [Range(0, 75)]
    public float percentageToDelete;
    public int removeAttempts = 5;
    public int seed;
    [HideInInspector]
    public System.Random rand = new();
    private RectInt savedRoom;
    private RectInt dungeonBounds;
    [HideInInspector]
    public int removeAttemptAmount;
    [HideInInspector]
    public float percentageDeleted;
    private float initialRoomsAmount;
    private int sizeToRemove;
    private int roomNumber = 0;
    private bool connectionsMade = false;
    private bool checkSplitDone = false;
    [HideInInspector]
    public GameObject roomParent;
    private bool waitForInput = false;
    private bool goSlow = false;
    [HideInInspector]
    public HashSet<Vector2> wallList = new();
    private HashSet<Vector2> doorList = new();
    private List<Vector2> discovered = new();
    [HideInInspector]
    public List<Vector3> toDoFloors = new();
    private DeleteRooms deleteRooms;
    private Recursion recursion;
    private SplittingRooms splittingRooms;
    private MarchingSquare marchingSquare;
    [Button("restart Generation")]
    private void RestartRoom()
    {
        selectedRoom = dungeonBounds;
        marchingSquare.dungeonBounds = dungeonBounds;
        ClearLists();
        Destroy(roomParent);
        roomParent = new("rooms");
        toDoRooms.Add(selectedRoom);
        removeAttemptAmount = 0;
        percentageDeleted = 0;
        initialRoomsAmount = 0;
        checkSplitDone = false;
        connectionsMade = false;
        marchingSquare.currentLocation = new(1, 1);
        marchingSquare.enabled = false;
        roomNumber = 0;
        StartCoroutine(BeginCutting());
        if (seed > 0)
        {
            rand = new System.Random(seed);
        }
    }

    private void ClearLists()
    {
        doneRooms.Clear();
        toDoRooms.Clear();
        doorsList.Clear();
        wallList.Clear();
        doorList.Clear();
        connections.Clear();
        discovered.Clear();
    }

    void Start()
    {
        roomParent = new("rooms");
        dungeonBounds = selectedRoom;
        deleteRooms = GetComponent<DeleteRooms>();
        recursion = GetComponent<Recursion>();
        marchingSquare = GetComponent<MarchingSquare>();
        splittingRooms = GetComponent<SplittingRooms>();
        marchingSquare.dungeonBounds = dungeonBounds;
        if (seed > 0)
        {
            rand = new System.Random(seed);
        }
        toDoRooms.Add(selectedRoom);
        CheckSpawnType();
        StartCoroutine(BeginCutting());
    }
    void Update()
    {
        DrawDebug();
        CheckSpawnType();
    }
    public void CheckSpawnType()
    {
        switch (spawnType)
        {
            case SpawnType.automatic:
                waitForInput = false;
                goSlow = false;
                if (checkSplitDone)
                {
                    marchingSquare.delay = 0;
                    marchingSquare.waitForInput = false;
                }
                break;
            case SpawnType.manual:
                waitForInput = true;
                goSlow = false;
                if (checkSplitDone)
                {
                    marchingSquare.delay = 0;
                    marchingSquare.waitForInput = true;
                }
                break;
            case SpawnType.slow:
                waitForInput = false;
                goSlow = true;
                if (checkSplitDone)
                {
                    marchingSquare.delay = cutSpeed;
                    marchingSquare.waitForInput = false;
                }
                break;
        }
    }
    IEnumerator BeginCutting()
    {
        yield return new WaitForSeconds(0.1f);

        while (!marchingSquare.enabled)
        {
            if (goSlow)
            {
                yield return new WaitForSeconds(cutSpeed);
            }
            else if (waitForInput)
            {
                yield return new WaitUntil(() => Input.GetKey(KeyCode.Space));
            }

            if (toDoRooms.Count > 0 && !checkSplitDone)
            {
                splittingRooms.SplitRoom();
            }
            else if (initialRoomsAmount == 0)
            {
                initialRoomsAmount = doneRooms.Count;
                //sort rooms based on smallest to biggest
                doneRooms.Sort((a, b) => (a.width + a.height).CompareTo(b.width + b.height));
            }
            else if (!connectionsMade)
            {
                //make the connections from door to room a and room b
                if (roomNumber < doneRooms.Count)
                {
                    selectedRoom = doneRooms[roomNumber];
                    for (int j = roomNumber + 1; j < doneRooms.Count; j++)
                    {
                        CheckSplits(roomNumber, j);
                    }
                    roomNumber++;
                }
                else if (roomNumber >= doneRooms.Count)
                {
                    connectionsMade = true;
                    roomNumber = 1;
                }
            }
            else if (!checkSplitDone)
            {
                checkSplitDone = deleteRooms.CheckDeleteRoom(connections, doneRooms, rand, percentageDeleted, initialRoomsAmount, removeAttempts, percentageToDelete, doorsList);
            }
            else if (checkSplitDone)
            {
                foreach (RectInt door in doorsList)
                {
                    WallDoorList(door);
                }
                foreach (RectInt room in doneRooms)
                {
                    AddWalls(room);
                }
                foreach (RectInt room in doneRooms)
                {
                    var tempRoom = new GameObject($"room{roomNumber}");
                    GameObject parentGameObject = Instantiate(tempRoom, transform.position, transform.rotation, roomParent.transform);
                    Destroy(tempRoom);
                    roomNumber++;
                    toDoFloors = new();
                    selectedRoom = room;
                    recursion.SpawnFloorsRecursive(discovered, parentGameObject, 1.5f, 1.5f);
                }
                AddFloorDoors(doorList, roomParent);
                marchingSquare.enabled = true;
            }
        }
    }

    private void WallDoorList(RectInt door)
    {
        //door.x/door.y is the bottom left/right of the door, adding 0.5f to x and y is (1,1) then adding the half of the height/width + 0.5f is either (2,1) or (1, 2) depending if the door is in height or width length.
        doorList.Add(new(door.x + 0.5f, door.y + 0.5f));
        doorList.Add(new(door.x + door.width / 2 + 0.5f, door.y + door.height / 2 + 0.5f));
        wallList.Add(new(door.x + 0.5f, door.y + 0.5f));
        wallList.Add(new(door.x + door.width / 2 + 0.5f, door.y + door.height / 2 + 0.5f));
    }

    //shows all rooms/doors/connections/currentroom with debug.
    private void DrawDebug()
    {
        foreach (RectInt room in toDoRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.red);
        }
        foreach (RectInt room in doneRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green);
        }
        foreach (RectInt doorway in doorsList)
        {
            AlgorithmsUtils.DebugRectInt(doorway, Color.magenta);
        }
        foreach (Connections connection in connections)
        {
            Debug.DrawLine(new(connection.door.x + connection.door.width / 2f, 0, connection.door.y + connection.door.height / 2f), new(connection.roomOne.x + connection.roomOne.width / 2f, 0, connection.roomOne.y + connection.roomOne.height / 2f), Color.red);
            Debug.DrawLine(new(connection.door.x + connection.door.width / 2f, 0, connection.door.y + connection.door.height / 2f), new(connection.roomTwo.x + connection.roomTwo.width / 2f, 0, connection.roomTwo.y + connection.roomTwo.height / 2f), Color.red);
        }
        AlgorithmsUtils.DebugRectInt(selectedRoom, Color.blue);
    }
    //check if a room connects to another room. O(n²)
    private void CheckSplits(int firstRoomNumber, int secondRoomNumber)
    {
        RectInt firstRoom = doneRooms[firstRoomNumber];
        RectInt secondRoom = doneRooms[secondRoomNumber];
        if (AlgorithmsUtils.Intersects(firstRoom, secondRoom))
        {
            //if the two rooms intersect, try to place a door in the middle
            RectInt doorPlacement = AlgorithmsUtils.Intersect(firstRoom, secondRoom);
            doorPlacement = Doors.MakeDoor(doorPlacement);
            //if the door was made, make a connection with the door, firstroom and secondroom
            if (doorPlacement != RectInt.zero)
            {
                doorsList.Add(doorPlacement);
                connections.Add(new Connections(doorPlacement, firstRoom, secondRoom));
            }
        }
    }
    private void AddWalls(RectInt room)
    {
        for (int width = 0; width < room.width; width++)
        {
            float widthLocation = room.x + width + 0.5f;
            float heightLocation = room.y + room.height - 0.5f;
            if (!wallList.Contains(new(widthLocation, room.y + 0.5f)))
            {
                wallList.Add(new(widthLocation, room.y + 0.5f));
            }
            if (!wallList.Contains(new(widthLocation, heightLocation)))
            {
                wallList.Add(new(widthLocation, heightLocation));
            }
        }
        for (int height = 0; height < room.height; height++)
        {
            float widthLocation = room.x + room.width - 0.5f;
            float heightLocation = room.y + height + 0.5f;
            if (!wallList.Contains(new(room.x + 0.5f, heightLocation)))
            {
                wallList.Add(new(room.x + 0.5f, heightLocation));
            }
            if (!wallList.Contains(new(widthLocation, heightLocation)))
            {
                wallList.Add(new(widthLocation, heightLocation));
            }
        }
    }
    private void AddFloorDoors(HashSet<Vector2> doors, GameObject parentGameObject)
    {
        foreach (Vector2 door in doorList)
        {
            Instantiate(floor, new(door.x, 0, door.y), new(1, 0, 0, 1), parentGameObject.transform);
            wallList.Remove(door);
        }
    }
}