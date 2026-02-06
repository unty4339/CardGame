using CardBattle.Core.Field;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// フィールドに出た後のキャラクター表示について責任を持つ
    /// </summary>
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private Text attackText;
        [SerializeField] private Text hpText;
        [SerializeField] private Image bodyImage;

        public Unit Unit { get; private set; }

        /// <summary>
        /// ユニットデータを受け取り、紐付けてHPやATKの表示を同期する
        /// </summary>
        public void Bind(Unit unitData)
        {
            Unit = unitData;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (Unit == null) return;
            if (attackText != null) attackText.text = Unit.Attack.ToString();
            if (hpText != null) hpText.text = Unit.HP.ToString();
        }

        /// <summary>
        /// ターゲットのUnitViewに向かって突進して戻る演出を再生する
        /// </summary>
        public void PlayAttackAnimation(UnitView target)
        {
            if (target == null) return;
            StartCoroutine(AttackAnimationCoroutine(target));
        }

        private System.Collections.IEnumerator AttackAnimationCoroutine(UnitView target)
        {
            var startPos = transform.position;
            var targetPos = target.transform.position;
            var duration = 0.15f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            transform.position = targetPos;
            yield return new WaitForSeconds(0.05f);

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                transform.position = Vector3.Lerp(targetPos, startPos, t);
                yield return null;
            }

            transform.position = startPos;
        }

        /// <summary>
        /// ダメージ値を受け取り、揺れる・赤く点滅するなどの演出を再生する
        /// </summary>
        public void PlayDamageAnimation(int damage)
        {
            StartCoroutine(DamageAnimationCoroutine());
        }

        private System.Collections.IEnumerator DamageAnimationCoroutine()
        {
            if (bodyImage != null)
            {
                var original = bodyImage.color;
                bodyImage.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                bodyImage.color = original;
            }

            var rt = transform as RectTransform;
            if (rt != null)
            {
                var orig = rt.localPosition;
                for (var i = 0; i < 3; i++)
                {
                    rt.localPosition = orig + new Vector3(3f, 0f, 0f);
                    yield return new WaitForSeconds(0.03f);
                    rt.localPosition = orig + new Vector3(-3f, 0f, 0f);
                    yield return new WaitForSeconds(0.03f);
                }
                rt.localPosition = orig;
            }

            RefreshDisplay();
        }
    }
}
