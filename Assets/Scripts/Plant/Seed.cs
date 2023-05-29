using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
    public int plantId;
    public GameObject SparkingEffect;
    public GameObject SmokingEffect;

    private int collecting = 0;
    public int fruit_cnt = 0;
    public int seed_cnt = 1;
    public Sidebar sidebar = null;
    public  Infobar infobar = null;
    

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(SparkingEffect, transform);
        Instantiate(SmokingEffect, transform);
    }

    public void SetSideBar(GameObject _sidebar)
    {
        sidebar = _sidebar.GetComponent<Sidebar>();
    }

    public void SetInfoBar(GameObject _infobar)
    {
        infobar = _infobar.GetComponent<Infobar>();
        if(infobar == null)
        {
            Debug.Log("InfoBar is Null!!!!!!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 l = transform.position - ray.origin;
        Vector3 r = ray.direction.normalized;
        Vector3 d = (Vector3.Dot(l, r) * r - l);
        if(collecting == 0 && d.magnitude < 0.3)
        {
            Instantiate(SparkingEffect, transform);
            Instantiate(SmokingEffect, transform);
            collecting = 1;
            var mesh = GetComponent<MeshRenderer>();
            mesh.enabled = false;
            Destroy(gameObject, 3.0f);
            if (sidebar)
            {
                // sidebar.SetSeedNumber(plantId + 1, sidebar.GetSeedNumber(plantId + 1) + seed_cnt);
                sidebar.AddSeedNumber(plantId + 1, seed_cnt);
                infobar.fruit_cnt += fruit_cnt;
            }
               
        }
    }
}
