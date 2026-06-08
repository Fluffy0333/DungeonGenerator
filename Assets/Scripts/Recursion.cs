using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(DungeonGenerator))]
public class Recursion : MonoBehaviour
{
    DungeonGenerator dungeonGenerator;
    private void Start() {
        dungeonGenerator = GetComponent<DungeonGenerator>();
    }
    public void SpawnFloorsRecursive(List<Vector3> discovered, GameObject parentGameObject, float width, float height)
    {
        if (!dungeonGenerator.wallList.Contains(new(width + dungeonGenerator.selectedRoom.x, height + dungeonGenerator.selectedRoom.y)) && !discovered.Contains(new(width + dungeonGenerator.selectedRoom.x, 0, height + dungeonGenerator.selectedRoom.y)) && (dungeonGenerator.toDoFloors.Count > 0 || (width == 1.5f && height == 1.5f)))
        {
            Instantiate(dungeonGenerator.floor, new(width + dungeonGenerator.selectedRoom.x, 0, height + dungeonGenerator.selectedRoom.y), new(1, 0, 0, 1), parentGameObject.transform);
            discovered.Add(new(width + dungeonGenerator.selectedRoom.x, 0, height + dungeonGenerator.selectedRoom.y));
            foreach (Vector3 nextToEachOther in CheckAdjacent(new(width, 0, height), discovered, dungeonGenerator.selectedRoom))
            {
                dungeonGenerator.toDoFloors.Add(nextToEachOther);
            }
            if (dungeonGenerator.toDoFloors.Count > 0)
            {
                SpawnFloorsRecursive(discovered, parentGameObject, dungeonGenerator.toDoFloors[dungeonGenerator.toDoFloors.Count - 1].x, dungeonGenerator.toDoFloors[dungeonGenerator.toDoFloors.Count - 1].z);
                dungeonGenerator.toDoFloors.Remove(dungeonGenerator.toDoFloors[dungeonGenerator.toDoFloors.Count - 1]);
            }
        }
        else
        {
            return;
        }
    }
    private List<Vector3> CheckAdjacent(Vector3 floor, List<Vector3> discovered, RectInt room)
    {
        Vector3 roomchecker = floor + new Vector3(room.x, 0, room.y);
        List<Vector3> Adjacent = new();
        if (!dungeonGenerator.wallList.Contains(new(roomchecker.x + 1, roomchecker.z)) && !discovered.Contains(new(roomchecker.x + 1, 0, roomchecker.z)))
        {
            Adjacent.Add(new(floor.x + 1, 0, floor.z));
        }
        if (!dungeonGenerator.wallList.Contains(new(roomchecker.x - 1, roomchecker.z)) && !discovered.Contains(new(roomchecker.x - 1, 0, roomchecker.z)))
        {
            Adjacent.Add(new(floor.x - 1, 0, floor.z));
        }
        if (!dungeonGenerator.wallList.Contains(new(roomchecker.x, roomchecker.z + 1)) && !discovered.Contains(new(roomchecker.x, 0, roomchecker.z + 1)))
        {
            Adjacent.Add(new(floor.x, 0, floor.z + 1));
        }
        if (!dungeonGenerator.wallList.Contains(new(roomchecker.x, roomchecker.z - 1)) && !discovered.Contains(new(roomchecker.x, 0, roomchecker.z - 1)))
        {
            Adjacent.Add(new(floor.x, 0, floor.z - 1));
        }
        return Adjacent;
    }
}
