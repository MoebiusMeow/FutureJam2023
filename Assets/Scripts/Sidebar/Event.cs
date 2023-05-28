using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Event : MonoBehaviour
{
    public GameObject obj, titleText, introText, buttonText;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.SetAsLastSibling();
    }

    public void OnClick()
    {
        obj.SetActive(false);
    }

    public void Popup(string title, string intro, string confirm)
    {
        titleText.GetComponent<TMP_Text>().text = title;
        introText.GetComponent<TMP_Text>().text = intro;
        buttonText.GetComponent<TMP_Text>().text = confirm;
        obj.SetActive(true);
    }
}
