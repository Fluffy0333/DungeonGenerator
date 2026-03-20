using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt selectedRoom = new(0, 0, 100, 150);
    public List<RectInt> toDoRooms;
    public List<RectInt> doneRooms;
    public List<RectInt> doorsList;
    public List<Connections> connections = new();
    public int minSize = 7;
    public enum SpawnType { automatic, manual, slow };
    public SpawnType spawnType;
    public float cutSpeed = 0.1f;
    public GameObject cube;
    private RectInt savedRoom;
    private int sizeToRemove;
    private bool widthSplit;
    private bool canSplit = true;
    private bool checkSplitDone = false;
    private int currentCheckingRoom = 0;
    [Button("restart Generation")]
    private void RestartRoom()
    {
        selectedRoom = new(0, 0, 100, 150);
        doneRooms.Clear();
        toDoRooms.Clear();
        doorsList.Clear();
        connections.Clear();
        toDoRooms.Add(selectedRoom);
        checkSplitDone = false;
        currentCheckingRoom = 0;
    }
    void Start()
    {
        toDoRooms.Add(selectedRoom);
    }
    void Update()
    {
        switch (spawnType)
        {
            case SpawnType.automatic:
                if (toDoRooms.Count > 0)
                {
                    SplitRoom();
                }
                else if (checkSplitDone == false)
                {
                    checkSplitDone = true;
                    for (int i = 0; i < doneRooms.Count; i++)
                    {
                        for (int j = i + 1; j < doneRooms.Count; j++)
                        {
                            CheckSplits(i, j);
                        }
                    }
                }
                break;
            case SpawnType.manual:
                if (toDoRooms.Count > 0)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        SplitRoom();
                    }
                }
                else if (checkSplitDone == false)
                {
                    checkSplitDone = true;
                    StartCoroutine(SplitsManualRoom());
                }
                break;
            case SpawnType.slow:
                if (toDoRooms.Count > 0)
                {
                    if (canSplit)
                    {
                        StartCoroutine(Wait());
                        SplitRoom();
                    }
                }
                else if (checkSplitDone == false)
                {
                    checkSplitDone = true;
                    StartCoroutine(SplitSlowRoom());
                }
                break;
        }
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
        int selectedRoomInt = Random.Range(0, toDoRooms.Count);
        selectedRoom = toDoRooms[selectedRoomInt];
        if (selectedRoom.width > minSize * 2 && selectedRoom.height > minSize * 2)
        {
            widthSplit = Random.Range(0, 1f) > 0.5f;
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
        sizeToRemove = Random.Range(minSize, selectedRoom.width - minSize);
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
        sizeToRemove = Random.Range(minSize, selectedRoom.height - minSize);
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
                GraphMaker.DrawGraph(savedRoom, doneRooms, i, j);
            }
        }
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

