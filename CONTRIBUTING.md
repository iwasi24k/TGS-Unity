## Git 開発ルール（Unityチーム開発用）

このプロジェクトは **Git フローに基づいたブランチ戦略**で開発します。
エンジン: Unity

---

# ブランチ構成

```
main        : リリース用の安定ブランチ
develop     : 開発統合ブランチ
feature/*   : 個別機能開発ブランチ
```

例

```
feature/player-controller
feature/enemy-ai
feature/ui-system
```

---

# 開発フロー

```
feature/* → develop → main
```
1. Issueを作成
2. `develop` から `feature` ブランチを作成
3. 機能開発
4. `develop` に Pull Request
5. レビュー後 `develop` にマージ
6. リリース時 `develop` → `main` に Pull Request

---

# Issue記述法

```
## 内容
- 

## 変更箇所
- 

## 完了条件
-[ ]

## 担当者
@github-name
```
変更箇所には、変更するファイルなどを必ず記載してください。

---

# シェルスクリプトによる実行

## ブランチの作成

どのブランチからでも実行可能です。  
最新の `develop` を取得し、新しいブランチを作成して切り替えます。
```bash
./scripts/gitdev <new branch name>
```

### ブランチ命名規則

以下のプレフィックスを使用してください。

- `feature/`
- `fix/`
- `hotfix/`
- `refactor/`

例:

```bash
./scripts/gitdev feature/player-move
./scripts/gitdev fix/enemy-spawn
```

注意事項
- 同名ブランチが存在する場合は作成できません。
- Working Tree に未コミットの変更が存在する場合は実行できません。
- develop ブランチは origin/develop の最新状態に同期されます。


## ブランチの更新とPush
現在のローカルブランチへ最新の develop を取り込み、commit と push を実行します。
```bash
./scripts/gitpush "commit message"
```

注意事項
- main / develop ブランチでは実行できません。
- 変更ファイルは自動で stage (git add .) されます。
- develop 取り込み時に conflict が発生した場合は処理が停止します。

---

# 作業開始前

作業開始前に `develop` を最新状態にする

```bash
git switch develop      # developへ移動
git fetch origin        # リモート情報を取得
git pull origin develop # developを最新化
```

---

# ブランチ作成

```bash
git switch develop
git fetch origin
git pull origin develop
git switch -c feature/<feature-name> # 新規ブランチ作成
```

例

```bash
git switch -c feature/player-movement
```

# ブランチ名の例

```txt
feature/player-movement # 機能追加
fix/jump-bug            # バグ修正
refactor/input-system   # リファクタリング
```

---

# 作業中の基本操作

```bash
git add . # 変更をステージング
git commit -m "feat: プレイヤー移動を追加" # 履歴保存
git push origin feature/<feature-name> # GitHubへ反映
```

# 状態確認・復元

```bash
git status  # 現在の変更状態を確認
git merge   # 他ブランチを取り込む
git restore # 変更を取り消す
```

例

```bash
git restore main.cpp # main.cpp の変更を破棄
```

---
# featureブランチ削除

featureブランチはマージ後に削除する。

ローカル削除

```bash
git branch -d feature/<feature-name>
```

リモート削除

```bash
git push origin --delete feature/<feature-name>
```

---

# よく使う確認コマンド

```bash
git branch         # ブランチ一覧
git log --oneline  # コミット履歴
git diff           # 変更差分を確認
```

---

# Pull Request

## feature → develop

* 開発者が Pull Request を作成
* 承認後マージ

## develop → main

* リリース前に作成
* 最低1人のレビュー
* 承認後マージ

---

# Pull Request ルール

* レビュー承認が必要
* 未解決コメントがある場合はマージ禁止
* 新しいコミット追加時は再レビュー
* 可能な限り小さな変更単位でPRを作成

---

# コミットメッセージルール

形式

```
type: 内容
```

例

```
feat: 新しい敵AIを追加
fix: プレイヤー移動バグ修正
refactor: コード整理
docs: ドキュメント更新
```

使用するtype

```
feat      : 新機能
fix       : バグ修正
refactor  : コード整理
docs      : ドキュメント変更
style     : フォーマット変更
test      : テスト追加
chore     : ビルド・設定変更
```

---

# 禁止事項

* `main` への直接 push
* `develop` への直接 push
* `force push`
* `feature → main` の直接マージ
* Unityの `Library` フォルダのコミット

---

# Unity プロジェクト注意点

Git管理対象

```
Assets/
Packages/
ProjectSettings/
```

Git管理対象外

```
Library/
Temp/
Logs/
Obj/
Build/
Assets/Textues
Assets/Models
Assets/Audio
```

`.gitignore` により自動で除外されます。

※Asset類（Texture / Model / Audio）の追加・更新は、事前に確認を取ったうえでコミットしてください。

---

# Unityシーン編集ルール

UnityのSceneファイルはコンフリクトが発生しやすいため注意してください。

対象ファイル

```
*.unity
*.prefab
```

ルール

* Sceneを編集する場合はチームに共有
* 同じSceneの同時編集を避ける
* 大きなScene変更は小さく分割してPR

---

# マージ順序

```
feature → develop → main
```

直接

```
feature → main
```

は禁止。

---

# まとめ

```
featureブランチで開発
↓
developへPull Request
↓
レビュー後developへマージ
↓
リリース時にmainへマージ
```

このルールに従ってチーム開発を行う。
