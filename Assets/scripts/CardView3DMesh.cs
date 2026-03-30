using UnityEngine;
using Poker;

public class CardView3DMesh : MonoBehaviour
{
    [Header("Mesh Renderers")]
    [SerializeField] private MeshRenderer frontRenderer;
    [SerializeField] private MeshRenderer backRenderer;

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

        if (frontRenderer != null && frontRenderer.material != null)
            ClearMaterial(frontRenderer.material);

        if (backRenderer != null && backRenderer.material != null)
            ClearMaterial(backRenderer.material);
    }

    private void RefreshVisual()
    {
        if (!hasCard || db == null) return;

        Sprite face = db.GetFace(currentCard);
        Sprite back = db.GetBack();

        if (frontRenderer != null && frontRenderer.material != null && face != null)
            ApplySpriteToMaterial(frontRenderer.material, face);

        if (backRenderer != null && backRenderer.material != null && back != null)
            ApplySpriteToMaterial(backRenderer.material, back);

        if (frontRenderer != null)
            frontRenderer.enabled = faceUp;

        if (backRenderer != null)
            backRenderer.enabled = !faceUp;
    }

    private void ApplySpriteToMaterial(Material mat, Sprite sprite)
    {
        if (mat == null || sprite == null) return;

        Texture tex = sprite.texture;
        Rect rect = sprite.textureRect;

        Vector2 scale = new Vector2(
            rect.width / tex.width,
            rect.height / tex.height
        );

        Vector2 offset = new Vector2(
            rect.x / tex.width,
            rect.y / tex.height
        );

        mat.mainTexture = tex;
        mat.mainTextureScale = scale;
        mat.mainTextureOffset = offset;
    }

    private void ClearMaterial(Material mat)
    {
        mat.mainTexture = null;
        mat.mainTextureScale = Vector2.one;
        mat.mainTextureOffset = Vector2.zero;
    }
}