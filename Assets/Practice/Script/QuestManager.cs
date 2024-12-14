using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public int questID;
    public string questName;

    public float requiredMoney;
    public bool isAvailable;
    public bool isCompleted;

    public Vector3 cameraPos;
    public GameObject questPrefab;
    public Vector3 questPos;

    public Quest(int id, string name, float requiredMoney, Vector3 cameraPos,
        GameObject questPrefab = null, Vector3 questPos = default)
    {
        questID = id;
        questName = name;
        this.requiredMoney = requiredMoney;
        isAvailable = false;
        isCompleted = false;
        this.cameraPos = cameraPos;
        this.questPrefab = questPrefab;
        this.questPos = questPos;
    }
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    List<Quest> quests = new List<Quest>();
    //List<GameObject> questObjects = new List<GameObject>();
    List<Quest> availableQuests = new List<Quest>();

    public GameObject quest1Obj;
    public GameObject quest2ObjPrefab;

    private void Awake()
    {
        quests.Add(new Quest(1, "unlock first place", 35f,
            new Vector3(-7f, 0.5f, 7.5f)));
        quests.Add(new Quest(2, "unlock second place", 100f,
            new Vector3(-0.7f, 0.5f, -9f), quest2ObjPrefab,
            new Vector3(-0.08f, 0.6f, -8.56f)));
        EventManager.OnFirstQuestIsReady += HandleFirstQuestReady;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
            Instance = this;

        //quests.Add(new Quest(1, "unlock first place", 35f,
        //    new Vector3(-7f, 0.5f, 7.5f)));
        //quests.Add(new Quest(2, "unlock second place", 100f,
        //    new Vector3(-0.7f, 0.5f, -9f)));

        //EventManager.OnFirstQuestIsReady += HandleFirstQuestReady;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void HandleFirstQuestReady()
    {
        CreateQuest();
    }

    void CreateQuest()
    {
        foreach(Quest quest in quests)
        {
            if(!quest.isAvailable && !availableQuests.Contains(quest)
                && !quest.isCompleted)
            {
                quest.isAvailable = true;
                availableQuests.Add(quest);

                if(quest.questPrefab != null)
                {
                    GameObject QuestObj = Instantiate(quest.questPrefab,
                        quest.questPos, Quaternion.identity);

                    if(quest.questID == 2)
                    {
                        QuestObj.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    }
                }
                else
                {
                    if(quest.questID == 1)
                    {
                        Quest1 script = quest1Obj.GetComponent<Quest1>();
                        script.enabled = true;
                        script.SetQuest(quest);
                    }
                }

                CameraControl cam = Camera.main.GetComponent<CameraControl>();
                cam.handleQuestAvailable(quest.cameraPos);

                break;
            }
        }
    }

    public void RemoveQuestFromAvailable(Quest completed)
    {
        if(availableQuests.Contains(completed))
        {
            availableQuests.Remove(completed);
            CreateQuest();
        }
    }
}
