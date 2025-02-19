using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    Default
}

public enum GameQueue
{
    Solo,
    Team
}

public enum Map
{
    Default
}

[Serializable]
public class UserData 
{
    public string userName;
    public string userAuthId;
    public GameInfo userGamePreferences = new GameInfo();
}

[Serializable]
public class GameInfo
{
    public Map map;
    public GameMode gameMode;
    public GameQueue gameQueue;

    public string ToMultiplayQueue()
    {
        if (gameQueue == GameQueue.Team)
        {
            return "team-queue";
        }

        return "solo-queue";
    }
}