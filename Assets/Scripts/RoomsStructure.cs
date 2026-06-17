using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DungeonGenerator))]
public class RoomsStructure : MonoBehaviour
{
    [HideInInspector]
    public HashSet<Vector2> wallList = new();
    [HideInInspector]
    public HashSet<Vector2> doorList = new();
    DungeonGenerator dungeonGenerator;
    void Start()
    {
        dungeonGenerator = GetComponent<DungeonGenerator>();
    }

    public void AddWalls(RectInt room)
    {
        //we don't HAVE to check if the wall list already contains things as it's a hashset and it will just override each other but it's still better to get in the mindset to do do it.
        for (int width = 0; width < room.width; width++)
        {
            float widthLocation = room.x + width + 0.5f;
            float heightLocation = room.y + room.height - 0.5f;
            if (!wallList.Contains(new(widthLocation, room.y + 0.5f)))
            {
                wallList.Add(new(widthLocation, room.y + 0.5f));
            }
            if (!wallList.Contains(new(widthLocation, heightLocation)))
            {
                wallList.Add(new(widthLocation, heightLocation));
            }
        }
        for (int height = 0; height < room.height; height++)
        {
            float widthLocation = room.x + room.width - 0.5f;
            float heightLocation = room.y + height + 0.5f;
            if (!wallList.Contains(new(room.x + 0.5f, heightLocation)))
            {
                wallList.Add(new(room.x + 0.5f, heightLocation));
            }
            if (!wallList.Contains(new(widthLocation, heightLocation)))
            {
                wallList.Add(new(widthLocation, heightLocation));
            }
        }
    }
    private void WallDoorList(RectInt door)
    {
        //door.x/door.y is the bottom left/right of the door, adding 0.5f to x and y is (1,1) then adding the half of the height/width + 0.5f is either (2,1) or (1, 2) depending if the door is in height or width length.
        doorList.Add(new(door.x + 0.5f, door.y + 0.5f));
        doorList.Add(new(door.x + door.width / 2 + 0.5f, door.y + door.height / 2 + 0.5f));
    }
    public void AddFloorToDoors(HashSet<Vector2> doors, GameObject parentGameObject)
    {
        foreach (RectInt door in dungeonGenerator.doorsList)
        {
            //add doors to a doorlist so we know where the doors are precisly for inputting floors later
            WallDoorList(door);
        }
        foreach (Vector2 door in doorList)
        {
            Instantiate(dungeonGenerator.floor, new(door.x, 0, door.y), new(1, 0, 0, 1), parentGameObject.transform);
            wallList.Remove(door);
        }
    }
}
