# クラス説明

- GameVisualManager
    - Monobehaviorを継承する
    - アタッチ対象はシーン内の管理用オブジェクト（GameManagerなど）
    - PlayerManagerのイベントを監視し、カードの生成や移動アニメーションを指揮する。UIとデータの橋渡しについて責任を持つ
    - CardViewを示すcardPrefabプロパティを持つ。カードのプレハブを参照する
    - Transformを示すdeckTransformプロパティを持つ。画面上のデッキ位置（出現地点）を参照する
    - HandVisualizerを示すhandVisualizerプロパティを持つ。手札の並び管理クラスを参照する
    - FieldVisualizerを示すfieldVisualizerプロパティを持つ。フィールドの管理クラスを参照する
    - Start (): 初期化時にPlayerManagerのイベントを購読する
    - PlayDrawAnimation (int playerId, Card cardData): プレイヤーIDとカードデータを受け取り、デッキ位置にCardViewを生成して手札の予定位置へ移動アニメーションを再生する。移動完了後にHandVisualizerに管理権限を渡す
    - PlaySummonAnimation (): ユニット召喚時の演出を再生する

- HandVisualizer
    - Monobehaviorを継承する
    - アタッチ対象は手札エリア（Canvas内の空オブジェクト）
    - 手札にあるCardViewの扇状（アーチ状）配置の計算・適用について責任を持つ
    - List<CardView>を示す_activeCardsプロパティを持つ。現在表示中のカードリストを保持する
    - floatを示すarcAngleプロパティを持つ。扇状に広げる角度を保持する（例: 30度）
    - floatを示すspacingプロパティを持つ。カード間の距離を保持する
    - CalculatePosition (int index, int totalCount): インデックスと総数を受け取り、指定インデックスのカードが画面上のどこにあるべきかを計算して返す
    - AddCard (CardView view): CardViewを受け取り、アニメーションが完了したカードをリストに加え、整列処理を呼ぶ
    - RemoveCard (CardView view): CardViewを受け取り、リストから外し、残りのカードを詰め直す
    - UpdateLayout (): リスト内の全カードに対して、目標座標へ滑らかに移動するよう指示する

- CardView
    - Monobehaviorを継承する
    - IDragHandler、IBeginDragHandler、IEndDragHandlerインターフェースを実装する
    - アタッチ対象はカードのプレハブ
    - カード1枚の表示と、マウス操作（ドラッグおよびドロップ）の入力受け付けについて責任を持つ。移動処理自体は行わず、ドラッグ中はカーソルに追従する
    - Cardインスタンスを1つ保持する。デッキ手札システムで定義済みのCardを参照する
    - Imageを示すartworkプロパティを持つ。絵柄表示パーツを参照する
    - Textを示すcostプロパティを持つ。コスト表示パーツを参照する
    - Textを示すattackプロパティを持つ。攻撃力表示パーツを参照する
    - Textを示すhpプロパティを持つ。体力表示パーツを参照する
    - CanvasGroupを示すcanvasGroupプロパティを持つ。ドラッグ中の透けやレイキャスト無効化に使用する
    - Initialize (Card data): カードデータを受け取り、表示を更新する
    - OnBeginDrag (PointerEventData eventData): ドラッグ開始時に呼ばれる。HandVisualizerの整列対象から一時的に外れるフラグを立てる
    - OnDrag (PointerEventData eventData): ドラッグ中に呼ばれる。座標をマウス位置に更新する
    - OnEndDrag (PointerEventData eventData): ドロップ時に呼ばれる。ドロップ位置がフィールドエリアならPlayerManagerへプレイ要求を出す。プレイ不成立ならHandVisualizerの定位置に戻るアニメーションを再生する

- FieldVisualizer
    - Monobehaviorを継承する
    - アタッチ対象はフィールドエリア
    - フィールド上のユニット配置の管理について責任を持つ。カードがプレイされた時、ここにあるスロットにユニットが生成される
    - AddUnit (UnitView unitView): UnitViewを受け取り、新しいユニットをフィールドに追加し、並べ直す
    - GetNextSpawnPosition (): 次にユニットが出るべき場所（座標）を返す

- UnitView
    - Monobehaviorを継承する
    - アタッチ対象はユニットのプレハブ
    - フィールドに出た後のキャラクター表示について責任を持つ。攻撃時のモーションやダメージ表示を担当する
    - Unitインスタンスを1つ保持する。ユニットトーテムシステムで定義済みのUnitを参照する
    - Bind (Unit unitData): ユニットデータを受け取り、紐付けてHPやATKの表示を同期する
    - PlayAttackAnimation (UnitView target): ターゲットのUnitViewを受け取り、ターゲットに向かって突進して戻るなどの演出を再生する
    - PlayDamageAnimation (int damage): ダメージ値を受け取り、揺れる、赤く点滅するなどの演出を再生する

- PlayerInfoView
    - Monobehaviorを継承する
    - アタッチ対象は画面端のステータスパネル
    - プレイヤーのHP、MP、PP（プレイポイント）の表示更新について責任を持つ。数値の正確な同期を担当する
    - UpdateState (PlayerData data): プレイヤーデータを受け取り、スライダーやテキストを最新の値にする。毎フレームまたは変更イベント時に呼ばれる。プレイヤー情報システムで定義済みのPlayerDataを参照する

# 処理シーケンス

## カードを引いたときの表示の処理シーケンス

1. ドロー処理（ゲームフロー管理 → PlayerManager）
    - PlayerManagerがカードを引く処理を行う
    - デッキから手札へカードデータが移動する

2. イベント通知（PlayerManager）
    - PlayerManagerがOnCardDrawnイベントを発行する
    - プレイヤーIDと引いたCardが渡される

3. 演出開始（GameVisualManager）
    - GameVisualManagerがイベントを購読しており、PlayDrawAnimationが呼ばれる
    - デッキ位置にCardViewを生成する
    - HandVisualizerに「このカードが入る予定の場所」を計算させる
    - デッキ位置から手札の予定位置へカードを移動させるアニメーションを再生する

4. 着地と整列（GameVisualManager → HandVisualizer）
    - 移動が終わるとHandVisualizerにカードの管理権限を渡す
    - HandVisualizerがAddCardでカードをリストに加え、扇状に整列する

## カードをプレイしたときの表示の処理シーケンス

1. プレイヤー操作（CardView）
    - プレイヤーがCardViewをドラッグして場にドロップする
    - OnEndDragでドロップ位置がフィールドエリアか判定する

2. プレイ要求と判定（CardView → PlayerManager）
    - フィールドエリアへのドロップ時、PlayerManagerへプレイ要求を出す
    - PlayerManagerがプレイ可否を判定する

3. プレイ不成立時（CardView → HandVisualizer）
    - プレイが不成立なら、HandVisualizerの定位置に戻るアニメーションを再生する

4. プレイ成立時の通知（PlayerManager）
    - プレイ成功時、PlayerManagerがOnUnitSummonedイベントを発行する

5. 演出（GameVisualManager）
    - GameVisualManagerがイベントを拾い、手札のCardViewを消す
    - FieldVisualizerのGetNextSpawnPositionで生成位置を取得する
    - UnitViewを生成し、FieldVisualizerのAddUnitでフィールドに追加する
