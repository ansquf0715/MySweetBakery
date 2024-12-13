using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Quest1 : MonoBehaviour
{
    Quest quest;
    MoneyManager moneyManager;

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject plantPrefab;
    public GameObject chairPrefab;
    public GameObject deskPrefab;
    public ParticleSystem particle;

    public List<GameObject> currentWalls = new List<GameObject>();
    public GameObject currentFloor;
    List<GameObject> plants = new List<GameObject>();
    Transform wallSpawnPos;

    // Start is called before the first frame update
    void Start()
    {
        moneyManager = FindObjectOfType<MoneyManager>();

        wallSpawnPos = transform.Find("newWallSpawnPos");
        Debug.Log("wall spawn pos" + wallSpawnPos.position);

        for(int i=0; i<4; i++)
        {
            currentWalls.Add(transform.Find("Wall" + (i + 1)).gameObject);
        }
        currentFloor = transform.Find("Quest1Floor").gameObject;

        //particle.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetQuest(Quest q)
    {
        quest = q;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(quest.requiredMoney <= moneyManager.getMoney())
            {
                ChangeObjects();
            }
        }
    }

    void ChangeObjects()
    {
        GameObject p = Instantiate(particle.gameObject, new Vector3(-6.62f, 0.83f, 7.69f), Quaternion.identity);
        ParticleSystem particleSystem = p.GetComponent<ParticleSystem>();
        particleSystem.Play();

        foreach (var wall in currentWalls)
        {
            Destroy(wall.gameObject);
        }
        currentWalls.Clear();

        Vector3 spawnPos = wallSpawnPos.position;

        for(int i=0; i<3; i++)
        {
            Vector3 wallPos = new Vector3(spawnPos.x,
                0.5f, spawnPos.z + i * 1.7f);
            GameObject newWall = Instantiate(wallPrefab,
                wallPos, Quaternion.Euler(0, 90f, 0));
            currentWalls.Add(newWall);
        }

        Vector3 newFloorPos = currentFloor.transform.position;
        Destroy(currentFloor);
        currentFloor = Instantiate(floorPrefab,
                    newFloorPos, Quaternion.identity);

        Vector3 plant1Pos = new Vector3(-9f, 0.5f, 9.7f);
        GameObject plant1 = Instantiate(plantPrefab,
            plant1Pos, Quaternion.identity);
        plants.Add(plant1);

        Vector3 plant2Pos = new Vector3(-5.06f, 0.5f, 9.7f);
        GameObject plant2 = Instantiate(plantPrefab,
            plant2Pos, Quaternion.identity);
        plants.Add(plant2);

        Vector3 chairPos = new Vector3(-7.4f, 0.5f, 7.7f);
        GameObject chair = Instantiate(chairPrefab,
            chairPos, Quaternion.Euler(0f, 90f, 0f));

        Vector3 deskPos = new Vector3(-5.7f, 0.5f, 7.7f);
        GameObject desk = Instantiate(deskPrefab,
            deskPos, Quaternion.identity);

        StartCoroutine(delay());
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);
        CompleteQuest();
    }

    void CompleteQuest()
    {
        quest.isCompleted = true;
        quest.isAvailable = false;
        QuestManager.Instance.RemoveQuestFromAvailable(quest);
    }
}
