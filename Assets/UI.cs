using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mathematics;
using SparseCollections;
using System;
using System.Linq;
using UnityEngine.UI;

public class UI : MonoBehaviour {
    public GameObject menu;
    public GameObject newGameMenu;
    public Checker checker;
    public Slider difficulty;
    public GameObject hint;

    public void Exit() {
        Application.Quit();
    }

    public void NewGame() {
        menu.SetActive(false);
        newGameMenu.SetActive(true);
    }

    void Start() {
        menu.SetActive(false);
        newGameMenu.SetActive(false);
    }

    public void Continue() {
        menu.SetActive(false);
        newGameMenu.SetActive(false);
    }

    public void StartNewGame() {
        if (difficulty.value == 0) {
            checker.basePenalty = 1.4f;
        }
        if (difficulty.value == 1) {
            checker.basePenalty = 1.2f;
        }
        if (difficulty.value == 2) {
            checker.basePenalty = 1.0f;
        }
        if (difficulty.value == 3) {
            checker.basePenalty = 0.6f;
        }
        if (difficulty.value == 4) {
            checker.basePenalty = 0.5f;
        }
        checker.score = 0;
        checker.NextLevel();
        menu.SetActive(false);
        newGameMenu.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            hint.SetActive(false);
            if (newGameMenu.activeInHierarchy || menu.activeInHierarchy) {
                menu.SetActive(false);
                newGameMenu.SetActive(false);
            } else {
                menu.SetActive(true);
            }
        }
    }
}
