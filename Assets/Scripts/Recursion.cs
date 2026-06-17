using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(DungeonGenerator))]
[RequireComponent(typeof(RoomsStructure))]
public class Recursion : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    private RoomsStructure roomsStructure;
    private List<Vector3> toDoFloors = new();
    private void Start() {
        roomsStructure = GetComponent<RoomsStructure>();
        dungeonGenerator = GetComponent<DungeonGenerator>();
    }
    public void SpawnFloorsRecursive(List<Vector2> discovered, GameObject parentGameObject, float width, float height)
    {
        Vector2 floorPosition = new(width + dungeonGenerator.selectedRoom.x, height + dungeonGenerator.selectedRoom.y);
        if (!roomsStructure.wallList.Contains(floorPosition) && !discovered.Contains(floorPosition) && (toDoFloors.Count > 0 || (width == 1.5f && height == 1.5f)))
        {
            Instantiate(dungeonGenerator.floor, new(floorPosition.x, 0, floorPosition.y), new(1, 0, 0, 1), parentGameObject.transform);
            discovered.Add(floorPosition);
            foreach (Vector3 nextToEachOther in CheckAdjacent(new(width, 0, height), discovered, dungeonGenerator.selectedRoom))
            {
                toDoFloors.Add(nextToEachOther);
            }
            if (toDoFloors.Count > 0)
            {
                Vector3 chosenFloor = toDoFloors[toDoFloors.Count - 1];
                SpawnFloorsRecursive(discovered, parentGameObject, chosenFloor.x, chosenFloor.z);
                toDoFloors.Remove(chosenFloor);
            }
        }
        else
        {
            return;
        }
    }
    private List<Vector3> CheckAdjacent(Vector3 floor, List<Vector2> discovered, RectInt room)
    {
        Vector3 roomFloorChecker = floor + new Vector3(room.x, 0, room.y);
        List<Vector3> Adjacent = new();
        if (!roomsStructure.wallList.Contains(new(roomFloorChecker.x + 1, roomFloorChecker.z)) && !discovered.Contains(new(roomFloorChecker.x + 1, roomFloorChecker.z)))
        {
            Adjacent.Add(new(floor.x + 1, 0, floor.z));
        }
        if (!roomsStructure.wallList.Contains(new(roomFloorChecker.x - 1, roomFloorChecker.z)) && !discovered.Contains(new(roomFloorChecker.x - 1, roomFloorChecker.z)))
        {
            Adjacent.Add(new(floor.x - 1, 0, floor.z));
        }
        if (!roomsStructure.wallList.Contains(new(roomFloorChecker.x, roomFloorChecker.z + 1)) && !discovered.Contains(new(roomFloorChecker.x, roomFloorChecker.z + 1)))
        {
            Adjacent.Add(new(floor.x, 0, floor.z + 1));
        }
        if (!roomsStructure.wallList.Contains(new(roomFloorChecker.x, roomFloorChecker.z - 1)) && !discovered.Contains(new(roomFloorChecker.x, roomFloorChecker.z - 1)))
        {
            Adjacent.Add(new(floor.x, 0, floor.z - 1));
        }
        return Adjacent;
    }
}
