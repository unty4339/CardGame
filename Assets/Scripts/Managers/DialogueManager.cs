using CardBattle.Core.Deck;
using CardBattle.Core.Field;
using System.Collections;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// 台詞のトリガー管理とコルーチン起動について責任を持つ
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        private static DialogueManager _instance;
        public static DialogueManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        /// <summary>
        /// カードがプレイされたことをトリガーに台詞用のコルーチンを起こす
        /// </summary>
        public void OnCardPlayed(Card card)
        {
            if (card == null) return;

            StartCoroutine(PlayDialogueCoroutine(card.Template?.name ?? "Card"));
        }

        /// <summary>
        /// ユニットが破壊されたことをトリガーに台詞用のコルーチンを起こす
        /// </summary>
        public void OnUnitDestroyed(Unit unit)
        {
            if (unit == null) return;

            StartCoroutine(PlayDialogueCoroutine("UnitDestroyed"));
        }

        /// <summary>
        /// 外部から指定したトリガーIDで台詞用のコルーチンを起こす
        /// </summary>
        public void OnCustomTrigger(string triggerId)
        {
            if (string.IsNullOrEmpty(triggerId)) return;

            StartCoroutine(PlayDialogueCoroutine(triggerId));
        }

        private IEnumerator PlayDialogueCoroutine(string triggerId)
        {
            // TODO: 台詞データの検索と表示
            yield return null;
        }
    }
}
