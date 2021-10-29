using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Mechanics;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    Text scoreText;
    PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        scoreText = GetComponent<Text>();
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = player.score.ToString();
    }
}
