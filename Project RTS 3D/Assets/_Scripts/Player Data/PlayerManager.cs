using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [field: SerializeField]
    public List<PlayerInfo> Players { get; private set; } = new List<PlayerInfo>();

    private const int MAX_NUMBER_OF_PLAYERS = 8;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        Players.Clear();
        for (int i = 0; i < MAX_NUMBER_OF_PLAYERS; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out PlayerInfo playerInfo))
            {
                Players.Add(playerInfo);
            }
        }
    }

    public PlayerInfo GetPlayer(int i)
    {
        if (i > Players.Count)
        {
            Debug.LogWarning("The specified number for PlayerManager/GetPlayer is above the player limit. (Players.Count: " + Players.Count + ").");
            return null;
        }
        else if (i < 0)
        {
            Debug.LogWarning("The specified number for PlayerManager/GetPlayer is less than 0.");
            return null;
        }
        else
        {
            return Players[i];
        }
    }
    public PlayerInfo GetPlayer(Player player)
    {
        if ((int)player > Players.Count)
        {
            Debug.LogWarning("The specified number for PlayerManager/GetPlayer is above the player limit. (Players.Count: " + Players.Count + ").");
            return null;
        }
        else if ((int)player < 0)
        {
            Debug.LogWarning("The specified number for PlayerManager/GetPlayer is less than 0.");
            return null;
        }
        else
        {
            return Players[(int)player];
        }
    }

}
