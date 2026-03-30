using System;

[Serializable]
public class PokerSimpleSaveData
{
    public string saveId;
    public string saveName;
    public string sceneName;
    public string createdAt;

    public int playerHp;
    public int aiHp;
    public bool playerIsDealer;
}
