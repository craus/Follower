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
    public Transform distanceSphere;
    MeshRenderer distanceSphereRenderer;
    MeshRenderer sphereRenderer;
    public Slider slider;
    public Text scoreText;
    public float basePenalty = 8;
    public int cnt = 4;
    public float targetSpeed = 3;
    public float minPeriodsRequired = 2;
    public Color bad;
    public Color normal;
    public Color good;
    public int score;

    public void Update() {
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        distanceSphere.transform.position = mouse;
        float distance = (mouse - target.position.xy()).magnitude;
        distanceSphere.localScale = Vector2.one * distance * 2;
        float penalty = Mathf.Pow(distance, 2);
        float speed = (basePenalty - penalty) / basePenalty;
        distanceSphereRenderer.material.SetColor("_EmissionColor", Color.Lerp(bad, good, (speed + 1) / 2));
        sphereRenderer.material.SetColor("_EmissionColor", Color.Lerp(bad, good, slider.value));
        slider.value += speed * Time.deltaTime / follower.period / minPeriodsRequired;
        scoreText.text = score.ToString();

        if (slider.value == 1) {
            score++;
            cnt = 2 + (int)(Math.Log(score, 2));
            NextLevel();
            slider.value = 0;
        }
    }

    void Start() {
        distanceSphereRenderer = distanceSphere.GetComponentInChildren<MeshRenderer>();
        sphereRenderer = target.GetComponentInChildren<MeshRenderer>();
        NextLevel();
    }

    private void NextLevel() {
        follower.speed = targetSpeed;
        follower.RandomTrajectory(cnt);
    }

}
