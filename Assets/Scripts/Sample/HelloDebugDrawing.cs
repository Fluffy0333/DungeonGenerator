using UnityEngine;

public class HelloDebugDrawing : MonoBehaviour
{
    void OnDrawGizmos()
    {
        
        RectInt a = new RectInt(0, 0, 10, 10);
        RectInt b = new RectInt(5, 5, 10, 10);
        RectInt c = AlgorithmsUtils.Intersect(a, b);
        
        Debug.DrawLine(Vector3.zero, Vector3.right, Color.red);
        Debug.DrawLine(Vector3.zero, Vector3.up, Color.green);
        Debug.DrawLine(Vector3.zero, Vector3.forward, Color.blue);

        AlgorithmsUtils.DebugRectInt(a, Color.red);
        AlgorithmsUtils.DebugRectInt(b, Color.green);
        AlgorithmsUtils.DebugRectInt(c, Color.yellow);
    }

}
