using System;

[Serializable]
public class PokerUnifiedSaveData
{
    public string saveId;
    public string saveName;
    public string sceneName;
    public string createdAt;
    public string screenshotFileName;

    public int playerHp;
    public int aiHp;
    public int potHp;
    public bool playerIsDealer;
}
