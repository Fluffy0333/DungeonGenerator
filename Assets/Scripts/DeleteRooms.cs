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
        if (dungeonGenerator.CheckRooms(doneRooms[rand.Next(0, doneRooms.Count)], true))
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
}
