using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action<bool> OnPlayerNearOven; //player가 oven 근처라는 이벤트
    public static event Action OnPlayerBreadRequest; //player가 빵을 달라고 요청하는 이벤트
    public static event Action<GameObject> OnPlayerReceiveBreads; //player가 빵을 받는 이벤트

    public static event Action OnSellBoxRequestBread; //sellbox가 player한테 빵 달라고 요청
    public static event Action<GameObject> OnPlayerGiveBreadToSellBox; //player가 sellbox에 빵을 줌

    //public event Action<Customer, int> OnCustomerRequestToSellBox; //customer가 sellbox에게 빵 달라고 요청
    //public event Action<Customer, List<GameObject>> OnCustomerReceiveBread; //sellbox가 빵 줄 수 있으면 줌
    //public event Action<Customer> OnSellboxHaveNotEnoughBread; //sellbox가 빵 없다고 전달

    public static event Action<Customer> OnCustomerAtCounter; // customerManager가 들어온 customer가 있다고 전달
    //public static event Action<GameObject> OnBagReady; //계산이 완료된 빵
    public static event Action<Customer, int> OnCustomerPay; //고객이 돈을 지불했다는 이벤트

    //public static event Action<Customer> OnNextCustomerArrivedAtCounterPosition;

    public static void SetPlayerNearOven(bool isNear)
    {
        OnPlayerNearOven?.Invoke(isNear);
    }

    public static void PlayerBreadRequest()
    {
        OnPlayerBreadRequest?.Invoke();
    }

    public static void DeliverBreadToPlayer(GameObject bread)
    {
        OnPlayerReceiveBreads?.Invoke(bread);
    }
    
    public static void SellBoxRequestBread()
    {
        OnSellBoxRequestBread?.Invoke();
    }

    public static void PlayerGiveBreadToSellBox(GameObject bread)
    {
        OnPlayerGiveBreadToSellBox?.Invoke(bread);
    }

    public static void CustomerAtCounter(Customer customer)
    {
        OnCustomerAtCounter?.Invoke(customer);
    }

    public static void CustomerPay(Customer customer, int amount)
    {
        OnCustomerPay?.Invoke(customer, amount);
    }

    //public static void NextCustomerArrivedAtCounterPosition(Customer customer)
    //{
    //    OnNextCustomerArrivedAtCounterPosition(customer);
    //}

    //public static void BagReady(GameObject bag)
    //{
    //    Debug.Log("Bag Ready Event Manager");
    //    OnBagReady?.Invoke(bag);
    //}

    //public void CustomerRequestToSellBox(Customer customer, int count)
    //{
    //    OnCustomerRequestToSellBox?.Invoke(customer, count);
    //}

    //public void CustomerReceiveBread(Customer customer, List<GameObject> breads)
    //{
    //    OnCustomerReceiveBread?.Invoke(customer, breads);
    //}

    //public void SellboxHaveNotEnoughBread(Customer customer)
    //{
    //    OnSellboxHaveNotEnoughBread?.Invoke(customer);
    //}
}
