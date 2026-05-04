namespace Poker
{
    public struct NPCHybridBrainInput
    {
        public float aiHandStrength;
        public float riskLevel;
        public float potPressure;
        public float callPressure;

        public float aiHpNormalized;
        public float playerHpNormalized;

        public float playerAggression;
        public float playerSuspicion;

        public float isPreflop;
        public float isFlop;
        public float isTurn;
        public float isRiver;

        public float canCheck;
        public float canCall;
        public float canRaise;
        public float canFold;

        public float knifeAvailable;
        public float randomMood;

        public int toCall;
        public int aiHp;
        public int currentPot;
        public int bigBlind;

        public float bluffFactor;
    }
}