using UnityEngine;
using Poker;

public class DeckVisual : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private CardSpriteDatabase spriteDb;
    [SerializeField] private Transform deckSpawn;

    [SerializeField] private int stackCount = 12;
    [SerializeField] private float yStep = 0.002f;

    private void Start()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("[DeckVisual] cardPrefab is NULL");
            return;
        }

        if (spriteDb == null)
        {
            Debug.LogError("[DeckVisual] spriteDb is NULL");
            return;
        }

        if (deckSpawn == null)
        {
            Debug.LogError("[DeckVisual] deckSpawn is NULL");
            return;
        }

        for (int i = 0; i < stackCount; i++)
        {
            Vector3 pos = deckSpawn.position + Vector3.up * (i * yStep);
            Quaternion rot = deckSpawn.rotation;

            GameObject go = Instantiate(cardPrefab, pos, rot, deckSpawn);

            // Сначала пробуем mesh-версию
            CardView3DMesh meshView = go.GetComponent<CardView3DMesh>();
            if (meshView != null)
            {
                meshView.SetDatabase(spriteDb);
                meshView.Init(new Card(Suit.Hearts, Rank.Two), false); // false = рубашкой вверх
                continue;
            }

            // Если вдруг prefab старый 2D
            CardView3D spriteView = go.GetComponent<CardView3D>();
            if (spriteView != null)
            {
                spriteView.SetDatabase(spriteDb);
                spriteView.Init(new Card(Suit.Hearts, Rank.Two), false);
                continue;
            }

            Debug.LogWarning("[DeckVisual] prefab has no CardView3DMesh or CardView3D");
        }

        Debug.Log("[DeckVisual] Deck stack spawned.");
    }
}