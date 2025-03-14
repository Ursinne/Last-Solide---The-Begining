using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [Header("Terrain Settings")]
    public Terrain terrain;
    public TerrainData terrainData;

    [Header("Textures")]
    public Texture2D grassTexture;
    public Texture2D dirtTexture;
    public Texture2D sandTexture;
    public Texture2D rockTexture;

    [Header("Normal Maps")]
    public Texture2D grassNormal;
    public Texture2D dirtNormal;
    public Texture2D sandNormal;
    public Texture2D rockNormal;

    [Header("Texture Settings")]
    public float tileSize = 15f;

    void Start()
    {
        if (terrain == null)
        {
            terrain = GetComponent<Terrain>();
        }

        if (terrain != null)
        {
            terrainData = terrain.terrainData;
            SetupTerrainTextures();
        }
        else
        {
            Debug.LogError("Ingen terrain hittades! Lägg till en terrain till scenen.");
        }
    }

    void SetupTerrainTextures()
    {
        // Skapa TerrainLayer array för alla texturer
        TerrainLayer[] terrainLayers = new TerrainLayer[4];

        // Konfigurera grass layer
        terrainLayers[0] = new TerrainLayer
        {
            diffuseTexture = grassTexture,
            normalMapTexture = grassNormal,
            tileSize = new Vector2(tileSize, tileSize),
            diffuseRemapMin = Vector4.zero,
            diffuseRemapMax = Vector4.one
        };

        // Konfigurera dirt layer
        terrainLayers[1] = new TerrainLayer
        {
            diffuseTexture = dirtTexture,
            normalMapTexture = dirtNormal,
            tileSize = new Vector2(tileSize, tileSize),
            diffuseRemapMin = Vector4.zero,
            diffuseRemapMax = Vector4.one
        };

        // Konfigurera sand layer
        terrainLayers[2] = new TerrainLayer
        {
            diffuseTexture = sandTexture,
            normalMapTexture = sandNormal,
            tileSize = new Vector2(tileSize, tileSize),
            diffuseRemapMin = Vector4.zero,
            diffuseRemapMax = Vector4.one
        };

        // Konfigurera rock layer
        terrainLayers[3] = new TerrainLayer
        {
            diffuseTexture = rockTexture,
            normalMapTexture = rockNormal,
            tileSize = new Vector2(tileSize, tileSize),
            diffuseRemapMin = Vector4.zero,
            diffuseRemapMax = Vector4.one
        };

        // Applicera layers till terrängen
        terrainData.terrainLayers = terrainLayers;
    }

    public void UpdateTerrainTexture(int layerIndex, float[,,] alphaMap)
    {
        if (terrainData != null)
        {
            terrainData.SetAlphamaps(0, 0, alphaMap);
        }
    }

    // Hjälpmetod för att sätta grundtextur (gräs)
    public void SetBaseGrassTexture()
    {
        int alphamapWidth = terrainData.alphamapWidth;
        int alphamapHeight = terrainData.alphamapHeight;
        int layerCount = terrainData.alphamapLayers;

        float[,,] alphaMap = new float[alphamapWidth, alphamapHeight, layerCount];

        // Sätt hela terrängen till gräs (layer 0)
        for (int x = 0; x < alphamapWidth; x++)
        {
            for (int y = 0; y < alphamapHeight; y++)
            {
                alphaMap[x, y, 0] = 1f; // Sätt första lagret (gräs) till 100%
                for (int i = 1; i < layerCount; i++)
                {
                    alphaMap[x, y, i] = 0f; // Sätt alla andra lager till 0%
                }
            }
        }

        UpdateTerrainTexture(0, alphaMap);
    }
}