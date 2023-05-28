using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    //delay for start spawn time
    private float spawnDelay = 4;
    //variables for clamping the spawn delay
    private float minSpawnDelay = 0.5f;
    private float maxSpawnDelay = 4;
    //index of subtraction of the spawnDelay every spawn time
    private float spawnRate = 0.075f;
    //how many zombies we want to reserve for optimisation
    private int reservedZombiesAmount = 100;
    //random zombie which will be spawned
    private int randomZombie;
    //random spawn position which zombie will be spawned in
    private int randomPosition;
    //list which contains initial spawned zombied
    [SerializeField] private List<GameObject> spawnedZombies = new();
    //list with prefabs of zombies for difference of enemies
    [SerializeField] private GameObject[] zombies;
    //list of all possible positions for different zombie spawn location
    [SerializeField] private Transform[] spawnPositions;

    private void Start()
    {
        //make a reserve of zombies
        ReservedSpawner();

        //every spawnDelay time we whether turn on a zombie from reserve or spawn a new one if reserve ends
        InvokeRepeating("ObjectSpawn", 0, spawnDelay);
    }

    private void ReservedSpawner()
    {
        //making reserve
        for (int i = 0; i < reservedZombiesAmount; i++)
        {
            Instantiating(false);
        }
    }

    private void ObjectSpawn()
    {
        //if reserve exists we turn on a zombie and remove in from the list
        if (spawnedZombies.Count > 0)
        {
            spawnedZombies[0].SetActive(true);
            spawnedZombies.Remove(spawnedZombies[0]);
        }

        //if reserve is done, we spawn a new one
        else Instantiating(true);

        //subtract spawnDelay to make next spawn faster for hardness
        spawnDelay -= spawnRate;

        //clamping spawnDelay
        spawnDelay = Mathf.Clamp(spawnDelay, minSpawnDelay, maxSpawnDelay);
    }

    private void Instantiating(bool isReserveEnded)
    {
        //if we male reserve, we spawn and add objects to the list
        if (!isReserveEnded)
        {
            randomZombie = Random.Range(0, zombies.Length - 1);
            randomPosition = Random.Range(0, spawnPositions.Length - 1);
            GameObject spawnedZombie = Instantiate(zombies[randomZombie], spawnPositions[randomPosition].position, Quaternion.identity);
            spawnedZombie.SetActive(false);
            spawnedZombies.Add(spawnedZombie);
        }

        //if reserve is done, we just spawn objects
        else
        {
            randomZombie = Random.Range(0, zombies.Length - 1);
            randomPosition = Random.Range(0, spawnPositions.Length - 1);
            GameObject spawnedZombie = Instantiate(zombies[randomZombie], spawnPositions[randomPosition].position, Quaternion.identity);
        }
    }
}
