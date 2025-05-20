using UnityEngine;

public class WinZone : MonoBehaviour
{
    const string PLAYER_TAG = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG)) GameManager.Instance.TriggerWin();
    }
}
