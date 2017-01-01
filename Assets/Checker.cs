using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mathematics;
using SparseCollections;
using System;
using System.Linq;
using UnityEngine.UI;

public class Checker : MonoBehaviour {

    public Follower follower;
    public Transform target;
    public Slider slider;
    public float baseDistance = 8;
    public float speed = 0.1f;
    public int cnt = 4;

    public void Update() {
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.LogFormat("mouse = {0}", mouse);
        float distance = (mouse - target.position.xy()).sqrMagnitude;
        float quality = Mathf.Pow(0.5f, distance / baseDistance);
        slider.value += (baseDistance - distance) * Time.deltaTime * speed;

        if (slider.value == 1) {
            NextLevel();
            slider.value = 0;
        }
    }

    void Start() {
        NextLevel();
    }

    private void NextLevel() {
        follower.RandomTrajectory(cnt);
    }

}
