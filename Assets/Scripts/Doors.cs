using UnityEngine;

public class Doors : MonoBehaviour
{
    //makes a door, if the door is too small (space that is 3 or lower (1x1, 2x1/1x2, 3x1/1x3)) don't make it, if the door is too big make it smaller till it is an 2x1/1x2.
    public static RectInt MakeDoor(RectInt selectedRoom)
    {
        if (selectedRoom.width == 3 || selectedRoom.height == 3)
        {
            return RectInt.zero;
        }
        else if (selectedRoom.width > 2 || selectedRoom.height > 2)
        {
            while (selectedRoom.width > 2)
            {
                selectedRoom.width--;
                if (selectedRoom.width > 2)
                {
                    selectedRoom.width--;
                    selectedRoom.x++;
                }
            }
            while (selectedRoom.height > 2)
            {
                selectedRoom.height--;
                if (selectedRoom.height > 2)
                {
                    selectedRoom.height--;
                    selectedRoom.y++;
                }
            }
            return selectedRoom;
        }
        else
        {
            return RectInt.zero;
        }
    }
}
