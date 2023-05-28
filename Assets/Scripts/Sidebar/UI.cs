using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/*
 * 
 *  public void Popup(string title, string intro, string confirm);
 *
 */


public class UI : MonoBehaviour
{
    public GameObject eventPrefab;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Popup(string title, string intro, string confirm)
    {
        GameObject ev = Instantiate(eventPrefab);
        ev.transform.SetParent(transform);
        ev.GetComponent<Event>().Popup(title, intro, confirm);
    }
}
