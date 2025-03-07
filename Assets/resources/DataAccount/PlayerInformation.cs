using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInformation
{
    public int score;
   
    public void SaveScore(int newscore)
    {
        if (newscore > score)
        {
            score = newscore;
            DataAccountPlayer.SavePlayerInfomation();
        }
    }
}