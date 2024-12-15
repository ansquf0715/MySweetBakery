using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SeatMoney : MonoBehaviour
{
    public MoneyManager moneyManager;
    public GameObject cashPrefab;
    public int billForBread = 5;
    float spacing = 0.5f;
    int rows = 3;
    int columns = 4;

    List<KeyValuePair<GameObject, bool>> cashesFromSeat = new List<KeyValuePair<GameObject, bool>>();
    // Start is called before the first frame update
    void Start()
    {
        EventManager.OnSeatPay += changeToMoney;
        moneyManager = FindObjectOfType<MoneyManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(RemoveCash());
        }
    }

    void changeToMoney(int count)
    {
        int newMoney = count * billForBread;
        StartCoroutine(delaySpawnCash(newMoney));
    }

    IEnumerator delaySpawnCash(int newMoney)
    {
        for (int i = 0; i < newMoney; i++)
        {
            SpawnCash(cashesFromSeat.Count);
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator RemoveCash()
    {
        while (true) // 조건은 적절히 조정
        {
            for (int i = cashesFromSeat.Count - 1; i >= 0; i--)
            {
                var cashPair = cashesFromSeat[i];
                if (cashPair.Value)
                {
                    GameObject cash = cashPair.Key;
                    cashesFromSeat.RemoveAt(i);
                    Destroy(cash);

                    moneyManager.ChangeMoney(1);
                    break;
                }
            }
            if (cashesFromSeat.Count == 0)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void SpawnCash(int index)
    {
        int itemsPerFloor = rows * columns;

        int floor = index / itemsPerFloor;
        int localIndex = index % itemsPerFloor;

        int row = localIndex / 4;
        int col = localIndex % 4;

        Vector3 targetPos = new Vector3(-6f, 0.7f, 6.25f);
        Vector3 spawnPos = new Vector3(
            targetPos.x - col * spacing + 1f,
            targetPos.y + floor * 0.1f,
            targetPos.z - row * 0.9f);

        Vector3 firstPos = new Vector3(-5.4f, 1.5f, 7.7f);
        GameObject cash = Instantiate(cashPrefab,
            firstPos, Quaternion.identity);
        cashesFromSeat.Add(new KeyValuePair<GameObject, bool>(cash, false));

        StartCoroutine(MoveToPosition(cash, spawnPos));
    }

    IEnumerator MoveToPosition(GameObject cash, Vector3 targetPos)
    {
        //Vector3 startPos = new Vector3(0.1f, 1.4f, 1.2f);
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
        for (int i = 0; i < cashesFromSeat.Count; i++)
        {
            if (cashesFromSeat[i].Key == cash)
            {
                cashesFromSeat[i] = new KeyValuePair<GameObject, bool>(cash, true);
                break;
            }
        }
    }
}
