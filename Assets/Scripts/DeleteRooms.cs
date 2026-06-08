using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(DungeonGenerator))]
public class DeleteRooms : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    void Start()
    {
        dungeonGenerator = GetComponent<DungeonGenerator>();
    }
    //check if room can be deleted -> if it can, increase percentage of deleted and return. if it cannot delete the room increase attempts used, if attempts/percentage reaches max then rooms won't be deleted anymore.
    public bool CheckDeleteRoom(List<Connections> connections, List<RectInt> doneRooms, System.Random rand, float percentageDeleted, float initialRoomsAmount, int removeAttempts, float percentageToDelete, List<RectInt> doorsList)
    {
        dungeonGenerator.selectedRoom = doneRooms[0];
        doneRooms.Remove(dungeonGenerator.selectedRoom);
        if (CheckRooms(doneRooms[rand.Next(0, doneRooms.Count)], true, dungeonGenerator.selectedRoom))
        {
            DeleteRoom(dungeonGenerator.selectedRoom);
            percentageDeleted += 1 / (initialRoomsAmount / 100);
            dungeonGenerator.percentageDeleted = percentageDeleted;
        }
        else
        {
            dungeonGenerator.removeAttemptAmount += 1;
            doneRooms.Add(dungeonGenerator.selectedRoom);
            if (dungeonGenerator.removeAttemptAmount >= removeAttempts)
            {
                Debug.Log($"Did not reach percentage: {percentageDeleted}/{percentageToDelete}");
                return true;
            }
        }
        if (percentageDeleted >= percentageToDelete)
        {
            Debug.Log($"Reached percentage: {percentageDeleted}/{percentageToDelete}");
            dungeonGenerator.selectedRoom = doneRooms[0];
            return true;
        }
        return false;
    }
    private void DeleteRoom(RectInt selectedRoom)
    {
        foreach (Connections connection in dungeonGenerator.connections.ToList())
        {
            if (connection.roomOne == selectedRoom)
            {
                dungeonGenerator.connections.Remove(connection);
                dungeonGenerator.doorsList.Remove(connection.door);
            }
            else if (connection.roomTwo == selectedRoom)
            {
                dungeonGenerator.connections.Remove(connection);
                dungeonGenerator.doorsList.Remove(connection.door);
            }
        }
    }
        //checks every room to see if every room are connected to each other if we delete firstRoom, this is used to then delete the firstRoom if all still connect.
    public bool CheckRooms(RectInt roomToCheck, bool firstRoom, RectInt selectedRoom)
    {
        if (firstRoom)
        {
            dungeonGenerator.checkedRooms.Add(selectedRoom);
        }
        foreach (Connections connection in dungeonGenerator.connections)
        {
            if (connection.roomOne == roomToCheck && !dungeonGenerator.checkedRooms.Contains(connection.roomTwo))
            {
                if (!dungeonGenerator.toDoRooms.Contains(connection.roomTwo))
                {
                    dungeonGenerator.toDoRooms.Add(connection.roomTwo);
                }
            }
            else if (connection.roomTwo == roomToCheck && !dungeonGenerator.checkedRooms.Contains(connection.roomOne))
            {
                if (!dungeonGenerator.toDoRooms.Contains(connection.roomOne))
                {
                    dungeonGenerator.toDoRooms.Add(connection.roomOne);
                }
            }
        }
        dungeonGenerator.checkedRooms.Add(roomToCheck);
        dungeonGenerator.toDoRooms.Remove(roomToCheck);
        if (dungeonGenerator.checkedRooms.Count == dungeonGenerator.doneRooms.Count + 1)
        {
            dungeonGenerator.checkedRooms.Clear();
            dungeonGenerator.toDoRooms.Clear();
            return true;
        }
        else if (dungeonGenerator.toDoRooms.Count == 0)
        {
            dungeonGenerator.checkedRooms.Clear();
            dungeonGenerator.toDoRooms.Clear();
            return false;
        }
        if (!CheckRooms(dungeonGenerator.toDoRooms[0], false, dungeonGenerator.selectedRoom))
        {
            return false;
        }
        return true;
    }
}
