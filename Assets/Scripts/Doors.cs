using UnityEngine;

public class Doors : MonoBehaviour
{
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
