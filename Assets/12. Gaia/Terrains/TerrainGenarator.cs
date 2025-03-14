using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Dimensions")]
    public int width = 256;
    public int length = 256;
    public int height = 20;
    public float scale = 20f;

    [Header("Trees")]
    public float treeSpread = 15f;
    public GameObject[] treePrefabs;

    [Header("Rocks")]
    public float rockSpread = 20f;
    public GameObject[] rockPrefabs;

    private Terrain terrain;
    private TerrainData terrainData;

    void Start()
    {
        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("No Terrain component found!");
            return;
        }

        // Initialize or get TerrainData
        terrainData = terrain.terrainData;
        if (terrainData == null)
        {
            terrainData = new TerrainData();
            terrain.terrainData = terrainData;
        }

        // Set terrain size and resolution
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, height, length);

        GenerateTerrain();
        AddTerrainTextures();
        if (treePrefabs != null && treePrefabs.Length > 0)
        {
            AddTrees();
        }
        if (rockPrefabs != null && rockPrefabs.Length > 0)
        {
            AddRocks();
        }
    }

    void GenerateTerrain()
    {
        float[,] heights = new float[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                float xCoord = (float)x / width * scale;
                float zCoord = (float)z / length * scale;

                // Use multiple layers of Perlin noise for more natural terrain
                float height = 0;
                height += Mathf.PerlinNoise(xCoord, zCoord);
                height += Mathf.PerlinNoise(xCoord * 2, zCoord * 2) * 0.5f;
                height += Mathf.PerlinNoise(xCoord * 4, zCoord * 4) * 0.25f;

                heights[x, z] = height / 1.75f; // Normalize the height
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }

    void AddTerrainTextures()
    {
        // Create terrain layers
        TerrainLayer[] terrainLayers = new TerrainLayer[3];

        // Grass layer
        terrainLayers[0] = new TerrainLayer();
        terrainLayers[0].diffuseTexture = Resources.Load<Texture2D>("TerrainTextures/Grass");
        terrainLayers[0].tileSize = new Vector2(15, 15);

        // Dirt layer
        terrainLayers[1] = new TerrainLayer();
        terrainLayers[1].diffuseTexture = Resources.Load<Texture2D>("TerrainTextures/Dirt");
        terrainLayers[1].tileSize = new Vector2(15, 15);

        // Rock layer
        terrainLayers[2] = new TerrainLayer();
        terrainLayers[2].diffuseTexture = Resources.Load<Texture2D>("TerrainTextures/Rock");
        terrainLayers[2].tileSize = new Vector2(15, 15);

        terrainData.terrainLayers = terrainLayers;
    }

    void AddTrees()
    {
        TreePrototype[] treePrototypes = new TreePrototype[treePrefabs.Length];
        for (int i = 0; i < treePrefabs.Length; i++)
        {
            treePrototypes[i] = new TreePrototype();
            treePrototypes[i].prefab = treePrefabs[i];
        }
        terrainData.treePrototypes = treePrototypes;

        // Place trees
    //    List<TreeInstance> treeInstances = new List<TreeInstance>();
    //    for (int x = 0; x < width; x += (int)treeSpread)
    //    {
    //        for (int z = 0; z < length; z += (int)treeSpread)
    //        {
    //            if (Random.value > 0.5f) continue;

    //            TreeInstance tree = new TreeInstance();
    //            tree.position = new Vector3((float)x / width, 0, (float)z / length);
    //            tree.prototypeIndex = Random.Range(0, treePrefabs.Length);
    //            tree.widthScale = Random.Range(0.8f, 1.2f);
    //            tree.heightScale = Random.Range(0.8f, 1.2f);
    //            treeInstances.Add(tree);
    //        }
    //    }
    //    terrainData.SetTreeInstances(treeInstances.ToArray(), true);
    }

    void AddRocks()
    {
        // Similar to AddTrees but for rocks
        // Implementation depends on how you want to handle rock placement
    }
}