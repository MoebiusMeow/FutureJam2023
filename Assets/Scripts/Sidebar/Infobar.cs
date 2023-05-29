using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/*
 * public void SetFruit(int a, int b);
 * 设置水果数量为 a/b
 * 
 * 
 * public void SetDay(int d);
 * 设置天数为 d
 * 
 * 
 * public int GetSpeed();
 * 获取当前运行速度
 * 
 * public void SetSpeed(int speed);
 * 设置当前运行速度
 * 
 * public void SetPause(bool pause);
 * 设置当前运行速度
 * 
 */

public class Infobar : MonoBehaviour
{
    public GameObject fruit, day, pause, run, fast;
    public Sprite sprite1x, sprite2x;
    int spd = 1;
    bool isPause = true;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        pause.GetComponent<Button>().interactable = true;
        run.GetComponent<Button>().interactable = true;
        if (isPause) pause.GetComponent<Button>().interactable = false;
        else run.GetComponent<Button>().interactable = false;
        if (spd == 1) fast.GetComponent<Image>().sprite = sprite1x;
        else fast.GetComponent<Image>().sprite = sprite2x;
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
        if (isPause) return 0;
        return spd;
    }
    public void SetPause(bool pause)
    {
        isPause = pause;
    }
    public void SetSpeed(int speed)
    {
        spd = speed;
    }
    // interface ends

    public void OnPauseClick() { isPause = true; }
    public void OnRunClick() { isPause = false; }
    public void OnFastClick() { spd = 3-spd; }
}
