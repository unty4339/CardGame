# クラス説明

- PlayerData
    - プレイヤー情報システムで定義済み

- Unit
    - ユニットトーテムシステムで定義済み

- Totem
    - ユニットトーテムシステムで定義済み

- GameState
    - Monobehaviorを継承しない
    - 現在の自分から見た状況を一意に表現する状態データについて責任を持つ
    - MyHandを示すList<Card>またはソート済みリストプロパティを持つ。自分の手札の内容を保持する
    - MyField、OpponentFieldを示すFieldZoneプロパティを持つ。自分と相手のゾーンのユニットとトーテムの内容を保持する
    - MyHP、OpponentHPを示すintプロパティを持つ。自分と相手それぞれの残りヒットポイントを保持する
    - MyMP、OpponentMPを示すintプロパティを持つ。自分と相手それぞれの残りマナポイントを保持する

- StateEvaluator
    - Monobehaviorを継承しない
    - 状態の評価について責任を持つ
    - Evaluate (GameState state): GameStateを受け取り、状態評価値を返す
    - ユニットの数、ステータスなど状態のデータを特定の重みで評価し総和を取る機能を持つ

- GameAction
    - Monobehaviorを継承しない
    - 各カードやユニットを選択して使用できる行動について責任を持つ
    - ActionTypeを示すActionTypeプロパティを持つ。攻撃、プレイなどの行動種類を保持する
    - Targetを示すobjectプロパティを持つ。場の特定のカードや効果の選択肢などの行動対象を保持する
    - SourceCard、SourceUnitなどの参照を保持する。行動の主体を示す

- ActionType
    - enumクラス
    - 行動の種類を定義する
    - Attack、Playの値を保持する

- AIController
    - Monobehaviorを継承する
    - AIの思考と行動決定について責任を持つ
    - CreateState (int playerId): プレイヤーIDを受け取り、現在のデッキ・手札・ゾーンの内容からGameStateを作成する
    - GetAvailableActions (GameState state): GameStateを受け取り、行動一覧を作成する
    - SimulateNextState (GameState state, GameAction action): GameStateとGameActionを受け取り、その行動を取った時の次のGameStateを作成する
    - DecideActions (int playerId): プレイヤーIDを受け取り、状態評価値を最大化するような行動の使用順を返す
    - 状態→行動一覧作成→選択→次状態の繰り返しによる状態遷移で探索する機能を持つ

# 処理シーケンス

## 状態作成の処理シーケンス

1. 作成リクエスト（ゲームフロー管理 → AIController）
    - プレイヤーIDが渡される
    - CreateState(playerId)が呼ばれる

2. データの収集（AIController → PlayerManager）
    - PlayerManagerから該当プレイヤーと相手のPlayerDataを取得する
    - 手札、フィールドゾーン、HP、MPの値を取得する

3. GameStateの構築（AIController）
    - 取得したデータをGameStateに格納する
    - MyHandは順序なしまたはソートして一意に表現する
    - MyField、OpponentFieldはゾーンのユニット・トーテムの内容を保持する

## 行動探索の処理シーケンス

1. 探索開始（AIController）
    - 現在のGameStateを初期状態とする
    - DecideActions()が呼ばれる

2. 行動一覧の作成（AIController）
    - GetAvailableActions(currentState)で行動一覧を取得する
    - 手札とフィールドの全カード・ユニット・トーテムの行動を合わせる

3. 評価と選択（AIController → StateEvaluator）
    - 各行動についてSimulateNextState()で次状態を取得する
    - StateEvaluator.Evaluate()で各次状態を評価する
    - 評価値が最も高い行動を選択する

4. 反復（AIController）
    - 選択した行動を行動列に追加する
    - 次状態を現在状態として、行動一覧が空になるまで手順2〜3を繰り返す

5. 結果の返却（AIController）
    - 構築した行動列を行動キューに変換して返す
