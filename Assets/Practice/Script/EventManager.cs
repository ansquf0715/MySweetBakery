using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action OnRequestBake; //�� ������ �̺�Ʈ
    public static event Action<GameObject> OnBreadBaked; //���� �������� �� ������ �̺�Ʈ

    public static event Action<bool> OnPlayerNearOven; //player�� oven ��ó��� �̺�Ʈ
    public static event Action OnPlayerBreadRequest; //player�� ���� �޶�� ��û�ϴ� �̺�Ʈ
    public static event Action<GameObject> OnPlayerReceiveBreads; //player�� ���� �޴� �̺�Ʈ

    public static event Action OnSellBoxRequestBread; //sellbox�� player���� �� �޶�� ��û
    public static event Action<GameObject> OnPlayerGiveBreadToSellBox; //player�� sellbox�� ���� ��

    public static event Action<int> OnCustomerRequestToSellBox; //customer�� sellbox���� �� �޶�� ��û
    public static event Action OnSellboxHaveNotEnoughBread; //sellbox�� ����� ���� ���� ��
    public static event Action<List<GameObject>> OnCustomerReceiveBreads; //customer�� sellbox���� ���� ������
    
    public static void RequestBread()
    {
        OnRequestBake?.Invoke();
    }

    public static void BreadBaked(GameObject bread)
    {
        OnBreadBaked?.Invoke(bread);
    }

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

    public static void CustomerRequestToSellBox(int count)
    {
        OnCustomerRequestToSellBox?.Invoke(count);
    }

    public static void SellboxHaveNotEnoughBread()
    {
        OnSellboxHaveNotEnoughBread?.Invoke();
    }

    public static void CustomerReceiveBreads(List<GameObject> breads)
    {
        OnCustomerReceiveBreads?.Invoke(breads);
    }
}
