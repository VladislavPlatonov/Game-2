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

    [Header("Visual Scale")]
    [SerializeField] private float visualScaleMultiplier = 0.03f;

    private Vector3 prefabBaseScale = Vector3.one;

    private void Awake()
    {
        if (cardPrefab != null)
            prefabBaseScale = cardPrefab.transform.localScale;
    }

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

        RebuildDeckVisual();
    }

    [ContextMenu("Rebuild Deck Visual")]
    public void RebuildDeckVisual()
    {
        ClearDeckVisual();

        for (int i = 0; i < stackCount; i++)
        {
            Vector3 pos =
                deckSpawn.position +
                Vector3.up * (i * yStep) +
                new Vector3(
                    Random.Range(-randomX, randomX),
                    0f,
                    Random.Range(-randomZ, randomZ)
                );

            Quaternion rot =
                deckSpawn.rotation *
                Quaternion.Euler(0f, Random.Range(-randomYaw, randomYaw), 0f);

            GameObject go = Instantiate(cardPrefab, pos, rot, visualRoot);

            ApplyWorldScale(go.transform, prefabBaseScale * visualScaleMultiplier);

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

        Debug.Log("[DeckVisual] Deck stack spawned.");
    }

    private void ApplyWorldScale(Transform target, Vector3 desiredWorldScale)
    {
        if (target == null) return;

        Vector3 parentLossy = Vector3.one;
        if (target.parent != null)
            parentLossy = target.parent.lossyScale;

        float x = parentLossy.x != 0f ? desiredWorldScale.x / parentLossy.x : desiredWorldScale.x;
        float y = parentLossy.y != 0f ? desiredWorldScale.y / parentLossy.y : desiredWorldScale.y;
        float z = parentLossy.z != 0f ? desiredWorldScale.z / parentLossy.z : desiredWorldScale.z;

        target.localScale = new Vector3(x, y, z);
    }

    [ContextMenu("Clear Deck Visual")]
    public void ClearDeckVisual()
    {
        if (visualRoot == null) return;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = visualRoot.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
