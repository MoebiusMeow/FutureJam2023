using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/*
 * 
 *  public void Popup(string title, string intro, string confirm);
 *  public void ShowLog(string text);
 *  public bool EventAllClear(); // 返回是否所有事件已经确认
 *
 */


public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }
    public GameObject eventPrefab, logPrefab;
    public GameObject infobar;
    public GameObject logs;
    public int eventCount, logCount;

    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool EventAllClear()
    {
        return eventCount <= 0;
    }

    public void Popup(string title, string intro, string confirm)
    {
        GameObject ev = Instantiate(eventPrefab);
        ev.transform.SetParent(transform);
        ev.GetComponent<Event>().parent = this;
        ev.GetComponent<Event>().Popup(title, intro, confirm);
        infobar.GetComponent<Infobar>().SetSpeed(0);
        eventCount += 1;
    }

    public void ShowLog(string text)
    {
        GameObject lg = Instantiate(logPrefab);
        lg.transform.SetParent(logs.transform);
        lg.GetComponent<Log>().parent = this;
        lg.GetComponent<Log>().Popup(text);
        logCount += 1;
    }
}
