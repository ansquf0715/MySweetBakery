using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;

public class Quest1 : MonoBehaviour
{
    Quest quest;
    MoneyManager moneyManager;

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject plantPrefab;
    public GameObject chairPrefab;
    public GameObject deskPrefab;
    public ParticleSystem openParticle;
    public ParticleSystem cleaningParticle;

    public List<GameObject> currentWalls = new List<GameObject>();
    public GameObject currentFloor;
    List<GameObject> plants = new List<GameObject>();
    Transform wallSpawnPos;

    Seat seat;

    bool alreadyCreated = false;
    bool haveDirtyTable = false;
    bool isCleaning = false;

    Vector3 deskPosition;

    TextMeshProUGUI moneyText;

    bool isPaying = false;

    float initialRequiredMoney;
    float paidAmount = 0;

    private void OnEnable()
    {
        EventManager.OnSeatDirty += HandleSeatDirty;
    }

    private void OnDisable()
    {
        EventManager.OnSeatDirty -= HandleSeatDirty;
    }

    // Start is called before the first frame update
    void Start()
    {
        moneyManager = FindObjectOfType<MoneyManager>();

        wallSpawnPos = transform.Find("newWallSpawnPos");

        for(int i=0; i<4; i++)
        {
            currentWalls.Add(transform.Find("Wall" + (i + 1)).gameObject);
        }
        currentFloor = transform.Find("Quest1Floor").gameObject;

        moneyText = transform.Find("Quest1Floor/Canvas/moneyUI").GetComponent<TextMeshProUGUI>();
        Debug.Log("moneyText" + moneyText.text);
        moneyText.text = 35.ToString();
        initialRequiredMoney = quest.requiredMoney;
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
        if (!enabled) return;

        if(other.gameObject.CompareTag("Player"))
        {
            EventManager.ArrowAction(5);

            if (!alreadyCreated)
            {
                isPaying = true;
                StartCoroutine(PayForQuest());
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(isPaying && !alreadyCreated)
            {
                StartCoroutine(PayForQuest());
            }

            if(!isCleaning && haveDirtyTable && seat.isDirty)
            {
                isCleaning = true;
                StartCoroutine(CleanSeat());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPaying = false;
            StopCoroutine("PayForQuest");
        }
    }

    IEnumerator PayForQuest()
    {
        isPaying = true;

        while (quest.requiredMoney > 0 && !alreadyCreated)
        {
            if (moneyManager.getMoney() > 0)
            {
                bool paymentSuccess = moneyManager.PayMoney(1); 
                if (paymentSuccess)
                {
                    quest.requiredMoney -= 1; 
                    paidAmount++;
                    UpdateMoneyUI();
                    yield return new WaitForSeconds(0.5f); 
                }
                else
                {
                    break;
                }
            }
            else
            {
                break; 
            }
        }

        if(!alreadyCreated)
        {
            float amountPaid = initialRequiredMoney - paidAmount;
            if (amountPaid == 0)
            {
                alreadyCreated = true;
                ChangeObjects();
            }
            isPaying = false;
        }
    }
    void UpdateMoneyUI()
    {
        float amountPaid = initialRequiredMoney - paidAmount;
        moneyText.text = amountPaid.ToString();
    }

    void HandleSeatDirty(Seat seat)
    {
        haveDirtyTable = true;
        this.seat = seat;
    }

    void ChangeObjects()
    {
        EventManager.OnArrowAction(4);


        GameObject p = Instantiate(openParticle.gameObject, new Vector3(-6.62f, 0.83f, 7.69f), Quaternion.identity);
        ParticleSystem particleSystem = p.GetComponent<ParticleSystem>();
        particleSystem.Play();

        foreach (var wall in currentWalls)
        {
            Destroy(wall.gameObject);
        }
        currentWalls.Clear();

        Vector3 spawnPos = new Vector3(-8.2f, 0.5f, 7.5f);
        GameObject newWall = Instantiate(wallPrefab, spawnPos, Quaternion.Euler(0, 90f, 0));
        currentWalls.Add(newWall);

        //for(int i=0; i<3; i++)
        //{
        //    Vector3 wallPos = new Vector3(spawnPos.x,
        //        0.5f, spawnPos.z + i * 1.7f);
        //    GameObject newWall = Instantiate(wallPrefab,
        //        wallPos, Quaternion.Euler(0, 90f, 0));
        //    currentWalls.Add(newWall);
        //}

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
        deskPosition = deskPos;

        EventManager.NewSeatAvailable(chair);

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

    IEnumerator CleanSeat()
    {
        Vector3 pPos = deskPosition;
        pPos.y = pPos.y + 1f;
        ParticleSystem particle = Instantiate(cleaningParticle,
            pPos, Quaternion.identity);
        particle.Play();

        yield return new WaitForSeconds(0.5f);

        if (seat.trash != null)
            Destroy(seat.trash);

        seat.chair.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        seat.isDirty = false;
        seat.trash = null;
        isCleaning = false;

        Destroy(particle.gameObject, 1f);
        EventManager.SeatCleaned(seat);

    }
}
