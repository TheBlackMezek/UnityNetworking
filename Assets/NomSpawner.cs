using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NomSpawner : NetworkBehaviour {

    public float spawnRange = 40.0f;
    public float spawnWait = 5.0f;

    public GameObject nom;

    private float timer = 0;



    private void Update()
    {
        if(!isServer)
        {
            return;
        }

        timer += Time.deltaTime;

        if(timer >= spawnWait)
        {
            timer = 0;

            GameObject n = Instantiate(nom);
            float x = Random.Range(-spawnRange, spawnRange);
            float z = Random.Range(-spawnRange, spawnRange);
            n.transform.position = new Vector3(x, 0, z);

            NetworkServer.Spawn(n);
        }
    }

}
