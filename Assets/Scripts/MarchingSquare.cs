using System.Collections;
using NaughtyAttributes;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(DungeonGenerator))]
public class MarchingSquare : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    public Vector2 currentLocation = new(1, 1);
    public NavMeshSurface navMeshSurface;
    private Vector2 topLeft;
    private Vector2 topRight;
    private Vector2 bottomLeft;
    private Vector2 bottomRight;
    private int totalNumber;
    private bool placeWalls = false;
    private GameObject parentGameObject;
    [HideInInspector]
    public RectInt dungeonBounds;
    [HideInInspector]
    public float delay = 0f;
    [HideInInspector]
    public bool waitForInput = false;
    public GameObject[] walls;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentGameObject = new($"walls");
        dungeonGenerator = GetComponent<DungeonGenerator>();
    }
    void Update()
    {
        AlgorithmsUtils.DebugRectInt(new((int)currentLocation.x, (int)currentLocation.y, 2, 2), Color.yellow);
        if (!placeWalls)
        {
            placeWalls = true;
            StartCoroutine(PlaceWalls());
        }
    }
    IEnumerator PlaceWalls()
    {
        for (int height = 0; height < dungeonBounds.height; height++)
        {
            for (int width = 0; width < dungeonBounds.width; width++)
            {
                totalNumber = 0;
                if (delay > 0)
                {
                    yield return new WaitForSeconds(delay);
                }
                else if(waitForInput)
                {
                    WaitUntil wait = new(() => Input.GetKeyDown(KeyCode.Space));
                }
                currentLocation = new(dungeonBounds.x + width, dungeonBounds.y + height);
                topLeft = new(currentLocation.x - 0.5f, currentLocation.y + 0.5f);
                topRight = new(currentLocation.x + 0.5f, currentLocation.y + 0.5f);
                bottomLeft = new(currentLocation.x - 0.5f, currentLocation.y - 0.5f);
                bottomRight = new(currentLocation.x + 0.5f, currentLocation.y - 0.5f);
                if (dungeonGenerator.wallList.Contains(bottomRight))
                {
                    totalNumber += 1;
                }
                if (dungeonGenerator.wallList.Contains(topRight))
                {
                    totalNumber += (1 << 1);
                }
                if (dungeonGenerator.wallList.Contains(topLeft))
                {
                    totalNumber += (1 << 2);
                }
                if (dungeonGenerator.wallList.Contains(bottomLeft))
                {
                    totalNumber += (1 << 3);
                }
                if (totalNumber != 0)
                {
                    GameObject newWall = Instantiate(walls[totalNumber], new(currentLocation.x, 0, currentLocation.y), transform.rotation, parentGameObject.transform);
                }
            }
        }
        BakeNavMesh();
    }
    [Button]
    private void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }

}
