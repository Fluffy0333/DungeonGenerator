using UnityEngine;

public class Doors : MonoBehaviour
{
    //makes a door, if the door is too small (space that is 3 or lower (1x1, 2x1/1x2, 3x1/1x3)) don't make it, if the door is too big make it smaller till it is an 2x1/1x2.
    public static RectInt MakeDoor(RectInt savedRoom)
    {
        if (savedRoom.width == 3 || savedRoom.height == 3)
        {

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
            return savedRoom;
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
            return savedRoom;
        }
        return RectInt.zero;
    }
}
