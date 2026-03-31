using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using NaughtyAttributes;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt selectedRoom = new(0, 0, 100, 150);
    public List<RectInt> toDoRooms;
    public List<RectInt> doneRooms;
    public List<RectInt> doorsList;
    public List<RectInt> checkedRooms;
    public List<Connections> connections = new();
    public int minSize = 7;
    public enum SpawnType { automatic, manual, slow };
    public SpawnType spawnType;
    public float cutSpeed = 0.1f;
    [Range(0, 75)]
    public float percentageToDelete;
    public int removeAttempts = 5;
    public int seed;
    public GameObject cube;
    private System.Random rand = new System.Random();
    private RectInt savedRoom;
    private int removeAttemptAmount;
    private float percentageDeleted;
    private float initialRoomsAmount;
    private int sizeToRemove;
    private bool widthSplit;
    private bool canSplit = true;
    private bool checkSplitDone = false;
    private bool roomsDeleted = false;
    [Button("restart Generation")]
    private void RestartRoom()
    {
        selectedRoom = new(0, 0, 100, 150);
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
    }
    void Start()
    {
        if (seed > 0)
        {
            rand = new System.Random(seed);
        }
        toDoRooms.Add(selectedRoom);
    }
    void Update()
    {
        switch (spawnType)
        {
            case SpawnType.automatic:
                if (toDoRooms.Count > 0 && checkSplitDone == false)
                {
                    SplitRoom();
                }
                else if (checkSplitDone == false)
                {
                    initialRoomsAmount = doneRooms.Count;
                    doneRooms.Sort((a, b) => (a.width + a.height).CompareTo(b.width + b.height));
                    checkSplitDone = true;
                    for (int i = 0; i < doneRooms.Count; i++)
                    {
                        for (int j = i + 1; j < doneRooms.Count; j++)
                        {
                            CheckSplits(i, j);
                        }
                    }
                    Debug.Log($"amount of donerooms: {doneRooms.Count}");
                    while (!roomsDeleted)
                    {
                        selectedRoom = doneRooms[0];
                        doneRooms.Remove(selectedRoom);
                        if (CheckRooms(doneRooms[rand.Next(0, doneRooms.Count)], true))
                        {
                            DeleteRoom();
                            percentageDeleted += 1 / (initialRoomsAmount / 100);
                            Debug.Log("Room can be deleted >w<");
                        }
                        else
                        {
                            removeAttemptAmount += 1;
                            doneRooms.Add(selectedRoom);
                            Debug.Log("Room cannot be deleted QwQ");
                            if (removeAttemptAmount >= removeAttempts)
                            {
                                roomsDeleted = true;
                                Debug.Log($"Did not reach percentage: {percentageDeleted}/{percentageToDelete}");
                            }
                        }
                        if (percentageDeleted >= percentageToDelete)
                        {
                            Debug.Log($"Reached percentage: {percentageDeleted}/{percentageToDelete}");
                            roomsDeleted = true;
                        }
                        // roomsDeleted = true;
                    }
                }
                break;
            case SpawnType.manual:
                if (toDoRooms.Count > 0 && checkSplitDone == false)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        SplitRoom();
                    }
                }
                else if (checkSplitDone == false)
                {
                    doneRooms.Sort((a, b) => (a.x + a.y).CompareTo(b.x + b.y));
                    checkSplitDone = true;
                    StartCoroutine(SplitsManualRoom());
                }
                break;
            case SpawnType.slow:
                if (toDoRooms.Count > 0 && checkSplitDone == false)
                {
                    if (canSplit)
                    {
                        StartCoroutine(Wait());
                        SplitRoom();
                    }
                }
                else if (checkSplitDone == false)
                {
                    doneRooms.Sort((a, b) => (a.x + a.y).CompareTo(b.x + b.y));
                    checkSplitDone = true;
                    StartCoroutine(SplitSlowRoom());
                }
                break;
        }
        DrawDebug();
    }

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
        //add for debugging with cubes
        // GameObject spawnedCube = Instantiate(cube, new(roomSelected.x + roomSelected.width / 2, transform.position.y, roomSelected.y + roomSelected.height / 2), Quaternion.identity);
        // spawnedCube.name = $"cube({doneRooms.Count - 1})";
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
    private void CheckSplits(int i, int j)
    {
        if (AlgorithmsUtils.Intersects(doneRooms[i], doneRooms[j]))
        {
            savedRoom = AlgorithmsUtils.Intersect(doneRooms[i], doneRooms[j]);
            savedRoom = Doors.MakeDoor(savedRoom);
            if (savedRoom != RectInt.zero)
            {
                doorsList.Add(savedRoom);
                //savedRoom, doneRooms[i], doneRooms[j]
                connections.Add(new Connections(savedRoom, doneRooms[i], doneRooms[j]));
                // GraphMaker.DrawGraph(savedRoom, doneRooms, i, j);
            }
        }
    }
    private void DeleteRoom()
    {
        foreach (Connections connection in connections.ToList())
        {
            if (connection.roomOne == selectedRoom)
            {
                connections.Remove(connection);
                doorsList.Remove(connection.door);
            }
            else if (connection.roomTwo == selectedRoom)
            {
                connections.Remove(connection);
                doorsList.Remove(connection.door);
            }
        }
    }
    private bool CheckRooms(RectInt roomToCheck, bool firstRoom)
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
    IEnumerator Wait()
    {
        canSplit = false;
        yield return new WaitForSeconds(cutSpeed);
        canSplit = true;
    }
    IEnumerator SplitSlowRoom()
    {
        for (int i = 0; i < doneRooms.Count; i++)
        {
            yield return new WaitForSeconds(cutSpeed);
            for (int j = i + 1; j < doneRooms.Count; j++)
            {
                CheckSplits(i, j);
            }
        }
    }
    IEnumerator SplitsManualRoom()
    {
        while (true)
        {
            for (int i = 0; i < doneRooms.Count; i++)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    for (int j = i + 1; j < doneRooms.Count; j++)
                    {
                        CheckSplits(i, j);
                    }
                }
                yield return null;
            }
            yield break;
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

