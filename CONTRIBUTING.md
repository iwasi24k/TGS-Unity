w## Git 開発ルール（Unityチーム開発用）

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
[ ]

## 担当者
@github-name
```
変更箇所には、変更するファイルなどを必ず記載してください。

---

# 作業開始前

作業開始前に `develop` を最新状態にする

```bash
git switch develop
git pull origin develop
```

---

# ブランチ作成

```bash
git switch develop
git pull origin develop
git switch -c feature/<feature-name>
```

例

```bash
git switch -c feature/player-movement
```

---

# 作業中の基本操作

```bash
git add .
git commit -m "feat: プレイヤー移動を追加"
git push origin feature/<feature-name>
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
