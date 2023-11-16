# 更新履歴
このプロジェクトに対する注目すべきすべての変更は、このファイルに記録されます。  
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/)のフォーマットと、
[Semantic Versioning](https://semver.org/spec/v2.0.0.html)の採番に則り更新されます。  
利用手順は[こちら](https://github.com/mimyquality/FukuroUdon/wiki)からご確認ください。

## [1.9.5] - 2023/11/16
### Fixed
- SwivelChair2
  - VRモードで操作ガイドが表示されなかったのを修正しました。

## [1.9.4] - 2023/11/12
### Fixed
- SwivelChair2
  - スマホモードでジャンプボタンの長押しが無効のため、椅子から降りる操作をジャンプボタンのダブルタップに変更しました。他のモードでは長押しのままです。

## [1.9.3] - 2023/11/12
### Fixed
- SwivelChair
  - 1.9.0以降でAndroidプラットフォームに切り替えた時、エラーになっていたのを修正しました。

## [1.9.2] - 2023/11/12
### Changed
- SwivelChair2
  - SwivelChair2内にあるピックアップが、座っている間掴めなくなる事があるのを修正しました。

## [1.9.1] - 2023/11/12
### Fixed
- SwivelChair2
  - スイベル回転の初期値がワールド空間準拠になっていたのを修正しました。

## [1.9.0] - 2023/11/11
### Added
- [SwivelChair2](https://github.com/mimyquality/FukuroUdon/wiki/Swivel-Chair-2)を追加
- Mirror Tuner
  - Add Componentメニューに追加しました。

### Deprecated
- SwivelChair
  - SwivelChair2の公開に伴い、SwivelChairは致命的な不具合を除いて更新を停止します。

## [1.8.1] - 2023/11/9
### Fixed
- 名前空間が被ってエラーになる可能性があった問題に対処しました。

## [1.8.0] - 2023/11/4
### Added
- [Mirror Tuner](https://github.com/mimyquality/FukuroUdon/wiki/Mirror-Tuner)を追加

## [1.7.1] - 2023/10/23
### Fixed
- Grab SlideDoor
  - VRモードでPickupHandleを掴んでもLimitedPosition/LookConstraintが有効にならないバグを修正しました。

## [1.7.0] - 2023/10/23
### Added
- VR Follow HUD
  - LocalPlayer Tracking Trackerコンポーネントを追加しました。

### Changed
- Input Flying System
  - キー入力設定の昇降操作のデフォルトを逆(Eで上昇、Qで下降)にしました。
- VR Follow HUD
  - VR Follow HUDコンポーネントはLocalPlayer Tracking Trackerを継承したコンポーネントになりました。

## [1.6.9] - 2023/9/24
### Fixed
- Grab SlideDoor
  - PickupHandleの無駄な処理を削減しました。

## [1.6.8] - 2023/9/18
### Fixed
- Grab SlideDoor
  - PickupHandleを掴んだまま動かないでいるとLimitedPosition/LookConstraintが無効になるのを修正しました。

## [1.6.7] - 2023/9/18
### Fixed
- Grab SlideDoor
  - OcclusionPortalの付いたオブジェクトをセットしても、オブジェクトのアクティブも切り替わってしまうのを修正しました

## [1.6.6] - 2023/9/17
### Fixed
- Grab SlideDoor
  - 大量に置いた場合の負荷軽減のため、必要な時だけPickupHandleからLimitedPosition/LookConstraintの有効無効を切り替えるようにしました。LimitedPosition/LookConstraint単体でも引き続き使えます。  
  - 内部処理のリファクタリングをしました。

## [1.6.5] - 2023/9/15
### Fixed
- U#がVRCSDK-WORLDに統合される発表を受けて、依存先をVRCSDK-WORLDのみに変更しました。

## [1.6.4] - 2023/8/21
### Changed
- Manual ObjectSync
  - RespawnHeightY(落下時のリスポーン基準)がVRC Scene Descriptorと連動するようになりました。

## [1.6.1] - 2023/8/20
### Fixed
- Manual ObjectSync
  - Nested Prefabに付いているManual ObjectSyncが再生/ビルド時にエラーになるのを修正しました。

## [1.6.0] - 2023/8/16
### Changed
- Manual ObjectSync
  - 大量に置いた場合の負荷軽減のため、更新処理を管理オブジェクトから配信する方式に変更しました。
- Swivel Chair
  - 大量に置いた場合の負荷軽減のため、更新処理をサブコンポーネントから配信する方式に変更しました。

## [1.5.2] - 2023/8/2
### Fixed
- Advanced World Settings
  - U# 1.1.9に正式対応しました
  - Avatar Eye Hieghtの設定を固定値ではなく、上限と下限を設定する方式に変更しました

## [1.5.0] - 2023/8/1
### Added
- [Advanced World Settings](https://github.com/mimyquality/FukuroUdon/wiki/Advanced-World-Settings)を追加

## [1.4.4] - 2023/7/5
### Fixed
- Swivel Chair
  - 最初に非アクティブだった場合の同期の安定性を向上しました。

## [1.4.3] - 2023/7/2
### Fixed
- Manual ObjectSync
  - アタッチモード/装着モード切り替え時にisKinematicが切り替わらない事があるのを修正しました。

## [1.4.2] - 2023/6/30
### Fixed
- Manual ObjectSync
  - VRCPickupと共に付けた場合に、Disallow Theftが機能していなかったのを修正しました。

## [1.4.1] - 2023/6/23
### Fixed
- Manual ObjectSync
  - PickupHandプロパティが正しくない値を返すのを修正しました。
  - PickupEventTransferにInteract()を追加しました。これに合わせてVRCPickupを必須コンポーネントから外しました。

## [1.4.0] - 2023/6/20
### Added
- Manual ObjectSync
  - 他のUdonスクリプトにピックアップ系イベントを横流しできる補助スクリプトを追加しました。  
    VRCPickupコンポーネントが付いていなくてもピックアップ系イベントを実行できるようになります。

### Changed
- Manual ObjectSync
  - アタッチモード用のフィールド名を変更しました。

## [1.3.0] - 2023/6/20
### Added
- Manual ObjectSync
  - アタッチモード、ボーンに装着モードを追加しました。
  - 使い方のサンプルとして、関連する補助スクリプトとサンプルプレハブも追加しました。

### Fixed
- Smart Slideshow
  - 次のスライドセットのページ数が現在のページ番号より少ない場合、エラー停止する不具合を修正しました。

## [1.2.1] - 2023/6/11
### Fixed
- Grab SlideDoor
  - 内部処理を見直しました。

## [1.2.0] - 2023/6/4
### Added
- Manual ObjectSync
  - 同期オブジェクト(VRC ObjectSyncまたはManual ObjectSync、SmartObjectSync等の、Respawn()メソッドの存在するUdonスクリプトが付いたオブジェクト)の一括位置リセットができるスイッチを追加しました。

### Changed
- Manual ObjectSync
- GameObject Celler
  - Add Componentメニューの階層を整理しました。

## [1.1.2] - 2023/5/27
### Added
- PlayerAudio Master
  - ユースケース別のサンプルプレハブを追加しました。使い方は[Wiki](https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master)を見てください。

## [1.1.1] - 2023/4/13
### Fixed
- PlayerAudio Master
  - サンプルプレハブ用のマテリアルがパッケージに含まれていなかったのを修正しました。

## [1.1.0] - 2023/4/13
### Added
- [PlayerAudio Master](https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master)を追加

### Fixed
- SamplesフォルダーのパスがRuntime以下になっていたのを修正しました。

## [1.0.0] - 2023/4/8
### Changed
- VCC 2.1.0からコミュニティーリポジトリーに対応したのに合わせて、[MimyLabリポジトリー](https://vpm.mimylab.com/)を公開しました。VCCからFukuro Udonのバージョン管理ができるようになります。
- 上記に合わせて[導入手順](https://github.com/mimyquality/FukuroUdon)を書き直しました。

## β版 - ～2023/2/25
### Added
- Active Relayを追加
- GameObject Cellerを追加
- Grab SlideDoorを追加
- Input Flying Systemを追加
- Manual ObjectSyncを追加
- Smart Slideshowを追加
- Swivel Chairを追加
- VR Follow HUDを追加
- Smart Slideshow
  - ImageDownloaderに対応しました。初回表示時にインターネットから画像を読み込んで表示できるようになります。
  - 読み込める画像はImageDownloaderの制限に準拠します。詳細は公式ドキュメント( https://docs.vrchat.com/docs/image-loading )を見てください。

### Fixed
- Add Componentメニューにて、関連スクリプトを *Fukuro Udon* カテゴリーに表示されるようにしました。
- Manual ObjectSync  
  - ピックアップを持ち替えた時に、他人からは持ち替えてないように見える可能性があるのを修正しました。
  - 11/3のアップデートにて、VRCPickupと併用した場合で、ピックアップしたまま退室されると他の人が掴み直せなくなる不具合に対応しました。
  - Rotationの変動チェックがMove Check Spaceを無視していたのを修正しました。
  - 初回のOwner委譲時に、Transform値が全て0になるのを修正しました。
  - 0.2.7バージョンで、later-joiner視点でjoin直後が非アクティブだと同期しなかったのを修正しました。
  - 他人がピックアップした際にオブジェクトが一瞬跳ねる現象を低減しました。

[1.9.5]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.9.5
[1.9.4]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.9.4
[1.9.3]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.9.3
[1.9.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.9.2
[1.9.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.9.1
[1.9.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.9.0
[1.8.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.8.1
[1.8.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.8.0
[1.7.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.7.1
[1.7.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.7.0
[1.6.9]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.6.9
[1.6.8]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.6.8
[1.6.7]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.6.7
[1.6.6]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.6.6
[1.6.5]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.6.5
[1.6.4]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.6.4
[1.6.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.6.0
[1.5.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.5.2
[1.5.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.5.0
[1.4.4]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.4.4
[1.4.3]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.4.3
[1.4.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.4.2
[1.4.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.4.1
[1.4.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.4.0
[1.3.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.3.0
[1.2.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.2.1
[1.2.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.2.0
[1.1.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.1.2
[1.1.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.1.1
[1.1.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.1.0
[1.0.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.0.0