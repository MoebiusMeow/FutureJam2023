using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/*
 * public void SetFruit(int a, int b);
 * ����ˮ������Ϊ a/b
 * 
 * 
 * public void SetDay(int d);
 * ��������Ϊ d
 * 
 * 
 * public int GetSpeed();
 * ��ȡ��ǰ�����ٶ�
 * 
 * public void SetSpeed();
 * ���õ�ǰ�����ٶ�
 * 
 */

public class Infobar : MonoBehaviour
{
    public GameObject fruit, day, pause, run, fast;
    int spd = 0;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        pause.GetComponent<Button>().interactable = true;
        run.GetComponent<Button>().interactable = true;
        fast.GetComponent<Button>().interactable = true;
        if (spd == 0) pause.GetComponent<Button>().interactable = false;
        if (spd == 1) run.GetComponent<Button>().interactable = false;
        if (spd == 2) fast.GetComponent<Button>().interactable = false;
    }

    // interface starts
    public void SetFruit(int a, int b)
    {
        fruit.GetComponent<TMP_Text>().text = $"{a}/{b}";
    }

    public void SetDay(int d)
    {
        day.GetComponent<TMP_Text>().text = $"Day {d}";
    }

    public int GetSpeed()
    {
        return spd;
    }

    public void SetSpeed(int s)
    {
        spd = s;
    }
    // interface ends

    public void OnPauseClick() { spd = 0; }
    public void OnRunClick() { spd = 1; }
    public void OnFastClick() { spd = 2; }
}
