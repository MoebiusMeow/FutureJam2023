using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Log : MonoBehaviour
{
    public GameObject obj, text;
    public UI parent;
    double showTime;


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > showTime + 10)
        {
            OnFade();
        }
    }

    public void OnFade()
    {
        parent.logCount -= 1;
        obj.SetActive(false);
    }

    public void Popup(string t)
    {
        text.GetComponent<TMP_Text>().text = t;
        obj.SetActive(true);
        showTime = Time.time;
    }
}
