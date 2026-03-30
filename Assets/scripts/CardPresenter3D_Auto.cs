using System.Collections.Generic;
using UnityEngine;

public class CardPresenter3DMesh_NoDuplicates : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Poker.PokerGame game;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private CardSpriteDatabase spriteDb;

    [Header("Points")]
    [SerializeField] private Transform deckSpawn;
    [SerializeField] private Transform[] playerSlots = new Transform[2];
    [SerializeField] private Transform[] aiSlots = new Transform[2];
    [SerializeField] private Transform[] tableSlots = new Transform[5];

    [Header("Layout")]
    [SerializeField] private float yLift = 0.02f;
    [SerializeField] private float zPerCard = 0.002f;

    [Header("Deal Animation")]
    [SerializeField] private bool animateCards = true;
    [SerializeField] private float cardDelayStep = 0.08f;
    [SerializeField] private float firstMoveTime = 0.18f;
    [SerializeField] private float secondMoveTime = 0.22f;
    [SerializeField] private float arcHeight = 0.35f;
    [SerializeField] private float landingPunchScale = 0.06f;
    [SerializeField] private bool spinCards = true;
    [SerializeField] private float spinDegrees = 360f;

    [Header("Return To Deck Animation")]
    [SerializeField] private float returnDelayStep = 0.04f;
    [SerializeField] private float returnMoveTime = 0.18f;
    [SerializeField] private float returnArcHeight = 0.20f;
    [SerializeField] private bool spinOnReturn = true;
    [SerializeField] private float returnSpinDegrees = -360f;

    [Header("Per group animation")]
    [SerializeField] private bool animatePlayerCards = true;
    [SerializeField] private bool animateAICards = true;
    [SerializeField] private bool animateTableCards = true;

    private readonly List<CardView3DMesh> playerViews = new();
    private readonly List<CardView3DMesh> aiViews = new();
    private readonly List<CardView3DMesh> tableViews = new();

    private readonly bool[] playerShown = new bool[2];
    private readonly bool[] aiShown = new bool[2];
    private readonly bool[] tableShown = new bool[5];

    private readonly Poker.Card[] playerCardsShown = new Poker.Card[2];
    private readonly Poker.Card[] aiCardsShown = new Poker.Card[2];
    private readonly Poker.Card[] tableCardsShown = new Poker.Card[5];

    private Vector3 cardBaseScale = Vector3.one;
    private int refreshVersion = 0;

    private void Awake()
    {
        if (game == null)
            game = FindFirstObjectByType<Poker.PokerGame>();

        if (cardPrefab != null)
            cardBaseScale = cardPrefab.transform.localScale;
    }

    private void OnEnable()
    {
        if (game == null) return;
        game.OnCardsChanged += Refresh;
    }

    private void OnDisable()
    {
        if (game == null) return;
        game.OnCardsChanged -= Refresh;
    }

    private void EnsureCreated()
    {
        if (cardPrefab == null || spriteDb == null) return;

        if (playerViews.Count == 0)
        {
            for (int i = 0; i < 2; i++)
                playerViews.Add(CreateAt(playerSlots[i]));
        }

        if (aiViews.Count == 0)
        {
            for (int i = 0; i < 2; i++)
                aiViews.Add(CreateAt(aiSlots[i]));
        }

        if (tableViews.Count == 0)
        {
            for (int i = 0; i < 5; i++)
                tableViews.Add(CreateAt(tableSlots[i]));
        }
    }

    private CardView3DMesh CreateAt(Transform point)
    {
        Vector3 spawnPos = point != null ? point.position : Vector3.zero;
        Quaternion spawnRot = point != null ? point.rotation : Quaternion.identity;

        var go = Instantiate(cardPrefab, spawnPos, spawnRot, transform);
        go.transform.localScale = cardBaseScale;

        var view = go.GetComponent<CardView3DMesh>();
        if (view == null)
        {
            Debug.LogError("[CardPresenter3DMesh_NoDuplicates] cardPrefab has no CardView3DMesh component.");
            return null;
        }

        view.SetDatabase(spriteDb);
        go.SetActive(false);
        return view;
    }

    private void Refresh()
    {
        if (game == null || cardPrefab == null || spriteDb == null || deckSpawn == null)
            return;

        if (playerSlots == null || playerSlots.Length < 2 ||
            aiSlots == null || aiSlots.Length < 2 ||
            tableSlots == null || tableSlots.Length < 5)
        {
            Debug.LogError("[CardPresenter3DMesh_NoDuplicates] Slots are not assigned correctly in Inspector.");
            return;
        }

        EnsureCreated();
        refreshVersion++;

        var ph = game.GetPlayerHand();
        var ah = game.GetAIHand();
        var cc = game.GetCommunityCards();

        HandleGroup(
            views: playerViews,
            shownFlags: playerShown,
            shownCards: playerCardsShown,
            desiredCards: ph,
            slots: playerSlots,
            faceUp: true,
            animateGroup: animatePlayerCards,
            groupDelayOffset: 0f,
            version: refreshVersion,
            attachToSlotsAfterDeal: true
        );

        HandleGroup(
            views: aiViews,
            shownFlags: aiShown,
            shownCards: aiCardsShown,
            desiredCards: ah,
            slots: aiSlots,
            faceUp: false,
            animateGroup: animateAICards,
            groupDelayOffset: 0.05f,
            version: refreshVersion,
            attachToSlotsAfterDeal: false
        );

        HandleGroup(
            views: tableViews,
            shownFlags: tableShown,
            shownCards: tableCardsShown,
            desiredCards: cc,
            slots: tableSlots,
            faceUp: true,
            animateGroup: animateTableCards,
            groupDelayOffset: 0.10f,
            version: refreshVersion,
            attachToSlotsAfterDeal: false
        );
    }

    private void HandleGroup(
        List<CardView3DMesh> views,
        bool[] shownFlags,
        Poker.Card[] shownCards,
        IReadOnlyList<Poker.Card> desiredCards,
        Transform[] slots,
        bool faceUp,
        bool animateGroup,
        float groupDelayOffset,
        int version,
        bool attachToSlotsAfterDeal)
    {
        int max = views.Count;

        for (int i = 0; i < max; i++)
        {
            int slotIndex = i;
            var view = views[slotIndex];

            if (view == null || slots[slotIndex] == null)
                continue;

            bool shouldExist = slotIndex < desiredCards.Count;

            Vector3 targetPos = slots[slotIndex].position + Vector3.up * yLift + Vector3.forward * (slotIndex * zPerCard);
            Quaternion targetRot = slots[slotIndex].rotation;

            if (!shouldExist && shownFlags[slotIndex])
            {
                shownFlags[slotIndex] = false;
                AnimateBackToDeckAndHide(view.gameObject, slotIndex * returnDelayStep, version);
                continue;
            }

            if (!shouldExist && !shownFlags[slotIndex])
            {
                view.gameObject.SetActive(false);
                continue;
            }

            Poker.Card desiredCard = desiredCards[slotIndex];

            if (shouldExist && !shownFlags[slotIndex])
            {
                shownFlags[slotIndex] = true;
                shownCards[slotIndex] = desiredCard;

                view.gameObject.SetActive(true);
                view.Init(desiredCard, faceUp);

                if (animateCards && animateGroup)
                {
                    AnimateCardToPoint(
                        view.gameObject,
                        targetPos,
                        targetRot,
                        attachToSlotsAfterDeal ? slots[slotIndex] : null,
                        groupDelayOffset + slotIndex * cardDelayStep,
                        spinCards,
                        version
                    );
                }
                else
                {
                    DetachFromSlot(view.gameObject);
                    view.transform.SetPositionAndRotation(targetPos, targetRot);
                    view.transform.localScale = cardBaseScale;

                    if (attachToSlotsAfterDeal)
                        AttachToSlotKeepWorldScale(view.gameObject, slots[slotIndex]);
                }

                continue;
            }

            if (shouldExist && shownFlags[slotIndex] && !SameCard(shownCards[slotIndex], desiredCard))
            {
                shownCards[slotIndex] = desiredCard;

                float returnDelay = groupDelayOffset + slotIndex * returnDelayStep;
                float redealDelay = returnDelay + returnMoveTime + 0.03f;

                Poker.Card cardToShow = desiredCard;
                bool localFaceUp = faceUp;
                Vector3 localTargetPos = targetPos;
                Quaternion localTargetRot = targetRot;
                Transform localTargetSlot = slots[slotIndex];
                bool localAttach = attachToSlotsAfterDeal;

                AnimateBackToDeck(view.gameObject, returnDelay, version, hideAfter: false);

                LeanTween.delayedCall(view.gameObject, redealDelay, () =>
                {
                    if (version != refreshVersion) return;

                    view.gameObject.SetActive(true);
                    view.transform.position = deckSpawn.position;
                    view.transform.rotation = deckSpawn.rotation;
                    view.transform.localScale = cardBaseScale;

                    view.Init(cardToShow, localFaceUp);

                    if (animateCards && animateGroup)
                    {
                        AnimateCardToPoint(
                            view.gameObject,
                            localTargetPos,
                            localTargetRot,
                            localAttach ? localTargetSlot : null,
                            0f,
                            spinCards,
                            version
                        );
                    }
                    else
                    {
                        DetachFromSlot(view.gameObject);
                        view.transform.SetPositionAndRotation(localTargetPos, localTargetRot);
                        view.transform.localScale = cardBaseScale;

                        if (localAttach)
                            AttachToSlotKeepWorldScale(view.gameObject, localTargetSlot);
                    }
                });

                continue;
            }

            if (!LeanTween.isTweening(view.gameObject))
            {
                if (attachToSlotsAfterDeal)
                {
                    AttachToSlotKeepWorldScale(view.gameObject, slots[slotIndex]);
                }
                else
                {
                    DetachFromSlot(view.gameObject);
                    view.transform.SetPositionAndRotation(targetPos, targetRot);
                    view.transform.localScale = cardBaseScale;
                }
            }
        }
    }

    private void AnimateCardToPoint(
        GameObject cardObj,
        Vector3 targetPos,
        Quaternion targetRot,
        Transform targetSlot,
        float delay,
        bool doSpin,
        int version)
    {
        if (cardObj == null || deckSpawn == null) return;

        DetachFromSlot(cardObj);
        LeanTween.cancel(cardObj);

        Vector3 startPos = deckSpawn.position;
        Quaternion startRot = deckSpawn.rotation;
        Vector3 midPos = Vector3.Lerp(startPos, targetPos, 0.5f) + Vector3.up * arcHeight;

        cardObj.transform.position = startPos;
        cardObj.transform.rotation = startRot;
        cardObj.transform.localScale = cardBaseScale;

        LeanTween.delayedCall(cardObj, delay, () =>
        {
            if (version != refreshVersion) return;

            LeanTween.move(cardObj, midPos, firstMoveTime).setEaseOutQuad();

            if (doSpin)
                LeanTween.rotateAround(cardObj, Vector3.up, spinDegrees, firstMoveTime).setEaseOutQuad();

            LeanTween.delayedCall(cardObj, firstMoveTime, () =>
            {
                if (version != refreshVersion) return;

                LeanTween.move(cardObj, targetPos, secondMoveTime).setEaseInOutQuad();
                LeanTween.rotate(cardObj, targetRot.eulerAngles, secondMoveTime).setEaseInOutQuad();

                LeanTween.delayedCall(cardObj, secondMoveTime, () =>
                {
                    if (version != refreshVersion) return;

                    if (targetSlot != null)
                    {
                        AttachToSlotKeepWorldScale(cardObj, targetSlot);
                    }

                    float punch = Mathf.Max(0f, landingPunchScale);

                    if (punch > 0f)
                    {
                        Vector3 currentScale = cardObj.transform.localScale;
                        Vector3 upScale = currentScale * (1f + punch);

                        LeanTween.scale(cardObj, upScale, 0.06f).setEaseOutQuad();

                        LeanTween.delayedCall(cardObj, 0.06f, () =>
                        {
                            if (version != refreshVersion) return;
                            LeanTween.scale(cardObj, currentScale, 0.08f).setEaseInOutQuad();
                        });
                    }
                });
            });
        });
    }

    private void AnimateBackToDeckAndHide(GameObject cardObj, float delay, int version)
    {
        AnimateBackToDeck(cardObj, delay, version, hideAfter: true);
    }

    private void AnimateBackToDeck(GameObject cardObj, float delay, int version, bool hideAfter)
    {
        if (cardObj == null || deckSpawn == null) return;

        DetachFromSlot(cardObj);
        LeanTween.cancel(cardObj);

        Vector3 startPos = cardObj.transform.position;
        Quaternion endRot = deckSpawn.rotation;
        Vector3 endPos = deckSpawn.position;
        Vector3 midPos = Vector3.Lerp(startPos, endPos, 0.5f) + Vector3.up * returnArcHeight;

        LeanTween.delayedCall(cardObj, delay, () =>
        {
            if (version != refreshVersion) return;

            LeanTween.move(cardObj, midPos, returnMoveTime * 0.5f).setEaseOutQuad();

            if (spinOnReturn)
                LeanTween.rotateAround(cardObj, Vector3.up, returnSpinDegrees, returnMoveTime).setEaseOutQuad();

            LeanTween.delayedCall(cardObj, returnMoveTime * 0.5f, () =>
            {
                if (version != refreshVersion) return;

                LeanTween.move(cardObj, endPos, returnMoveTime * 0.5f).setEaseInQuad();
                LeanTween.rotate(cardObj, endRot.eulerAngles, returnMoveTime * 0.5f).setEaseInQuad();

                LeanTween.delayedCall(cardObj, returnMoveTime * 0.5f, () =>
                {
                    if (version != refreshVersion) return;

                    cardObj.transform.position = endPos;
                    cardObj.transform.rotation = endRot;
                    cardObj.transform.localScale = cardBaseScale;

                    if (hideAfter)
                        cardObj.SetActive(false);
                });
            });
        });
    }

    private void AttachToSlotKeepWorldScale(GameObject cardObj, Transform slot)
    {
        if (cardObj == null || slot == null) return;

        Vector3 worldScale = cardObj.transform.lossyScale;

        cardObj.transform.SetParent(slot, false);
        cardObj.transform.localPosition = Vector3.zero;
        cardObj.transform.localRotation = Quaternion.identity;

        Vector3 parentScale = slot.lossyScale;

        float sx = parentScale.x != 0f ? worldScale.x / parentScale.x : cardBaseScale.x;
        float sy = parentScale.y != 0f ? worldScale.y / parentScale.y : cardBaseScale.y;
        float sz = parentScale.z != 0f ? worldScale.z / parentScale.z : cardBaseScale.z;

        cardObj.transform.localScale = new Vector3(sx, sy, sz);
    }

    private void DetachFromSlot(GameObject cardObj)
    {
        if (cardObj == null) return;

        Vector3 worldScale = cardObj.transform.lossyScale;

        cardObj.transform.SetParent(transform, true);

        Vector3 parentScale = transform.lossyScale;

        float sx = parentScale.x != 0f ? worldScale.x / parentScale.x : cardBaseScale.x;
        float sy = parentScale.y != 0f ? worldScale.y / parentScale.y : cardBaseScale.y;
        float sz = parentScale.z != 0f ? worldScale.z / parentScale.z : cardBaseScale.z;

        cardObj.transform.localScale = new Vector3(sx, sy, sz);
    }

    private bool SameCard(Poker.Card a, Poker.Card b)
    {
        return a.Suit == b.Suit && a.Rank == b.Rank;
    }

    [ContextMenu("Reset Shown Flags")]
    private void ResetShownFlags()
    {
        for (int i = 0; i < playerShown.Length; i++) playerShown[i] = false;
        for (int i = 0; i < aiShown.Length; i++) aiShown[i] = false;
        for (int i = 0; i < tableShown.Length; i++) tableShown[i] = false;
    }
}