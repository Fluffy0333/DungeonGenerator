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
    private System.Random rand = new();
    private RectInt savedRoom;
    private RectInt dungeonBounds;
    [HideInInspector]
    public int removeAttemptAmount;
    [HideInInspector]
    public float percentageDeleted;
    private float initialRoomsAmount;
    private int sizeToRemove;
    private bool widthSplit;
    private int i = 0;
    private bool canCheck = false;
    private bool checkSplitDone = false;
    [HideInInspector]
    public GameObject roomParent;
    private bool waitForInput = false;
    private bool goSlow = false;
    [HideInInspector]
    public HashSet<Vector2> wallList = new();
    private HashSet<Vector2> doorList = new();
    private List<Vector3> discovered = new();
    [HideInInspector]
    public List<Vector3> toDoFloors = new();
    private DeleteRooms deleteRooms;
    private Recursion recursion;
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
        canCheck = false;
        marchingSquare.currentLocation = new(1, 1);
        marchingSquare.enabled = false;
        i = 0;
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
        while (marchingSquare.enabled == false)
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
                SplitRoom();
            }
            else if (initialRoomsAmount == 0)
            {
                initialRoomsAmount = doneRooms.Count;
                doneRooms.Sort((a, b) => (a.width + a.height).CompareTo(b.width + b.height));
            }
            else if (canCheck == false)
            {
                if (i < doneRooms.Count)
                {
                    selectedRoom = doneRooms[i];
                    for (int j = i + 1; j < doneRooms.Count; j++)
                    {
                        CheckSplits(i, j);
                    }
                    i++;
                }
                else if (i >= doneRooms.Count)
                {
                    canCheck = true;
                    i = 1;
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
                    CreateRoomStructure(room);
                }
                foreach (RectInt room in doneRooms)
                {
                    var tempRoom = new GameObject($"room{i}");
                    GameObject parentGameObject = Instantiate(tempRoom, transform.position, transform.rotation, roomParent.transform);
                    Destroy(tempRoom);
                    i++;
                    toDoFloors = new();
                    selectedRoom = room;
                    recursion.SpawnFloorsRecursive(discovered, parentGameObject, 1.5f, 1.5f);
                }
                AddFloorDoors(doorList, roomParent);
                marchingSquare.enabled = true;
            }
        }
    }

    private void CreateRoomStructure(RectInt room)
    {
        AddWalls(room);
        // AddFloors(room, parentGameObject);
    }

    private void WallDoorList(RectInt door)
    {
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
    //reduces width or height from a room till it can't anymore without going under the minimum requirements. width/height is predictably randomly chosen.
    private void SplitRoom()
    {
        int selectedRoomInt = rand.Next(0, toDoRooms.Count);
        selectedRoom = toDoRooms[selectedRoomInt];
        if (selectedRoom.width > minSize * 2 && selectedRoom.height > minSize * 2)
        {
            widthSplit = rand.Next(0, 100) > 50;
            if (widthSplit)
            {
                ReduceWidth();
            }
            else
            {
                ReduceHeight();
            }
        }
        else if (selectedRoom.width > minSize * 2)
        {
            ReduceWidth();
        }
        else if (selectedRoom.height > minSize * 2)
        {
            ReduceHeight();
        }
        else
        {
            //doesn't trigger anymore but is gonna remain for failsafe.
            Debug.LogWarning("Done room found in not done list!");
            AddDoneRoom(selectedRoom);
            return;
        }
        toDoRooms[selectedRoomInt] = selectedRoom;
        if (selectedRoom.height <= minSize * 2 && selectedRoom.width <= minSize * 2)
        {
            AddDoneRoom(selectedRoom);
        }
    }
    private void AddDoneRoom(RectInt roomSelected)
    {
        doneRooms.Add(roomSelected);
        toDoRooms.Remove(roomSelected);
    }
    private void ReduceWidth()
    {
        sizeToRemove = rand.Next(minSize, selectedRoom.width - minSize);
        savedRoom = new(selectedRoom.width - sizeToRemove + selectedRoom.xMin, selectedRoom.yMin, sizeToRemove, selectedRoom.height);
        if (savedRoom.width > minSize * 2 || savedRoom.height > minSize * 2)
        {
            toDoRooms.Add(savedRoom);
        }
        else
        {
            AddDoneRoom(savedRoom);
        }
        selectedRoom.width = selectedRoom.width - sizeToRemove + 1;
    }
    private void ReduceHeight()
    {
        sizeToRemove = rand.Next(minSize, selectedRoom.height - minSize);
        savedRoom = new(selectedRoom.xMin, selectedRoom.height - sizeToRemove + selectedRoom.yMin, selectedRoom.width, sizeToRemove);
        if (savedRoom.width > minSize * 2 || savedRoom.height > minSize * 2)
        {
            toDoRooms.Add(savedRoom);
        }
        else
        {
            AddDoneRoom(savedRoom);
        }
        selectedRoom.height = selectedRoom.height - sizeToRemove + 1;
    }
    //check if a room connects to another room. O(n²)
    private void CheckSplits(int i, int j)
    {
        if (AlgorithmsUtils.Intersects(doneRooms[i], doneRooms[j]))
        {
            savedRoom = AlgorithmsUtils.Intersect(doneRooms[i], doneRooms[j]);
            savedRoom = Doors.MakeDoor(savedRoom);
            if (savedRoom != RectInt.zero)
            {
                doorsList.Add(savedRoom);
                connections.Add(new Connections(savedRoom, doneRooms[i], doneRooms[j]));
            }
        }
    }
    private void AddWalls(RectInt room)
    {
        for (int width = 0; width < room.width; width++)
        {
            if (!wallList.Contains(new(room.x + width + 0.5f, room.y + 0.5f)))
            {
                wallList.Add(new(room.x + width + 0.5f, room.y + 0.5f));
            }
            if (!wallList.Contains(new(room.x + width + 0.5f, room.y + room.height - 0.5f)))
            {
                wallList.Add(new(room.x + width + 0.5f, room.y + room.height - 0.5f));
            }
        }
        for (int height = 0; height < room.height; height++)
        {
            if (!wallList.Contains(new(room.x + 0.5f, room.y + height + 0.5f)))
            {
                wallList.Add(new(room.x + 0.5f, room.y + height + 0.5f));
            }
            if (!wallList.Contains(new(room.x + room.width - 0.5f, room.y + height + 0.5f)))
            {
                wallList.Add(new(room.x + room.width - 0.5f, room.y + height + 0.5f));
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


