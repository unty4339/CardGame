using System;

/// <summary>
/// Addressablesでアセットが見つからない場合にスローされる例外
/// </summary>
public class AddressableAssetNotFoundException : Exception
{
    /// <summary>
    /// 見つからなかったアセットのアドレス
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="address">見つからなかったアセットのアドレス</param>
    public AddressableAssetNotFoundException(string address)
        : base($"Addressableアセットが見つかりませんでした: {address}")
    {
        Address = address;
    }

    /// <summary>
    /// コンストラクタ（メッセージ付き）
    /// </summary>
    /// <param name="address">見つからなかったアセットのアドレス</param>
    /// <param name="message">追加のエラーメッセージ</param>
    public AddressableAssetNotFoundException(string address, string message)
        : base($"Addressableアセットが見つかりませんでした: {address}. {message}")
    {
        Address = address;
    }
}
