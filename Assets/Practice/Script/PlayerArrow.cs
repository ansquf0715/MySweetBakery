using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow
{
    public int arrowId;
    public Transform pointingObj;
    public float offset;
    public bool isCompleted;


    public Arrow(int arrowId, Transform pointingObj,
        float offset, bool isCompleted = false)
    {
        this.arrowId = arrowId;
        this.pointingObj = pointingObj;
        this.offset = offset;
        this.isCompleted = isCompleted;
    }
}

public class PlayerArrow : MonoBehaviour
{
    public bool arrowIsAvailable = false;

    public GameObject arrowPrefab;

    float offset = 3f;

    List<Arrow> arrowList = new List<Arrow>();
    Arrow nowAvailableArrows;

    GameObject arrowInstance;
    Canvas playerCan;
    RectTransform playerArrowImage;

    // Start is called before the first frame update
    void Start()
    {
        GameObject oven = GameObject.Find("BreadOven");
        Transform basket = oven.transform.Find("Basket");
        arrowList.Add(new Arrow(1, basket, 3f));

        GameObject sellbox = GameObject.Find("SellBasket");
        arrowList.Add(new Arrow(2, sellbox.transform, 2f));

        GameObject counter = GameObject.Find("Counter");
        arrowList.Add(new Arrow(3, counter.transform, offset));

        GameObject manager = GameObject.Find("GameManager");
        Transform moneySpawnPos = manager.transform.Find("MoneySpawnPoint");
        arrowList.Add(new Arrow(4, moneySpawnPos, 2f));

        GameObject quest1obj = GameObject.Find("Quest1");
        Transform quest1floor = quest1obj.transform.Find("Quest1Floor");
        arrowList.Add(new Arrow(5, quest1floor, 2f));

        nowAvailableArrows = arrowList[0];
        arrowIsAvailable = true;
        showArrow();

        EventManager.OnArrowAction += UpdateArrowState;

        playerCan = transform.Find("Canvas").GetComponent<Canvas>();
        playerArrowImage = playerCan.transform.Find("arrow").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        checkArrowAvailable();
    }

    void checkArrowAvailable()
    {
        if(!arrowIsAvailable)
        {
            if(playerCan.gameObject.activeSelf)
            {
                playerCan.gameObject.SetActive(false);
            }
        }
        else
        {
            if(!playerCan.gameObject.activeSelf)
            {
                playerCan.gameObject.SetActive(true);
            }
            UpdateCanvasRotation();
        }
    }

    void UpdateCanvasRotation()
    {
        if (arrowInstance != null)
        {
            Vector3 playerPosition = transform.position; 
            Vector3 arrowPosition = arrowInstance.transform.position; 

            Vector3 toArrow = (arrowPosition - playerPosition).normalized;

            Vector3 playerForward = transform.forward;

            float angle = Vector3.SignedAngle(playerForward, toArrow, Vector3.up);

            playerArrowImage.localEulerAngles = new Vector3(0, 0, -angle);
        }
    }

    void UpdateArrowState(int arrowId)
    {
        if (arrowId == 5)
        {
            StopAllCoroutines();
            arrowIsAvailable = false;
            Destroy(arrowInstance);
        }
        else
        {
            if (nowAvailableArrows.arrowId == arrowId)
            {
                nowAvailableArrows.isCompleted = true;


                if (arrowId < arrowList.Count)
                {
                    StopAllCoroutines();

                    nowAvailableArrows = arrowList[arrowId];
                    Vector3 newPos = nowAvailableArrows.pointingObj.position;
                    newPos.y += nowAvailableArrows.offset;
                    if (arrowId == 4)
                    {
                        newPos.z -= 0.6f;
                    }

                    arrowInstance.transform.position = newPos;
                    //UpdateCanvasRotation();
                    StartCoroutine(AnimateArrowUntilComplete(nowAvailableArrows));
                }

            }
        }
    }

    void showArrow()
    {
        Vector3 spawnPos = nowAvailableArrows.pointingObj.position;
        spawnPos.y += nowAvailableArrows.offset;
        arrowInstance = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0f, 90f, 0f));
        StartCoroutine(AnimateArrowUntilComplete(nowAvailableArrows));
    }

    IEnumerator AnimateArrowUntilComplete(Arrow arrow)
    {
        while (!arrow.isCompleted)
        {
            Vector3 originalPosition = arrowInstance.transform.position;

            Vector3 downPosition = originalPosition;
            downPosition.y -= 0.5f;
            yield return MoveArrow(originalPosition, downPosition, 1);

            yield return MoveArrow(downPosition, originalPosition, 1);
        }

    }

    IEnumerator MoveArrow(Vector3 start, Vector3 end, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            arrowInstance.transform.position = Vector3.Lerp(start, end, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        arrowInstance.transform.position = end;
    }
}
