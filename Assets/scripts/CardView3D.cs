using UnityEngine;
using Poker;

public class CardView3D : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer frontRenderer;
    [SerializeField] private SpriteRenderer backRenderer;

    private CardSpriteDatabase db;
    private Card currentCard;
    private bool hasCard;
    private bool faceUp;

    public void SetDatabase(CardSpriteDatabase database)
    {
        db = database;
    }

    public void Init(Card card, bool faceUp)
    {
        currentCard = card;
        hasCard = true;
        this.faceUp = faceUp;
        RefreshVisual();
    }

    public void SetFaceUp(bool value)
    {
        faceUp = value;
        RefreshVisual();
    }

    public void Clear()
    {
        hasCard = false;

        if (frontRenderer != null)
            frontRenderer.sprite = null;

        if (backRenderer != null)
            backRenderer.sprite = null;
    }

    private void RefreshVisual()
    {
        if (!hasCard || db == null) return;

        if (frontRenderer != null)
            frontRenderer.sprite = db.GetFace(currentCard);

        if (backRenderer != null)
            backRenderer.sprite = db.GetBack();

        if (frontRenderer != null)
            frontRenderer.enabled = faceUp;

        if (backRenderer != null)
            backRenderer.enabled = !faceUp;
    }
}