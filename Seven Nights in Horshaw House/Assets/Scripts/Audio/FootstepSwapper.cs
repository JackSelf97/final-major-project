using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSwapper : MonoBehaviour
{
    private TerrainChecker terrainChecker = null;
    private PlayerController playerController = null;
    private string currLayer = string.Empty;
    public FootstepCollection[] terrainFootstepCollections;

    // Start is called before the first frame update
    void Start()
    {
        terrainChecker = new TerrainChecker();
        playerController = GetComponent<PlayerController>();
    }

    public void CheckLayers()
    {
        // Raycast downwards
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 3))
        {
            // Check if terrain exists
            if (hit.transform.GetComponent<Terrain>() != null)
            {
                Terrain terrain = hit.transform.GetComponent<Terrain>();
                if (currLayer != terrainChecker.GetLayerName(transform.position, terrain))
                {
                    currLayer = terrainChecker.GetLayerName(transform.position, terrain);
                    foreach (FootstepCollection collection in terrainFootstepCollections)
                    {
                        if (currLayer == collection.name) // Name of the asset file
                        {
                            playerController.SwapFootsteps(collection);
                        }
                    }
                }
            }
            if (hit.transform.GetComponent<SurfaceType>() != null)
            {
                FootstepCollection collection = hit.transform.GetComponent<SurfaceType>().footstepCollection;
                currLayer = collection.name;
                playerController.SwapFootsteps(collection);
            }
        }
    }
}
