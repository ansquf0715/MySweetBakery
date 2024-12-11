using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oven : MonoBehaviour
{
    public GameObject breadPrefab;
    public Transform basket;
    Collider basketCol;

    public ParticleSystem bakeParticlePrefab;
    //ParticleSystem bakeParticle;
    ParticleSystem bakeParticle;

    [SerializeField]
    int maxBreadCount = 10;
    int currentBreadCount = 0;
    float bakeInterval = 1.0f;
    float moveDuration = 1.0f;

    float moveSpeed = 2f;

    Vector3 breadSpawnPos = new Vector3(4f, 1.8f, -5f);

    // Start is called before the first frame update
    void Start()
    {
        basket = transform.Find("Basket");
        basketCol = basket.GetComponent<Collider>();

        bakeParticle = transform.Find("BakeParticle").GetComponent<ParticleSystem>();
        bakeParticle.Pause();

        EventManager.OnRequestBake += Bake;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Bake()
    {
        bakeParticle = Instantiate(bakeParticlePrefab, new Vector3(5f, 2f, -5f), Quaternion.identity);
        bakeParticle.time = 5f;
        bakeParticle.Play();

        GameObject bread = Instantiate(breadPrefab, breadSpawnPos, Quaternion.Euler(0f, 90f, 0f));
        currentBreadCount++;

        StartCoroutine(BakeNotify(bread));
        StartCoroutine(MoveBreadToBasket(bread));
    }

    IEnumerator BakeNotify(GameObject bread)
    {
        yield return new WaitForSeconds(1f);
        EventManager.BreadBaked(bread);
        Destroy(bakeParticle.gameObject);
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

    bool isBreadStable(GameObject bread)
    {
        Rigidbody breadRB = bread.GetComponent<Rigidbody>();
        return breadRB.velocity.magnitude < 0.01f
            && breadRB.angularVelocity.magnitude < 0.01f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            EventManager.SetPlayerNearOven(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            EventManager.SetPlayerNearOven(false);
        }
    }
}
