using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// フィールドに出た後のキャラクター表示について責任を持つ。攻撃ドラッグ（自分ユニット→相手ユニット/相手プレイヤーゾーン）に対応。
/// </summary>
public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}