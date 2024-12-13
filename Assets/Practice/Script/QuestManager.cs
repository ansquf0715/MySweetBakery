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

    public Quest(int id, string name, float requiredMoney, Vector3 cameraPos)
    {
        questID = id;
        questName = name;
        this.requiredMoney = requiredMoney;
        isAvailable = false;
        isCompleted = false;
        this.cameraPos = cameraPos;
    }
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    List<Quest> quests = new List<Quest>();
    //List<GameObject> questObjects = new List<GameObject>();
    List<Quest> availableQuests = new List<Quest>();

    public GameObject quest1Obj;

    private void Awake()
    {
        EventManager.OnFirstQuestIsReady += HandleFirstQuestReady;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
            Instance = this;

        quests.Add(new Quest(1, "unlock first place", 35f,
            new Vector3(-7f, 0.5f, 7.5f)));
        quests.Add(new Quest(2, "unlock second place", 100f,
            new Vector3(-0.7f, 0.5f, -9f)));
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
                //EventManager.QuestIsAvailable(quest.cameraPos);
                CameraControl cam = Camera.main.GetComponent<CameraControl>();
                cam.handleQuestAvailable(quest.cameraPos);

                if (quest.questID == 1)
                {
                    Quest1 quest1Script = quest1Obj.GetComponent<Quest1>();
                    quest1Script.enabled = true;
                    quest1Script.SetQuest(quest);
                }
                break;
            }
        }
    }

    public void RemoveQuestFromAvailable(Quest completed)
    {
        Debug.Log("이건 되나");
        if(availableQuests.Contains(completed))
        {
            availableQuests.Remove(completed);
            Debug.Log("다음 거 생성해라");
            CreateQuest();
        }
    }
}
