using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int CurrentScore;
    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI highscore;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        this.RegisterListener(EventID.GameOver, (sender, param) =>{
            GameLose();
        });
        highscore.text = DataAccountPlayer.PlayerInformation.score.ToString();

    }
    private void Start()
    {
        CurrentScore = 0;
    }
    void Update()
    {
        scoreTxt.text = "score : " + CurrentScore.ToString();
    }
    public void addScore(int score)
    {
        CurrentScore += score;
    }
    private void GameLose()
    {
        DataAccountPlayer.PlayerInformation.SaveScore(CurrentScore);
    }
}