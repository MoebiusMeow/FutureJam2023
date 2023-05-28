using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FruitPhysics : MonoBehaviour
{
    private int stage = 0;
    public float max_speed = 10.0f;
    public float accleration = 0.2f;
    private float velocity = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (var hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition)))
        {
            var target = hit.collider.gameObject;
            if (target == this)
            {
                Destroy(target);
                return;
            }
        }
        Vector3 Gravity = Physics.gravity;
        Vector3 l = transform.position - Camera.main.transform.position;
        Vector3 r = ray.direction.normalized;
        Vector3 force_dir = (Vector3.Dot(l, r) * r - l);
        var distance = force_dir.magnitude;
        force_dir = force_dir.normalized;
        var rigbody = GetComponent<Rigidbody>();
        if (distance < 5)
        {
            rigbody.AddForce(force_dir * rigbody.mass * Gravity.magnitude / Mathf.Max(0.1f, distance / 10));
        }
        rigbody.AddForce(-rigbody.velocity * rigbody.mass / Mathf.Min(Mathf.Max(0.01f, distance / 5), 0.5f));
        */
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 l = transform.position - Camera.main.transform.position;
        Vector3 r = ray.direction.normalized;
        Vector3 d = (Vector3.Dot(l, r) * r - l);
        if (d.magnitude < 4)
            stage = 1;
        Vector3 target_pos = Camera.main.transform.position + Vector3.Dot(l, r) * r;
        Vector3 distance = target_pos - transform.position;
        if (stage == 1) {
            velocity += Time.deltaTime * accleration;
            velocity = Mathf.Min(velocity, max_speed);
            transform.position = transform.position + distance.normalized * velocity * Time.deltaTime;
        }
    }
}
