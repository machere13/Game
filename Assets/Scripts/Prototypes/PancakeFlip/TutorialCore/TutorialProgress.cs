namespace IdlePancake.PancakeFlip.TutorialCore
{
    /// <summary>Pure step-index tracker for the tutorial. No UnityEngine dependency.</summary>
    public sealed class TutorialProgress
    {
        public int StepCount { get; }
        public int CurrentIndex { get; private set; }
        public bool IsComplete { get; private set; }

        public TutorialProgress(int stepCount)
        {
            StepCount = stepCount < 0 ? 0 : stepCount;
            CurrentIndex = 0;
            IsComplete = StepCount == 0;
        }

        public void Advance()
        {
            if (IsComplete) return;
            CurrentIndex++;
            if (CurrentIndex >= StepCount)
            {
                CurrentIndex = StepCount;
                IsComplete = true;
            }
        }
    }
}
