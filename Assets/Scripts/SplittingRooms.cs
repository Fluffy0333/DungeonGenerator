using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DungeonGenerator))]
public class SplittingRooms : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    private RectInt selectedRoom;
    private int sizeToRemove;
    private int minSize;
    void Start()
    {
        dungeonGenerator = GetComponent<DungeonGenerator>();
        minSize = dungeonGenerator.minSize;
    }
    public void SplitRoom()
    {
        int selectedRoomInt = dungeonGenerator.rand.Next(0, dungeonGenerator.toDoRooms.Count);
        selectedRoom = dungeonGenerator.toDoRooms[selectedRoomInt];
        if (selectedRoom.width > minSize * 2 && selectedRoom.height > minSize * 2)
        {
            if (dungeonGenerator.rand.Next(0, 100) > 50)
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
        dungeonGenerator.toDoRooms[selectedRoomInt] = selectedRoom;
        if (selectedRoom.height <= minSize * 2 && selectedRoom.width <= minSize * 2)
        {
            AddDoneRoom(selectedRoom);
        }
    }
    private void ReduceWidth()
    {
        sizeToRemove = dungeonGenerator.rand.Next(minSize, selectedRoom.width - minSize);
        RectInt savedRoom = new(selectedRoom.width - sizeToRemove + selectedRoom.xMin, selectedRoom.yMin, sizeToRemove, selectedRoom.height);
        if (savedRoom.width > minSize * 2 || savedRoom.height > minSize * 2)
        {
            dungeonGenerator.toDoRooms.Add(savedRoom);
        }
        else
        {
            AddDoneRoom(savedRoom);
        }
        selectedRoom.width = selectedRoom.width - sizeToRemove + 1;
    }
    private void ReduceHeight()
    {
        sizeToRemove = dungeonGenerator.rand.Next(minSize, selectedRoom.height - minSize);
        RectInt savedRoom = new(selectedRoom.xMin, selectedRoom.height - sizeToRemove + selectedRoom.yMin, selectedRoom.width, sizeToRemove);
        if (savedRoom.width > minSize * 2 || savedRoom.height > minSize * 2)
        {
            dungeonGenerator.toDoRooms.Add(savedRoom);
        }
        else
        {
            AddDoneRoom(savedRoom);
        }
        selectedRoom.height = selectedRoom.height - sizeToRemove + 1;
    }
    private void AddDoneRoom(RectInt roomSelected)
    {
        dungeonGenerator.doneRooms.Add(roomSelected);
        dungeonGenerator.toDoRooms.Remove(roomSelected);
    }
}
