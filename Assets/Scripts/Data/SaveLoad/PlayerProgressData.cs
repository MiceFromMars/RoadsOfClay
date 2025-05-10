using System;
using System.Collections.Generic;

namespace ROC.Data.SaveLoad
{
    [Serializable]
    public class PlayerProgressData
    {
        public int TotalScore;
        public float MaxHeight;
        public float MaxSpeed;
        public List<LevelProgressData> LevelProgress = new List<LevelProgressData>();
    }
    
    [Serializable]
    public class LevelProgressData
    {
        public int LevelIndex;
        public bool IsUnlocked;
        public int MaxScore;
        public float MaxHeight;
        public float MaxSpeed;
    }
} 