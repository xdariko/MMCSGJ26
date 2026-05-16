using UnityEngine;

[CreateAssetMenu(menuName = "Levels/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public LevelEntry[] levels;
}

[System.Serializable]
public class LevelEntry
{
    public string levelName;
    public WaveConfig wave;
}
