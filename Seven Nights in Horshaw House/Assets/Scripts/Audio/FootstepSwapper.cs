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
                string newLayer = terrainChecker.GetLayerName(transform.position, terrain);

                // Check if the new layer is different from the current one
                if (currLayer != newLayer)
                {
                    currLayer = newLayer;
                    foreach (FootstepCollection collection in terrainFootstepCollections)
                    {
                        if (currLayer == collection.name) // Name of the asset file
                        {
                            playerController.SwapFootsteps(collection);
                        }
                    }
                }
            }
            else if (hit.transform.GetComponent<SurfaceType>() != null)
            {
                FootstepCollection collection = hit.transform.GetComponent<SurfaceType>().footstepCollection;
                string newLayer = collection.name;

                // Check if the new layer is different from the current one
                if (currLayer != newLayer)
                {
                    currLayer = newLayer;
                    playerController.SwapFootsteps(collection);
                }
            }
        }
    }
}
