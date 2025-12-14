# TextMesh Pro VRC Fallback Font JP

TextMesh Proでワールド容量を増やさないためのフォールバックフォント 日本向け

## 概要

TextMesh Proにはフォントアセットが必要ですが、日本語フォントは文字数が多く、そのまま含めるとワールド容量を増加させてしまいます。

Fallback Fonts機能を使ってVRChatクライアント内蔵のNotoフォントを使うことで、容量増加無しに日本語フォントを利用できます。

本アセットはFallback Fontsに設定する空のフォントと、エディタでのプレビュー用の日本語文字セットが含まれるNoto Sans JPフォントアセットを含みます。

- [TextMeshPro in VRChat](https://hai-vr.notion.site/TextMeshPro-in-VRChat-91561782adea47a78569cec641fd5ee9)
- [【VRChat】Emptyフォントでワールドを軽量化しよう](https://note.com/nomlas/n/n89690221c221)

日本語文字セットは[kgsi/japanese_full.txt](https://gist.github.com/kgsi/ed2f1c5696a2211c1fd1e1e198c96ee4#file-japanese_full-txt)を利用しました。

## インストール

### VCCによる方法

1. https://vpm.narazaka.net/ から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. プロジェクトの「Manage Project」から「TextMesh Pro VRC Fallback Font JP」をインストールします。

## 使い方

1. 前提として、「TMP Essentials」をインポートして下さい。
  - TextMesh Proを既に使用している場合、インポートされているはずです。
  - Edit→Project SettingsでProject Settingsを開き、TextMesh ProタブからTMP Essentialsをインポートできます。
2. 「Tools→TextMesh Pro VRC Fallback Font JPを設定」を実行すると、デフォルトフォントが空のフォント「Empty SDF for Default Font」になり、フォールバックフォントが日本語文字セットのNoto Sans JPに設定されます。
  - 既に設定されていたフォントは手動で変更する必要があります。
  - この操作は、Project SettingsのTextMesh Pro→Settingsタブを開いて、Default Font Assetに「Empty SDF for Default Font」を、Fallback Font Assets Listに「NotoSansJP-Medium SDF for Fallback Font」を設定するのと同じです。

## 更新履歴

- 1.0.0
  - リリース

## ライセンス

Noto Sans JPフォントのライセンスと同一です。

このライセンスはエディタでのプレビュー用に使用されるNoto Sans JPフォントについてのものです。上記の使い方をする場合はワールド自体にNoto Sans JPフォントアセットは含まれないため、ワールドへの適用や表記をする必要はありません。ワールドではNotoフォントが使われますが、これはVRChatクライアントに含まれているものであるため、VRChat側の取り扱い範囲になります。

[SIL Open Font License](LICENSE.txt)
