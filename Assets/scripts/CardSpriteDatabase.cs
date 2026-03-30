// CardSpriteDatabase.cs
using UnityEngine;
using Poker;

[CreateAssetMenu(fileName = "CardSpriteDatabase", menuName = "Poker/Card Sprite Database")]
public class CardSpriteDatabase : ScriptableObject
{
    [Header("52 sprites in order: Hearts 2..A, Diamonds 2..A, Clubs 2..A, Spades 2..A")]
    public Sprite[] faceSprites = new Sprite[52];

    public Sprite backSprite;

    public Sprite GetFace(Card card)
    {
        if (faceSprites == null || faceSprites.Length < 52) return null;

        int suitOffset = (int)card.Suit * 13;
        int rankOffset = (int)card.Rank - 2;
        int idx = suitOffset + rankOffset;

        if (idx < 0 || idx >= faceSprites.Length) return null;
        return faceSprites[idx];
    }

    public Sprite GetBack() => backSprite;
}