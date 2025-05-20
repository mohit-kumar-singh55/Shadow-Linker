using UnityEngine;

[CreateAssetMenu(fileName = "NewDifficultySettings", menuName = "Game/DifficultySettings")]
public class DifficultySettings : ScriptableObject
{
    public float viewRadius;
    public float detectionTime;
    public float losePlayerTime;
}