using System.Collections;
using NaughtyAttributes;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(DungeonGenerator))]
public class MarchingSquare : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    private RoomsStructure roomsStructure;
    public Vector2Int currentLocation = new(1, 1);
    public NavMeshSurface navMeshSurface;
    private int totalNumber;
    private bool placeWalls = false;
    private GameObject parentGameObject;
    [HideInInspector]
    public RectInt dungeonBoundaries;
    [HideInInspector]
    public float delay = 0f;
    [HideInInspector]
    public bool waitForInput = false;
    public GameObject[] walls;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        dungeonGenerator = GetComponent<DungeonGenerator>();
        roomsStructure = GetComponent<RoomsStructure>();
        var tempRoom = new GameObject($"walls");
        parentGameObject = Instantiate(tempRoom, transform.position, transform.rotation, dungeonGenerator.roomParent.transform);
        Destroy(tempRoom);
        placeWalls = false;
    }
    void Update()
    {
        AlgorithmsUtils.DebugRectInt(new(currentLocation.x, currentLocation.y, 2, 2), Color.yellow);
        if (!placeWalls)
        {
            placeWalls = true;
            StartCoroutine(PlaceWalls());
        }
    }
    IEnumerator PlaceWalls()
    {
        for (int height = 0; height < dungeonBoundaries.height; height++)
        {
            for (int width = 0; width < dungeonBoundaries.width; width++)
            {
                totalNumber = 0;
                if (delay > 0)
                {
                    yield return new WaitForSeconds(delay);
                }
                else if (waitForInput)
                {
                    WaitUntil wait = new(() => Input.GetKeyDown(KeyCode.Space));
                }
                currentLocation = new(dungeonBoundaries.x + width, dungeonBoundaries.y + height);
                Vector2 topLeft = new(currentLocation.x - 0.5f, currentLocation.y + 0.5f);
                Vector2 topRight = new(currentLocation.x + 0.5f, currentLocation.y + 0.5f);
                Vector2 bottomLeft = new(currentLocation.x - 0.5f, currentLocation.y - 0.5f);
                Vector2 bottomRight = new(currentLocation.x + 0.5f, currentLocation.y - 0.5f);
                if (roomsStructure.wallList.Contains(bottomRight))
                {
                    totalNumber += 1;
                }

                if (roomsStructure.wallList.Contains(topRight))
                {
                    totalNumber += (1 << 1);
                }

                if (roomsStructure.wallList.Contains(topLeft))
                {
                    totalNumber += (1 << 2);
                }

                if (roomsStructure.wallList.Contains(bottomLeft))
                {
                    totalNumber += (1 << 3);
                }

                if (totalNumber > 0)
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
