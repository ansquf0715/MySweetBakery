using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oven : MonoBehaviour
{
    public GameObject breadPrefab;
    public Transform basket;
    Collider basketCol;

    [SerializeField]
    int maxBreadCount = 10;
    int currentBreadCount = 0;
    float bakeInterval = 1.0f;
    float moveDuration = 1.0f;

    float moveSpeed = 2f;

    Vector3 breadSpawnPos = new Vector3(4.7f, 1.8f, -4f);

    // Start is called before the first frame update
    void Start()
    {
        basket = transform.Find("Basket");
        basketCol = basket.GetComponent<Collider>();
        //InvokeRepeating(nameof(Bake), bakeInterval, bakeInterval);

        EventManager.OnRequestBread += Bake;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Bake()
    {
        //if(currentBreadCount >= maxBreadCount)
        //{
        //    return;
        //}
        Debug.Log("Bake func");

        GameObject bread = Instantiate(breadPrefab, breadSpawnPos, Quaternion.Euler(0f, 90f, 0f));
        currentBreadCount++;
        StartCoroutine(MoveBreadToBasket(bread));
        EventManager.BreadBaked(bread);
    }

    IEnumerator MoveBreadToBasket(GameObject bread)
    {
        float targetX = basket.position.x;
        Vector3 currentPos = bread.transform.position;

        Rigidbody breadRB = bread.GetComponent<Rigidbody>();
        breadRB.isKinematic = true;

        while (Mathf.Abs(bread.transform.position.x - targetX) > 0.1f)
        {
            float newX = Mathf.MoveTowards(bread.transform.position.x, targetX, moveSpeed * Time.deltaTime);
            bread.transform.position = new Vector3(newX, currentPos.y, currentPos.z);
            yield return null;
        }

        breadRB.isKinematic = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("플레이어가 빵을 가지러 왔다!");
        }
    }
}
