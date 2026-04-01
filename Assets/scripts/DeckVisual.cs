using UnityEngine;
using Poker;

public class DeckVisual : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private CardSpriteDatabase spriteDb;
    [SerializeField] private Transform deckSpawn;
    [SerializeField] private Transform visualRoot;

    [Header("Stack")]
    [SerializeField] private int stackCount = 12;
    [SerializeField] private float yStep = 0.002f;

    [Header("Randomness")]
    [SerializeField] private float randomYaw = 1.5f;
    [SerializeField] private float randomX = 0.002f;
    [SerializeField] private float randomZ = 0.002f;

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

        if (visualRoot == null)
        {
            Debug.LogWarning("[DeckVisual] visualRoot is NULL, using this.transform");
            visualRoot = transform;
        }

        ClearOldCards();
        BuildDeck();
    }

    [ContextMenu("Rebuild Deck")]
    public void BuildDeck()
    {
        ClearOldCards();

        for (int i = 0; i < stackCount; i++)
        {
            Vector3 localPos =
                deckSpawn.localPosition +
                Vector3.up * (i * yStep) +
                new Vector3(
                    Random.Range(-randomX, randomX),
                    0f,
                    Random.Range(-randomZ, randomZ)
                );

            Quaternion localRot =
                deckSpawn.localRotation *
                Quaternion.Euler(0f, Random.Range(-randomYaw, randomYaw), 0f);

            GameObject go = Instantiate(cardPrefab, visualRoot);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;

            CardView3DMesh meshView = go.GetComponent<CardView3DMesh>();
            if (meshView != null)
            {
                meshView.SetDatabase(spriteDb);
                meshView.Init(new Card(Suit.Hearts, Rank.Two), false);
                continue;
            }

            CardView3D spriteView = go.GetComponent<CardView3D>();
            if (spriteView != null)
            {
                spriteView.SetDatabase(spriteDb);
                spriteView.Init(new Card(Suit.Hearts, Rank.Two), false);
                continue;
            }

            Debug.LogWarning("[DeckVisual] prefab has no CardView3DMesh or CardView3D");
        }
    }

    [ContextMenu("Clear Deck")]
    public void ClearOldCards()
    {
        if (visualRoot == null) return;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = visualRoot.GetChild(i);

            if (child == deckSpawn)
                continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
