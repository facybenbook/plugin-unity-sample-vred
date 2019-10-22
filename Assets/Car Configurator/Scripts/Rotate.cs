using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

    public float speed = 10.0f;
    //public GameObject car;
    //private GameObject carPivot;

    private void Start()
    {

    }

    void FixedUpdate () {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}
