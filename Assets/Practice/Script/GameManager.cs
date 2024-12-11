using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 5f;

    //spawn À§Ä¡ new Vector3(-12f, 0.5f, 3f);
    float spawnTimer;

    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        //spawnTimer -= Time.deltaTime;

        //if(spawnTimer <= 0f)
        //{
        //    SpawnCustomer();
        //    spawnTimer = spawnInterval;
        //}
    }

    void SpawnCustomer()
    {
        GameObject newCustomer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
    }
}
