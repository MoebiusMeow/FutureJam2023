using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/*
 * 
 *  public void Popup(string title, string intro, string confirm);
 *  public bool EventAllClear(); // 返回是否所有事件已经确认
 *
 */


public class UI : MonoBehaviour
{
    public GameObject eventPrefab;
    public GameObject infobar;
    public int eventCount;

    void Start()
    {

        Popup("1", "1", "1");
        Popup("2", "2", "2");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool EventAllClear()
    {
        return eventCount == 0;
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
}
