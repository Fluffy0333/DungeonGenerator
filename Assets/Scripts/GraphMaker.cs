using System.Collections.Generic;
using UnityEngine;

static class GraphMaker
{
    public static void DrawGraph(RectInt savedRoom, List<RectInt> doneRooms, int i, int j)
    {
        Debug.DrawLine(new(savedRoom.x + savedRoom.width / 2f, 0, savedRoom.y + savedRoom.height / 2f), new(doneRooms[i].x + doneRooms[i].width / 2f, 0, doneRooms[i].y + doneRooms[i].height / 2f), Color.red, float.PositiveInfinity);
        Debug.DrawLine(new(savedRoom.x + savedRoom.width / 2f, 0, savedRoom.y + savedRoom.height / 2f), new(doneRooms[j].x + doneRooms[j].width / 2f, 0, doneRooms[j].y + doneRooms[j].height / 2f), Color.red, float.PositiveInfinity);
    }
}
