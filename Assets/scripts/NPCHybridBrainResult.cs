namespace Poker
{
    public struct NPCHybridBrainResult
    {
        public PlayerActionType action;
        public int raiseTo;

        public EnemyHeadCalmController.EmotionState emotion;
        public NPCDialogueIntent dialogueIntent;

        public float confidence;
        public float dialogueChance;
    }
}