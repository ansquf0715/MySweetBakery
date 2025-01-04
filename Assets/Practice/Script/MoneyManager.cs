using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    int billForBread = 5;

    public GameObject cashPrefab;
    public int money = 0;

    List<KeyValuePair<GameObject, bool>> cashes = new List<KeyValuePair<GameObject, bool>>();

    public Transform moneySpawnPos;
    float spacing = 0.5f;
    int rows = 3;
    int columns = 4;

    public TextMeshProUGUI moneyText;

    bool playerIsGettingCash = false;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.OnCustomerPay += changeBreadToMoney;

        UpdateMoneyUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EventManager.OnArrowAction(4);

            playerIsGettingCash = true;
            StartCoroutine(RemoveCash());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerIsGettingCash = false;
        }
    }

    public bool PayMoney(int amount = 1)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateMoneyUI();
            return true;
        }
        return false;
    }

    IEnumerator RemoveCash()
    {
        while (playerIsGettingCash)
        {
            for (int i = cashes.Count - 1; i >= 0; i--)
            {
                var cashPair = cashes[i];
                if (cashPair.Value)
                {
                    GameObject cash = cashPair.Key;
                    cashes.RemoveAt(i);
                    Destroy(cash);

                    money++;
                    UpdateMoneyUI();
                    break;
                }
            }

            if (cashes.Count == 0)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void changeBreadToMoney(Customer customer, int count)
    {
        int newMoney = count * billForBread;
        StartCoroutine(delaySpawnCash(newMoney));
    }

    IEnumerator delaySpawnCash(int newMoney)
    {
        for (int i = 0; i < newMoney; i++)
        {
            SpawnCash(cashes.Count);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void SpawnCash(int index)
    {
        int itemsPerFloor = rows * columns;

        int floor = index / itemsPerFloor;
        int localIndex = index % itemsPerFloor;

        int row = localIndex / 4;
        int col = localIndex % 4;

        Vector3 targetPos = moneySpawnPos.position;
        Vector3 spawnPos = new Vector3(
            targetPos.x - col * spacing + 1f,
            targetPos.y + floor * 0.1f,
            targetPos.z - row * 0.9f);

        Vector3 firstPos = new Vector3(0.1f, 1.2f, 1.4f);
        GameObject cash = Instantiate(cashPrefab,
            firstPos, Quaternion.identity);
        cashes.Add(new KeyValuePair<GameObject, bool>(cash, false));

        StartCoroutine(MoveToPosition(cash, spawnPos));
    }

    IEnumerator MoveToPosition(GameObject cash, Vector3 targetPos)
    {
        Vector3 startPos = cash.transform.position;
        float elapsedTime = 0f;
        float duration = 0.5f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cash.transform.position = Vector3.Lerp(startPos,
                targetPos, (elapsedTime / duration));
            yield return null;
        }
        cash.transform.position = targetPos;
        for (int i = 0; i < cashes.Count; i++)
        {
            if (cashes[i].Key == cash)
            {
                cashes[i] = new KeyValuePair<GameObject, bool>(cash, true);
                break;
            }
        }
    }

    void UpdateMoneyUI()
    {
        moneyText.text = money.ToString();
    }

    public int getMoney()
    {
        return money;
    }

    public void ChangeMoney(int count)
    {
        money++;
        UpdateMoneyUI();
    }

}