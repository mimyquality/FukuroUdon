# 更新履歴
このプロジェクトに対する注目すべきすべての変更は、このファイルに記録されます。  
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/)のフォーマットと、
[Semantic Versioning](https://semver.org/spec/v2.0.0.html)の採番に則り更新されます。  
利用手順は[こちら](https://github.com/mimyquality/FukuroUdon/wiki)からご確認ください。

## [1.1.0] - 2023/4/13
### Add
- [PlayerAudio Master](https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master)を追加

### Fixed
- SamplesフォルダーのパスがRuntime以下になっていたのを修正しました。

## [1.0.0] - 2023/4/8
### Changed
- VCC 2.1.0からコミュニティーリポジトリーに対応したのに合わせて、[MimyLabリポジトリー](https://vpm.mimylab.com/)を公開しました。VCCからFukuro Udonのバージョン管理ができるようになります。
- 上記に合わせて[導入手順](https://github.com/mimyquality/FukuroUdon)を書き直しました。

## [0.3.2] - 2023/2/25
### Delete
- Smart Slideshow
  - ImageDownloader対応にマテリアルが不要だったため、これを削除しました。

## [0.3.1] - 2023/2/24
### Changed
- Add Componentメニューにて、関連スクリプトを *Fukuro Udon* カテゴリーに表示されるようにしました。

## [0.3.0] - 2023/2/24
### Add
- Smart Slideshow
  - ImageDownloaderに対応しました。初回表示時にインターネットから画像を読み込んで表示できるようになります。
  - 読み込める画像はImageDownloaderの制限に準拠します。詳細は公式ドキュメント( https://docs.vrchat.com/docs/image-loading )を見てください。

## [0.2.10] - 2023-2-20
### Fixed
- Manual ObjectSync
  - 他人がピックアップした際にオブジェクトが一瞬跳ねる現象を低減しました。

## [0.2.8] - 2023-2-7
### Fixed
- Manual ObjectSync
  - 0.2.7バージョンで、later-joiner視点でjoin直後が非アクティブだと同期しなかったのを修正しました。

## [0.2.7] - 2023-2-3
### Fixed
- Manual ObjectSync
  - 初回のOwner委譲時に、Transform値が全て0になるのを修正しました。

## [0.2.6] - 2022-12-28
### Fixed
- Manual ObjectSync
  - Rotationの変動チェックがMove Check Spaceを無視していたのを修正しました。

## [0.2.5] - 2022-11-04
### Fixed
- Manual ObjectSync
  - 11/3のアップデートにて、VRCPickupと併用した場合で、ピックアップしたまま退室されると他の人が掴み直せなくなる不具合に対応しました。

## [0.2.4] - 2022-10-26
### Fixed
- Manual ObjectSync
  - ピックアップを持ち替えた時に、他人からは持ち替えてないように見える可能性があるのを修正しました。

## [0.2.3] - 2022-10-18
### Added
- Active Relayを追加

### Fixed
- 全体的に軽微なリファクタリング

## [0.2.2] - 2022-10-10
βリリース
### Added
- GameObject Cellerを追加
- Grab SlideDoorを追加
- Input Flying Systemを追加
- Manual ObjectSyncを追加
- Smart Slideshowを追加
- Swivel Chairを追加
- VR Follow HUDを追加

[1.1.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.1.0
[1.0.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.0.0
[0.3.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.3.2
[0.3.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.3.1
[0.3.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.3.0
[0.2.10]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.2.10
[0.2.8]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.2.8
[0.2.7]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.2.7
[0.2.6]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.2.6
[0.2.5]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.2.5
[0.2.4]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.2.4
[0.2.3]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.2.3
[0.2.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/0.2.2