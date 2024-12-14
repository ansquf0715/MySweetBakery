using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action<bool> OnPlayerNearOven; //player�� oven ��ó��� �̺�Ʈ
    public static event Action OnPlayerBreadRequest; //player�� ���� �޶�� ��û�ϴ� �̺�Ʈ
    public static event Action<GameObject> OnPlayerReceiveBreads; //player�� ���� �޴� �̺�Ʈ

    public static event Action OnSellBoxRequestBread; //sellbox�� player���� �� �޶�� ��û
    public static event Action<GameObject> OnPlayerGiveBreadToSellBox; //player�� sellbox�� ���� ��

    public static event Action<Customer> OnCustomerAtCounter; // customerManager�� ���� customer�� �ִٰ� ����
    //public static event Action<GameObject> OnBagReady; //����� �Ϸ�� ��
    public static event Action<Customer, int> OnCustomerPay; //���� ���� �����ߴٴ� �̺�Ʈ

    public static event Action OnFirstQuestIsReady; //ù��° quest�� ������ �غ� ��

    public static event Action<GameObject> OnNewSeatAvailable; //seat�� �������� ��

    public static event Action<Seat> OnSeatDirty; //seat�� �����ٴ� �̺�Ʈ
    public static event Action<Seat> OnSeatCleaned; //seat�� û�ҵƴٴ� �̺�Ʈ

    public static Action<int> OnArrowAction; //ȭ��ǥ ��ġ�� �����ϱ� ����

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

    public static void FirstQuestIsReady()
    {
        OnFirstQuestIsReady?.Invoke();
    }

    public static void NewSeatAvailable(GameObject chair)
    {
        OnNewSeatAvailable?.Invoke(chair);
    }

    public static void SeatDirty(Seat seat)
    {
        OnSeatDirty?.Invoke(seat);
    }

    public static void SeatCleaned(Seat seat)
    {
        OnSeatCleaned?.Invoke(seat);
    }

    public static void ArrowAction(int arrowId)
    {
        //Debug.Log("arrowId" + arrowId);
        OnArrowAction?.Invoke(arrowId);
    }
}
