using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quest2 : MonoBehaviour
{
    Quest quest;
    MoneyManager moneyManager;

    TextMeshProUGUI moneyText;

    bool isPaying = false;
    float initialRequiredMoney;
    public float paidAmount = 0;


    bool alreadyCreated = false;

    private void Awake()
    {
        moneyText = transform.Find("Canvas/moneyText").GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        moneyManager = FindObjectOfType<MoneyManager>();

        //moneyText = transform.Find("Quest2/Canvas/moneyText").GetComponent<TextMeshProUGUI>();
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
            Debug.Log("여기까지 된다");
            if(!alreadyCreated)
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
            if(isPaying)
            {
                StartCoroutine("PayForQuest");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            isPaying = false;
            StopCoroutine(PayForQuest());
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

        float amountPaid = initialRequiredMoney - paidAmount;
        if (amountPaid == 0)
        {
            alreadyCreated = true;
        }
        isPaying = false;
    }
    void UpdateMoneyUI()
    {
        float amountPaid = initialRequiredMoney - paidAmount;
        moneyText.text = amountPaid.ToString();
    }
}
