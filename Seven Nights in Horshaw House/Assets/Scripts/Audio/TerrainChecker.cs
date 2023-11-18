using UnityEngine;

// https://www.youtube.com/watch?v=wXcjxeetg70&t=364s&ab_channel=NattyGameDev
public class TerrainChecker 
{
    private float[] GetTextureMix(Vector3 playerPos, Terrain terrain)
    {
        Vector3 terrainPos = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;
        // Player's X and Z position relative to the terrain
        int mapX = Mathf.RoundToInt((playerPos.x - terrainPos.x) / terrainData.size.x * terrainData.alphamapWidth);
        int mapZ = Mathf.RoundToInt((playerPos.z - terrainPos.z) / terrainData.size.z * terrainData.alphamapHeight);
        float[,,] splatMapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float[] cellmix = new float[splatMapData.GetUpperBound(2) + 1];
        for (int i = 0; i < cellmix.Length; i++)
        {
            cellmix[i] = splatMapData[0, 0, i];
        }
        return cellmix;
    }

    public string GetLayerName(Vector3 playerPos, Terrain terrain)
    {
        float[] cellmix = GetTextureMix(playerPos, terrain);
        float strongest = 0;
        int maxIndex = 0;
        for (int i = 0; i < cellmix.Length; i++)
        {
            if (cellmix[i] > strongest)
            {
                maxIndex = i;
                strongest = cellmix[i];
            }
        }
        return terrain.terrainData.terrainLayers[maxIndex].name;
    }
}