namespace ModelChecker {
    class LTSTransition {
        public LTSState Left;
        public LTSState Right;
        public string Action;

        public LTSTransition(LTSState left, LTSState right, string action) {
            Left = left;
            Right = right;
            Action = action;
        }

        public LTSTransition(LTSState startState, string label, LTSState endState) {
            // TODO: Complete member initialization
            Left = startState;
            Action = label;
            Right = endState;
        }

        public override string ToString() {
            return Left + " --" + Action + "--> " + Right;
        }
    }
}
