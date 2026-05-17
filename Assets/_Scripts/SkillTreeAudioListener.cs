using GridSkillTree;
using UnityEngine;

public class SkillTreeAudioListener : MonoBehaviour
{
    [SerializeField] private SkillTreeRuntime tree;
    [SerializeField] private AudioClip[] upgradeSounds;
    [SerializeField] private float volume = 1f;

    private void OnEnable()
    {
        tree.OnNodeUpgraded += PlayUpgradeSound;
    }

    private void OnDisable()
    {
        tree.OnNodeUpgraded -= PlayUpgradeSound;
    }

    private void PlayUpgradeSound(SkillNodeData node)
    {
        if (upgradeSounds == null || upgradeSounds.Length == 0)
            return;

        SoundManagerSO.PlaySoundFXClip(
            upgradeSounds,
            Vector3.zero,
            volume
        );
    }
}