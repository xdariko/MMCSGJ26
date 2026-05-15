using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;

    public void Setup(float damage, bool isCrit)
    {
        text.text = Mathf.RoundToInt(damage).ToString();

        if (isCrit)
        {
            text.color = Color.yellow;
            text.fontSize *= 1.25f;
        }
        else
        {
            text.color = Color.white;
        }

        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(Random.Range(-0.4f, 0.4f), 1.2f, 0f);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(end, 0.6f).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(isCrit ? 1.3f : 1f, 0.15f).From(0.5f));
        seq.Join(text.DOFade(0f, 0.6f).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
