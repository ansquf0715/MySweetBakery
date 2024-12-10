using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenBasket : MonoBehaviour
{
    public List<GameObject> breads = new List<GameObject>();

    public float maxStackHeight = 5f; // �ٱ��Ͽ� ���� �� �ִ� �ִ� ����
    public float spreadForce = 2f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Bread"))
        {
            if(!breads.Contains(other.gameObject))
            {
                breads.Add(other.gameObject);
            }
        }
    }

}
