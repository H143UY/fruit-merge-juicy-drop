using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataAccountPlayer : MonoBehaviour
{
    private static PlayerInformation _playerInformation;
    public static PlayerInformation PlayerInformation
    {
        get
        {
            if (_playerInformation != null)
            {
                return _playerInformation;
            }
            var playerinformation = new PlayerInformation();
            _playerInformation = ES3.Load(DataAccountPlayerConstains.PlayerInformation, playerinformation);
            return _playerInformation;
        }
    }
    public static void SavePlayerInfomation()
    {
        ES3.Save(DataAccountPlayerConstains.PlayerInformation, _playerInformation);
    }

}