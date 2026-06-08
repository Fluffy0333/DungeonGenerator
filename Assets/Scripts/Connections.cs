using UnityEngine;

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
