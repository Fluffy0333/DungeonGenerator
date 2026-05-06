using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using NaughtyAttributes;
using UnityEngine;
using Unity.AI.Navigation;
using System.IO;

[RequireComponent(typeof(DeleteRooms))]
[RequireComponent(typeof(MarchingSquare))]
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
    private float percentageDeleted;
    private float initialRoomsAmount;
    private int sizeToRemove;
    private bool widthSplit;
    private int i = 0;
    private bool canCheck = false;
    private bool checkSplitDone = false;
    private bool roomsDeleted = false;
    private GameObject roomParent;
    private bool waitForInput = false;
    private bool goSlow = false;
    [HideInInspector]
    public HashSet<Vector2> wallList = new();
    private HashSet<Vector2> doorList = new();
    public DeleteRooms deleteRooms;
    MarchingSquare marchingSquare;
    [Button("restart Generation")]
    private void RestartRoom()
    {
        selectedRoom = dungeonBounds;
        marchingSquare.dungeonBounds = dungeonBounds;
        doneRooms.Clear();
        toDoRooms.Clear();
        doorsList.Clear();
        connections.Clear();
        toDoRooms.Add(selectedRoom);
        removeAttemptAmount = 0;
        percentageDeleted = 0;
        initialRoomsAmount = 0;
        checkSplitDone = false;
        roomsDeleted = false;
        canCheck = false;
        marchingSquare.enabled = false;
        StartCoroutine(BeginCutting());
        if (seed > 0)
        {
            rand = new System.Random(seed);
        }
    }
    void Start()
    {
        roomParent = new("rooms");
        dungeonBounds = selectedRoom;
        deleteRooms = GetComponent<DeleteRooms>();
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
                if (checkSplitDone == true)
                {
                    marchingSquare.delay = 0;
                    marchingSquare.waitForInput = false;
                }
                break;
            case SpawnType.manual:
                waitForInput = true;
                goSlow = false;
                if (checkSplitDone == true)
                {
                    marchingSquare.delay = 0;
                    marchingSquare.waitForInput = true;
                }
                break;
            case SpawnType.slow:
                goSlow = true;
                waitForInput = false;
                if (checkSplitDone == true)
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
                Debug.Log("waiting for input");
                yield return new WaitUntil(() => Input.GetKey(KeyCode.Space));
            }
            if (toDoRooms.Count > 0 && checkSplitDone == false)
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
                    Debug.Log("done checking connections");
                    canCheck = true;
                }
            }
            else if (checkSplitDone == false)
            {
                if (!roomsDeleted)
                {
                    roomsDeleted = deleteRooms.CheckDeleteRoom(connections, doneRooms, rand, percentageDeleted, initialRoomsAmount, removeAttempts, percentageToDelete, doorsList);
                    checkSplitDone = roomsDeleted;
                }
            }
            else if (checkSplitDone == true)
            {
                foreach (RectInt door in doorsList)
                {
                    WallDoorList(door);
                }
                foreach (RectInt room in doneRooms)
                {
                    CreateRoomStructure(room);
                }
                AddFloorDoors(doorList, new("Doors"));
                marchingSquare.enabled = true;
            }
        }
    }

    private void CreateRoomStructure(RectInt room)
    {
        GameObject parentGameObject = new($"room{i}");
        Instantiate(parentGameObject, transform.position, transform.rotation, roomParent.transform);
        i++;
        AddWalls(room);
        AddFloors(room, parentGameObject);
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
    //checks every room to see if every room are connected to each other if we delete firstRoom, this is used to then delete the firstRoom if all still connect.
    public bool CheckRooms(RectInt roomToCheck, bool firstRoom)
    {
        if (firstRoom)
        {
            checkedRooms.Add(selectedRoom);
        }
        foreach (Connections connection in connections)
        {
            if (connection.roomOne == roomToCheck && !checkedRooms.Contains(connection.roomTwo))
            {
                if (!toDoRooms.Contains(connection.roomTwo))
                {
                    toDoRooms.Add(connection.roomTwo);
                }
            }
            else if (connection.roomTwo == roomToCheck && !checkedRooms.Contains(connection.roomOne))
            {
                if (!toDoRooms.Contains(connection.roomOne))
                {
                    toDoRooms.Add(connection.roomOne);
                }
            }
        }
        checkedRooms.Add(roomToCheck);
        toDoRooms.Remove(roomToCheck);
        if (checkedRooms.Count == doneRooms.Count + 1)
        {
            checkedRooms.Clear();
            toDoRooms.Clear();
            return true;
        }
        else if (toDoRooms.Count == 0)
        {
            checkedRooms.Clear();
            toDoRooms.Clear();
            return false;
        }
        if (!CheckRooms(toDoRooms[0], false))
        {
            return false;
        }
        return true;
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
    private void AddFloors(RectInt room, GameObject parentGameObject)
    {
        for (int width = 0; width < room.width; width++)
        {
            // Debug.Log(room.allPositionsWithin);
            for (int height = 0; height < room.height; height++)
            {
                if (!wallList.Contains(new(room.x + width + 0.5f, room.y + height + 0.5f)))
                {
                    Instantiate(floor, new(width + 0.5f + room.x, 0, height + 0.5f + room.y), new(1, 0, 0, 1), parentGameObject.transform);
                }
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
[System.Serializable]
public class Connections
{
    public RectInt door;
    public RectInt roomOne;
    public RectInt roomTwo;

    public Connections(RectInt door, RectInt roomOne, RectInt roomTwo)
    {
        this.door = door;
        this.roomOne = roomOne;
        this.roomTwo = roomTwo;
    }
}

