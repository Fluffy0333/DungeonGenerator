using System.Collections;
using System.Collections.Generic;
using System;
using NaughtyAttributes;
using UnityEngine;
[RequireComponent(typeof(DeleteRooms))]
[RequireComponent(typeof(MarchingSquare))]
[RequireComponent(typeof(Recursion))]
[RequireComponent(typeof(SplittingRooms))]
[RequireComponent(typeof(RoomsStructure))]
public class DungeonGenerator : MonoBehaviour
{
    //lists here
    public List<RectInt> toDoRooms;
    public List<RectInt> doneRooms;
    public List<RectInt> doorsList;
    public List<RectInt> checkedRooms;
    public List<Connections> connections = new();

    //public variables here
    public RectInt selectedRoom = new(0, 0, 100, 150);
    public GameObject floor;
    public int minSize = 7;
    public enum SpawnType { automatic, manual, slow };
    public SpawnType spawnType;
    public float slowModeSpeed = 0.1f;
    [Range(0, 75)]
    public float percentageToDelete;
    public int removeAttempts = 5;
    public int seed;

    //public hideininspector here
    [HideInInspector]
    public System.Random rand = new();
    [HideInInspector]
    public int removeAttemptAmount;
    [HideInInspector]
    public float percentageDeleted;
    [HideInInspector]
    public GameObject roomParent;

    //private variables here
    private RectInt dungeonBoundaries;
    private float initialRoomsAmount;
    private int roomNumber = 0;
    private bool madeRoomStructure = false;
    private bool waitForInput = false;
    private bool goSlow = false;
    private DeleteRooms deleteRooms;
    private Recursion recursion;
    private SplittingRooms splittingRooms;
    private MarchingSquare marchingSquare;
    private RoomsStructure roomsStructure;

    //buttons here
    [Button("restart Generation")]
    private void RestartRoom()
    {
        //reset everything back to the default
        selectedRoom = dungeonBoundaries;
        marchingSquare.dungeonBoundaries = dungeonBoundaries;
        ClearLists();
        Destroy(roomParent);
        roomParent = new("rooms");
        toDoRooms.Add(selectedRoom);
        ClearVariables();
        StartCoroutine(BeginCutting());
        rand = new System.Random(seed);
    }

    private void ClearLists()
    {
        doneRooms.Clear();
        toDoRooms.Clear();
        doorsList.Clear();
        roomsStructure.wallList.Clear();
        roomsStructure.doorList.Clear();
        connections.Clear();
    }
    private void ClearVariables()
    {
        removeAttemptAmount = 0;
        percentageDeleted = 0;
        initialRoomsAmount = 0;
        madeRoomStructure = false;
        marchingSquare.currentLocation = new(1, 1);
        marchingSquare.enabled = false;
        roomNumber = 0;
    }

    void Start()
    {
        roomParent = new("rooms");
        //selectedroom in this case is the very first room that is made which will then split down
        dungeonBoundaries = selectedRoom;
        deleteRooms = GetComponent<DeleteRooms>();
        recursion = GetComponent<Recursion>();
        marchingSquare = GetComponent<MarchingSquare>();
        splittingRooms = GetComponent<SplittingRooms>();
        roomsStructure = GetComponent<RoomsStructure>();
        marchingSquare.dungeonBoundaries = dungeonBoundaries;
        rand = new System.Random(seed);
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
            //waitforinput = wait for spacebar and goslow means it will wait everytime with a variable that can be set in the inspecter equivalent to slowModeSpeed.
            case SpawnType.automatic:
                waitForInput = false;
                goSlow = false;
                if (madeRoomStructure)
                {
                    marchingSquare.delay = 0;
                    marchingSquare.waitForInput = false;
                }
                break;
            case SpawnType.manual:
                waitForInput = true;
                goSlow = false;
                if (madeRoomStructure)
                {
                    marchingSquare.delay = 0;
                    marchingSquare.waitForInput = true;
                }
                break;
            case SpawnType.slow:
                waitForInput = false;
                goSlow = true;
                if (madeRoomStructure)
                {
                    marchingSquare.delay = slowModeSpeed;
                    marchingSquare.waitForInput = false;
                }
                break;
        }
    }
    IEnumerator BeginCutting()
    {
        //if it does not wait for 0.1f seconds some things haven't loaded yet and gives a null exception error
        yield return new WaitForSeconds(0.1f);

        while (!marchingSquare.enabled)
        {
            //slow mode / manual mode checkers
            if (goSlow)
            {
                yield return new WaitForSeconds(slowModeSpeed);
            }
            else if (waitForInput)
            {
                yield return new WaitUntil(() => Input.GetKey(KeyCode.Space));
            }

            if (toDoRooms.Count > 0 && !madeRoomStructure)
            {
                splittingRooms.SplitRoom();
            }
            else if (initialRoomsAmount == 0)
            {
                //how many rooms are there in total when all rooms have been split?
                initialRoomsAmount = doneRooms.Count;
                //sort rooms based on smallest to biggest
                doneRooms.Sort((a, b) => (a.width + a.height).CompareTo(b.width + b.height));
            }
            else if (roomNumber < doneRooms.Count)
            {
                //make the connections from door to room a and room b this is not a for loop so that it can be slowed with slow and manual
                selectedRoom = doneRooms[roomNumber];
                //check with selected room each room and which of them are adjacent, this is in a for loop as it would otherwise seem like it's not doing anything in slow / manual mode
                for (int secondRoomNumber = roomNumber + 1; secondRoomNumber < doneRooms.Count; secondRoomNumber++)
                {
                    RectInt secondRoom = doneRooms[secondRoomNumber];
                    //if the two rooms intersect, try to place a door in the middle
                    if (AlgorithmsUtils.Intersects(selectedRoom, secondRoom))
                    {
                        CheckIntersection(selectedRoom, secondRoom);
                    }
                }
                roomNumber++;
            }
            else if (!madeRoomStructure)
            {
                //when it's done with deleting all the rooms it will tell it that the roomstructure is made, it will then 
                madeRoomStructure = deleteRooms.CheckDeleteRoom(connections, doneRooms, rand, percentageDeleted, initialRoomsAmount, removeAttempts, percentageToDelete, doorsList);
            }
            else if (madeRoomStructure)
            {
                CreateFlooring();
            }
        }
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
        //the room that is currently busy / last used room
        AlgorithmsUtils.DebugRectInt(selectedRoom, Color.blue);
    }
    //check if a room connects to another room. O(n²)
    private void CheckIntersection(RectInt firstRoom, RectInt secondRoom)
    {
        RectInt doorPlacement = AlgorithmsUtils.Intersect(firstRoom, secondRoom);
        doorPlacement = Doors.MakeDoor(doorPlacement);
        //if the door was made, make a connection with the door, firstroom and secondroom
        if (doorPlacement != RectInt.zero)
        {
            doorsList.Add(doorPlacement);
            connections.Add(new Connections(doorPlacement, firstRoom, secondRoom));
        }
    }
    private void CreateFlooring()
    {
        foreach (RectInt room in doneRooms)
        {
            //take the boundries of each room and add them to the wall list.
            roomsStructure.AddWalls(room);
        }
        int currentRoomNumber = 1;
        foreach (RectInt room in doneRooms)
        {
            //all floors belong to a different room and this will tell us that, 
            currentRoomNumber = LayFloors(currentRoomNumber, room);
        }
        //finally add the floor to doors and remove them from the wall list and begin marching!
        roomsStructure.AddFloorToDoors(roomsStructure.doorList, roomParent);
        marchingSquare.enabled = true;
    }

    private int LayFloors(int currentRoomNumber, RectInt room)
    {
        var tempRoom = new GameObject($"room{currentRoomNumber}");
        GameObject parentGameObject = Instantiate(tempRoom, transform.position, transform.rotation, roomParent.transform);
        Destroy(tempRoom);
        currentRoomNumber++;
        selectedRoom = room;
        List<Vector2> discovered = new();
        recursion.SpawnFloorsRecursive(discovered, parentGameObject, 1.5f, 1.5f);
        return currentRoomNumber;
    }
}