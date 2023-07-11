using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private PlayerController playerController = null;
    [SerializeField] private GameObject playerCorpse = null;
    [SerializeField] private int currHP = 0, maxHP = 100;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        currHP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
