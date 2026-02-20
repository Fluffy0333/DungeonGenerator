using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonGenerator : MonoBehaviour
{
    //put rooms in 2 lists done and not yet done
    //choose a random room in not yet done and split it (make two rooms in one)
    //make the split random (e.g 60/40 30/70)
    //if room is too small set it to done
    //randomly pick between height and width
    public RectInt selectedRoom = new(0, 0, 100, 150);
    public List<RectInt> toDoRooms;
    public List<RectInt> doneRooms;
    public int minSize = 7;
    public enum SpawnType { automatic, manual };
    public SpawnType spawnType;
    private RectInt savedRoom;
    private int sizeToRemove;
    private bool reduceWidth;
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
                break;
            case SpawnType.manual:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SplitRoom();
                }
                break;
        }
        foreach (RectInt room in toDoRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green);
        }
        foreach (RectInt room in doneRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow);
        }
    }
    private void SplitRoom()
    {
        int selectedRoomInt = Random.Range(0, toDoRooms.Count);
        selectedRoom = toDoRooms[selectedRoomInt];
        reduceWidth = Random.value > 0.5f;

        if (selectedRoom.width > minSize * 2)
        {
            sizeToRemove = Random.Range(minSize, selectedRoom.width - minSize);
            savedRoom = new(Mathf.CeilToInt(selectedRoom.width - sizeToRemove) + selectedRoom.xMin, selectedRoom.yMin, Mathf.CeilToInt(sizeToRemove), selectedRoom.height);
            if (savedRoom.width > minSize || savedRoom.height > minSize)
            {
                toDoRooms.Add(savedRoom);
            }
            else
            {
                AddDoneRoom(savedRoom);
            }
            selectedRoom.width = Mathf.CeilToInt(selectedRoom.width - sizeToRemove + 1);
        }
        else if(selectedRoom.height > minSize * 2)
        {
            sizeToRemove = Random.Range(minSize, selectedRoom.height - minSize);
            savedRoom = new(selectedRoom.xMin, Mathf.CeilToInt(selectedRoom.height - sizeToRemove) + selectedRoom.yMin, selectedRoom.width, Mathf.CeilToInt(sizeToRemove));
            if (savedRoom.width > minSize || savedRoom.height > minSize)
            {
                toDoRooms.Add(savedRoom);
            }
            else
            {
                AddDoneRoom(savedRoom);
            }
            selectedRoom.height = Mathf.CeilToInt(selectedRoom.height - sizeToRemove + 1);
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
    }
    private void ReduceWidth()
    {
        // if (selectedRoom.width <= minSize * 2 && selectedRoom.height > minSize * 2)
        // {
        //     ReduceHeight();
        //     return;
        // }
        // else if (selectedRoom.height <= minSize * 2)
        // {
        //     Debug.LogWarning("Done room found in not done list!");
        //     AddDoneRoom(selectedRoom);
        //     return;
        // }
        // sizeToRemove = Random.Range(minSize, selectedRoom.width - minSize);
        // savedRoom = new(Mathf.CeilToInt(selectedRoom.width - sizeToRemove) + selectedRoom.xMin, selectedRoom.yMin, Mathf.CeilToInt(sizeToRemove), selectedRoom.height);
        // if (savedRoom.width > minSize || savedRoom.height > minSize)
        // {
        //     toDoRooms.Add(savedRoom);
        // }
        // else
        // {
        //     AddDoneRoom(savedRoom);
        // }
        // selectedRoom.width = Mathf.CeilToInt(selectedRoom.width - sizeToRemove + 1);
    }
    private void ReduceHeight()
    {
        // if (selectedRoom.height <= minSize * 2 && selectedRoom.width > minSize * 2)
        // {
        //     ReduceWidth();
        //     return;
        // }
        // else if (selectedRoom.width <= minSize * 2)
        // {
        //     Debug.LogWarning("Done room found in not done list!");
        //     AddDoneRoom(selectedRoom);
        //     return;
        // }
        // sizeToRemove = Random.Range(minSize, selectedRoom.height - minSize);
        // savedRoom = new(selectedRoom.xMin, Mathf.CeilToInt(selectedRoom.height - sizeToRemove) + selectedRoom.yMin, selectedRoom.width, Mathf.CeilToInt(sizeToRemove));
        // if (savedRoom.width > minSize || savedRoom.height > minSize)
        // {
        //     toDoRooms.Add(savedRoom);
        // }
        // else
        // {
        //     AddDoneRoom(savedRoom);
        // }
        // selectedRoom.height = Mathf.CeilToInt(selectedRoom.height - sizeToRemove + 1);
    }
}
