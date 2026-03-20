using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt selectedRoom = new(0, 0, 100, 150);
    public List<RectInt> toDoRooms;
    public List<RectInt> doneRooms;
    public List<RectInt> doors;
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
        doors.Clear();
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
        if (toDoRooms.Count > 0)
        {
            switch (spawnType)
            {
                case SpawnType.automatic:
                    SplitRoom();
                    break;
                case SpawnType.manual:
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        SplitRoom();
                    }
                    break;
                case SpawnType.slow:
                    if (canSplit)
                    {
                        StartCoroutine(Wait());
                    }
                    break;
            }
        }
        else if (checkSplitDone == false)
        {
            checkSplitDone = true;
            CheckSplits();
        }
        foreach (RectInt room in toDoRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.red);
        }
        foreach (RectInt room in doneRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green);
        }
        foreach (RectInt doorway in doors)
        {
            AlgorithmsUtils.DebugRectInt(doorway, Color.magenta);
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
    private void CheckSplits()
    {
        for (int i = 0; i < doneRooms.Count; i++)
        {
            for (int j = i + 1; j < doneRooms.Count; j++)
            {
                if (AlgorithmsUtils.Intersects(doneRooms[i], doneRooms[j]))
                {
                    savedRoom = AlgorithmsUtils.Intersect(doneRooms[i], doneRooms[j]);
                    if (savedRoom.width == 3 || savedRoom.height == 3)
                    {
                        Debug.LogWarning("Door found to be in a too narrow area");
                    }
                    else if (savedRoom.width > 2)
                    {
                        while (savedRoom.width > 2)
                        {
                            savedRoom.width--;
                            if (savedRoom.width > 2)
                            {
                                savedRoom.width--;
                                savedRoom.x++;
                            }
                        }
                        doors.Add(savedRoom);
                        GraphMaker.DrawGraph(savedRoom, doneRooms, i, j);
                    }
                    else if (savedRoom.height > 2)
                    {
                        while (savedRoom.height > 2)
                        {
                            savedRoom.height--;
                            if (savedRoom.height > 2)
                            {
                                savedRoom.height--;
                                savedRoom.y++;
                            }
                        }
                        doors.Add(savedRoom);
                        GraphMaker.DrawGraph(savedRoom, doneRooms, i, j);
                    }
                }
            }
        }
    }
    IEnumerator Wait()
    {
        canSplit = false;
        yield return new WaitForSeconds(cutSpeed);
        SplitRoom();
        canSplit = true;
    }
}
