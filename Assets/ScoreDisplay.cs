using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Mechanics;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    Text scoreText;
    PlayerController player;
    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        scoreText = GetComponent<Text>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = gameManager.score.ToString();
    }
}
