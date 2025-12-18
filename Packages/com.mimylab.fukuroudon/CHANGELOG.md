# 更新履歴

このプロジェクトに対する注目すべきすべての変更は、このファイルに記録されます。  
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/)のフォーマットと、
[Semantic Versioning](https://semver.org/spec/v2.0.0.html)の採番に則り更新されます。  

利用手順は[こちら](https://github.com/mimyquality/FukuroUdon/wiki)からご確認ください。

## [3.14.0] - 2025/12/16

**Added**  

- Area Culling と Boundary Culling に、VRCカメラ/ドローンを計算に含む設定を追加しました。 [#34](https://github.com/mimyquality/FukuroUdon/issues/34)
- Flexible AudioSource にコライダー範囲内に居る間の音量を変更できる機能を追加しました。 [#36](https://github.com/mimyquality/FukuroUdon/issues/36)

**Fixed**  

- AmbientEffect Assistant の Area 対象コライダーを、ver 3.12.0 以前のように動的に計算するか切り替えられるようにしました。
- 内部的に設けていたインスタンス内の人数上限(90人)を撤廃しました。
- PARegulator 系コンポーネントの設定項目を整理しました。

## [3.13.0] - 2025/11/28

**Added**  

- ActiveRelay に以下を追加しました。
  - ActiveRelay with Drop
  - ActiveRelay to GameObject in PlayerObject [#40](https://github.com/mimyquality/FukuroUdon/issues/40)
  - ActiveRelay to Physbone
  - ActiveRelay by Physbone
  - ActiveRelay by Contact
- ActiveRelay to Component で切り替えられるコンポーネントに以下を追加しました。
  - UdonBehaviour
  - U# コンポーネント(UdonSharpBehaviour を継承したもの)
  - VRC Constraint
  - VRC Physbone
  - VRC Contact Sender
  - VRC Contact Receiver
- Better AvatarPedestal に以下を追加しました。
  - Dynamics Parameter Transfer
    - Animator に付ける事で、子孫オブジェクトにある全ての Physbone と Contact Receiver Infomation を走査し、その状態を Animator にパラメーターとして渡せるコンポーネントです。
    - 子孫にある全ての VRC Contact Receiver に Contact Receiver Infomation を追加するボタンがあります。
  - Contact Receiver Infomation
    - Dynamics Parameter Transfer から Contact Receiver の状態を読み取るための補助コンポーネントです。Contact Receiver にセットで付けます。
- VR Follow HUD に正しく主観視点に追従する Camera Follow Tracker と LocalPlayer Camera Tracker を追加しました。
  - これに伴い、VR Follow HUD コンポーネントは Tracking Follow Tracker に改名しました。
- いくつかのインタラクトスイッチに public メソッドを追加しました。 [#39](https://github.com/mimyquality/FukuroUdon/issues/39)

**Changed**  

- AmbientEffect Assistant を最適化しました。
  - 全てのコンポーネントは Viewpoint Tracker が不要になりました。これにより、Viewpoint Tracker は廃止となります。代替として LocalPlayer Camera Tracker をご利用ください。
  - 主観視点基準で判定するべきものは主観視点基準で判定するようになりました。
  - 動的に Area 対象のコライダーを無効にしたり動かした後は RecalculateAreaBounds() を実行しないと正しく判定が取れません。
  - Boundary Culling コンポーネントに境界面を示す Gizmo を追加しました。
- SwivelChair2 の SC2 InputManager コンポーネントを単体オブジェクトとして切り離しました。

**Fixed**  

- Mirror Tuner の CustomMaterial がおかしくなるバグを修正しました。 [#38](https://github.com/mimyquality/FukuroUdon/issues/38)
- Animator Parameter Sync を少し軽量化しました。
- その他いくつかのコンポーネントのリファクタリングをしました。

**既知の問題**  

- Flexible SpatialAudio と Flexible ReverbZone は"耳"の位置を VRCカメラ・ドローン基準にすることができません。

## [3.12.0] - 2025/10/26

- **Added**
  - Manual ObjectSync ファミリーとして、Audio Play Sync を追加しました。

## [3.11.0] - 2025/9/21

- **Added**
  - ActiveRelay with Delay と ActiveRelay with Return を追加しました。
    - それぞれ ActiveRelay to GameObject, ActiveRelay to Transform で同じ事ができますが、用途を特化させることで設定項目を簡略化したものになります。
  - ActiveRelay to GameObject に Delay Latest Only プロパティを追加しました。
  - Advanced WorldSettings にVRCSDK 3.9.0 から増えた項目を追加しました。

- **Changed**
  - Input Flying System が非VR(デスクトップ、スマホ)では視線方向基準に飛行するようになりました。
    - 顔を上に向けて前進すれば上昇、下に向けて前進すれば下降していく、という事です。
    - この変更に合わせて、ジャンプボタンによる上昇効果を無くしました。
  
  - **Fixed**
    - ActiveRelay to Transform が特定の条件下で実行すると異常終了を引き起こす可能性があるのを修正しました。
    - ActiveRelay to Transform は実行時にピックアップを強制ドロップしてから操作するようになりました。
      - スケールだけ操作する場合は強制ドロップしません。
    - Input Flying System が飛行モードに入った直後だけ制動しなかったのを修正しました。 [#37](https://github.com/mimyquality/FukuroUdon/issues/37)

## [3.10.1] - 2025/9/19

- **Fixed**
  - Better AvatarPedestal は AvatarPedestal(scale0) オブジェクトを必要な時だけアクティブにするようにしました。
    - 無駄な読み込みを避けるため、初期状態が非アクティブになっています。

## [3.10.0] - 2025/8/25

- **Added**
  - ActiveRelay に以下を追加しました。
    - ActiveRelay to Player Teleport
    - ActiveRelay to Player Mobility
    - ActiveRelay to AvatarScaling
    - ActiveRelay to Drone
    - ActiveRelay by PlayerParticleCollision
    - ActiveRelay by DroneTrigger
    - ActiveRelay by ParticleCollision

- **Fixed**
  - InputFlySystem が飛行中に非アクティブまたは無効になった場合に、飛行状態が解除されるようになりました。
  - InputFlySystem が不要なタイミングに飛行解除処理をしていたのを無くしました。

## [3.9.0] - 2025/8/24

- **Added**
  - ActiveRelay by Collision と ActiveRelay by Trigger を追加しました。
  - 全てのスクリプトにヘルプURLを追加しました。

## [3.8.1] - 2025/7/21

- **Fixed**
  - PlayerAudio Master: 自分自身のチャットボックス表示とアバター音量がデフォルト値固定になっていたのを修正しました。 [#33](https://github.com/mimyquality/FukuroUdon/issues/33)

## [3.8.0] - 2025/6/28

- **Added**
  - PlayerAudio Regulator に `othersOnly` 設定を追加しました。

## [3.7.1] -2025/6/26

- **Fixed**
  - VRCSDK 3.8.2 にて ActiveRelay by Visible がビルド時にエラーを起こす問題に対処しました。
  - Mirror Tuner の各種プレハブのミラーに Item が映るようにしました。

## [3.7.0] - 2025/6/23

- **Added**
  - Manual ObjectSync に OnEquip/OnUnequip/OnAttach/OnDetach イベントを追加しました。 [#32](https://github.com/mimyquality/FukuroUdon/issues/32)

## [3.6.0] - 2025/5/28

- **Added**
  - ActiveRelay to VRC Component の対象に UdonBehaviour.DisableInteractive を追加しました。
  - ActiveRelay to Component の対象に Light, Animator, CanvasGroup.interactable を追加しました。
  - Better AvatarPedestal はダイアログ表示中にインタラクト判定が一時無効になるようになりました。

## [3.5.0] - 2025/5/27

- **Added**
  - 全ての ActiveRelay by シリーズは Allowed Player Name List で実行可能なプレイヤーをフィルターできるようになりました。
  - ActiveRelay by JoinLeave を追加しました。
  - ActiveRelay to UdonBehaviour がネットワークイベントも送信できるようになりました。(引数には対応しません) [#30](https://github.com/mimyquality/FukuroUdon/issues/30)

- **Changed**
  - ActiveRelay by Trigger, ActiveRelay by Collision, ActiveRelay by Station はイベントを実行したプレイヤーの種類に応じて実行をフィルターできるようになりました。
    - この変更により、Event Type に LocalPlayer～ を選択していたものは設定がリセットされます。再設定してください。
  - PlayerAudio Regulator シリーズの `White List Player Name` は `Allowed Player Name List` に改められました。
    - この変更により、内容がリセットされます。再設定してください。

- **Fixed**
  - SwivelChair2 はカメラやドローンカメラ起動中に動かなくなりました。(降りることはできます)  [#31](https://github.com/mimyquality/FukuroUdon/issues/31)

## [3.4.0] - 2025/5/24

- **Added**
  - Better AvatarPedestal 追加しました。 [#21](https://github.com/mimyquality/FukuroUdon/issues/21)
    - アバターペデスタルをデフォルトのアバターサムネイル板ではなく3Dモデルとして展示するためのプレハブです。
  - Advanced WorldSettings
    - VRCCameraSettings の初期設定を追加しました。 [#26](https://github.com/mimyquality/FukuroUdon/issues/26)
  - PlayerAudio Master
    - PA Regulator Register を追加しました。 [#25](https://github.com/mimyquality/FukuroUdon/issues/25)
      - Persistence 機能を使った、より安定して使える PA Regulator List として追加しました。
      - VCメニューの構造もこれに合わせて変更したため、改変して設置している場合は再設定が必要です。
    - PA Regulator List に 新 SendCustomNetworkEvent 対応メソッドを追加しました。
  - GameObject Celler
    - Dust Box Return Trigger を追加しました。
    - 新 SendCustomNetworkEvent 対応メソッドを追加しました。
  - Manual ObjectSync
    - 新 SendCustomNetworkEvent 対応メソッドを追加しました。

- **Changed**
  - AmbientEffect Assistant
    - ViewPoint Tracker を `GetTrackingData()` 式から `screenCamera` 式に変更しました。 [#27](https://github.com/mimyquality/FukuroUdon/issues/27)
  - ActiveRelay
    - ActiveRelay to Transform に Scale 操作を追加、Position, Rotation, Scale を選択的に指定できるよう変更しました。  [#24](https://github.com/mimyquality/FukuroUdon/issues/24)

- **Fixed**
  - GameObject Celler の Dust Box は指定座標に飛ばすのではなく、初期位置にリスポーンさせてから返却処理をするようになりました。

## [3.3.0] - 2025/3/17

- **Added**
  - ActiveRelay
    - ActiveRelay to Effect が AudioSource に対して Play()/Pause()/Stop() もできるオプションを追加
    - ActiveRelay to Effect が ParticleSystem に対して Emission モジュールのオンオフと Emit() を個別に設定できるよう追加変更
    - ActiveRelay to Transform を追加

- **Fixed**
  - PlayOneShot() を使った処理の音量が過剰に小さくなるのを修正
  - Manual ObjectSync
    - 非VRモードのプレイヤーがピックアップを掴んだ時の挙動を改善 [#23](https://github.com/mimyquality/FukuroUdon/pull/23)
    - Persistence 対象にした時、位置情報が書き戻されるように修正

## [3.2.2] - 2025/1/28

- **Changed**
  - AmbientEffect Assistant
    - Flexible SpatialAudio は AudioSource の無効ではなく、 `AudioSource.Pause()` で再生を止めるようになりました。

## [3.2.1] - 2025/1/4

- **Fixed**
  - AmbientEffec tAssistant
    - Canvas DistanceFade のコンポーネント名を修正しました。

## [3.2.0] - 2025/1/3

- **Added**
  - AmbientEffect Assistant
    - Canvas DistanceFade を追加しました。 [#20](https://github.com/mimyquality/FukuroUdon/issues/20)

- **Changed**
  - ViewPoint Tracker は非アクティブor無効な Receiver を無視するようになりました。 [#20](https://github.com/mimyquality/FukuroUdon/issues/20)

- **Fixed**
  - Manual ObjectSync
    - オーナーチェックを適宜実行するよう見直し

## [3.1.1] - 2024/12/23

- **Fixed**
  - PlayerAudio Master
    - VoiceChannel メニューで先にインスタンスに居る人のチャンネル所在表示が間違うバグを修正しました。

## [3.1.0] - 2024/12/9

- **Added**
  - AmbientEffect Assistant
    - Area Culling と Boundary Culling で有効無効を切り替えられる対象にゲームオブジェクトを追加しました。

## [3.0.2] - 2024/11/24

- **Fixed**
  - Manual ObjectSync
    - プレイヤーがピックアップしたまま退出した時の処理を再修正しました。 [#16](https://github.com/mimyquality/FukuroUdon/issues/16)

## [3.0.1] - 2024/11/23

- **Fixed**
  - Manual ObjectSync
    - プレイヤーがピックアップしたまま退出した時のリセット処理を見直し、インスタンスマスターがピックアップ状態になるバグを修正しました。 [#16](https://github.com/mimyquality/FukuroUdon/issues/16)

## [3.0.0] - 2024/11/22

- **Added**
  - SwivelChair2
    - Persistence 機能により、インスタンス越しに座位置を保存できるようになる AdjustmentSync プレハブを追加しました。  
      既存の椅子にも後付けできます。
    - SwivelChair2 コンポーネントは必須の参照を自動で子孫オブジェクトから探すようになりました。
  - PlayerAudio Master
    - PlayerAudioMaster_Channel_Sample プレハブが Persistence 機能を使うようになりました。また、チャンネルへの参加・退出時に効果音が鳴るようにしました。

## [2.0.1] - 2024/10/27

- **Fixed**
  - PlayerAudio Master
    - PA Regulator Areaの初期化処理を見直しました。
  - GameObject Celler
  - PlayerAudio Master
    - 内部処理を一部最適化しました。

## [2.0.0] - 2024/9/26

- **Changed**
  - サポートバージョンをUnity2022.3、VRCSDK 3.5.0以上に引き上げました。
  - 全てのU#スクリプトの名前空間を `MimyLab` から `MimyLab.FukuroUdon` に変更しました。
  - 全てのコンポーネントにアイコンを実装しました🦉
  - AmbientEffect Assistant
    - 軽量化。これに伴い、ViewPoint Tracker自身がビューポイントと一致して動くようになりました。

- **Fixed**
  - ActiveRelay
    - ActiveRelay to Componentに対象外のオブジェクト・コンポーネントをセットできなくなり、セットされていた場合除去されるようになりました。

## [1.24.1] - 2024/9/16

- **Fixed**
  - Manual ObjectSync
    - 複数のUdonコンポーネントが付いているオブジェクトに対して、Reset Switch for ObjectSync がManual ObjectSyncを見失わなくなりました。 [#15](https://github.com/mimyquality/FukuroUdon/pull/15)
    - これに伴い、VRC ObjectSyncもManual ObjectSyncも付いていないオブジェクトに対しては、全てのUdonコンポーネントに対して `SendCustomEvent("Reset")` を実行するようになりました。

## [1.24.0] - 2024/8/3

- **Added**
  - Active Relay
    - ActiveRelay by Visibleを追加しました。

- **Changed**
  - AmbientEffect Assistant
    - Area CullingとBoundary Cullingで有効無効を切り替えられる対象をRenderer型に拡張しました。元のMeshRenderer/SkinnedMeshRenderer配列に設定していたものは再設定が必要です。

## [1.23.0] - 2024/7/27

- **Added**
  - Active Relay
    - ActiveRelay by PlayerRespawnを追加しました。

- **Fixed**
  - SwivelChair2
    - 将来用に、iOS版もモバイルプラットフォームとして認識するよう修正しました。

## [1.22.1] - 2024/7/18

- **Fixed**
  - InputFlyingSystem
    - QとEキーのデフォルトが反対になっているのを修正

## [1.22.0] - 2024/7/18

- **Added**
  - Active Relay
    - ActiveRelay to GameObjectにDelayTimeを指定できるようにしました。

## [1.21.0] - 2024/6/6

- **Added**
  - Active Relay
    - ActiveRelay by InteractとActiveRelay by Pickupが同期できる機構を追加しました。
    - ActiveRelay to Componentで切り替えられるコンポーネントに `Camera` を追加しました。

## [1.20.0] - 2024/6/5

- **Added**
  - Active Relay
    - ActiveRelay by Interactを追加しました。
    - ActiveRelay by Pickupを追加しました。
    - ActiveRelay by Stationを追加しました。
    - ActiveRelay by PlayerCollisionを追加しました。
    - ActiveRelay by PlayerTriggerを追加しました。
  
  - **Changed**
    - ActiveRelay
      - 同期が不要なコンポーネントに対して、全てBehaviourSyncModeをNoneにしました。

## [1.19.0] - 2024/5/18

- **Added**
  - Active Relay
    - ActiveRelay to GameObjectを追加、これに伴い、ActiveRelay to Componentではゲームオブジェクトのアクティブ切替ができなくなりました。
    - ActiveRelay to ComponentにEvent Typeを追加、オブジェクトがアクティブに/非アクティブになった時だけ実行できるようになりました。
    - ActiveRelay to VRCComponentを追加しました。
    - ActiveRelay to ObjectSyncを追加しました。

- **Changed**  
  - AmbientEffect Assistant
    - AmbientSound Assistantから改名しました。これに伴い、General枠に追加していた一部コンポーネントをこちらに移しました。
  - Grab SlideDoor
    - Prefab内のオブジェクト名を分かりやすいようリネームしました。

## [1.18.3] - 2024/5/5

- **Added**
  - General
    - Area Cullingを追加しました。コライダーで範囲指定する形式のBoundary Cullingです。

## [1.18.2] - 2024/5/4

- **Added**
  - General
    - Boundary Cullingを追加しました。ViewPoint Trackerの対象として使うコンポーネントです。

## [1.18.1] - 2024/4/17

- **Fixed**
  - AmbientSound Assistant
    - Flexible TransformのActive Rangeが0の時、オブジェクトが常に非アクティブになってしまうのを修正しました。

## [1.18.0] - 2024/4/16

- **Added**
  - Active Relay
    - Active Relay to GameObjectを追加しました。

## [1.17.0] - 2024/4/15

- **Added**
  - AmbientSound Assistant
    - Flexible ReverbZoneとFlexible Transformを追加しました。

## [1.16.4] - 2024/4/5

- **Fixed**
  - Manual ObjectSync
    - Equip/Attach機能併用時、Pickuppableが同期しない事があるのを修正しました。

## [1.16.3] - 2024/4/3

- **Fixed**
  - Manual ObjectSync
    - オーナー権継承周りの安定性向上

## [1.16.2] - 2024/4/2

- **Fixed**
  - Manual ObjectSync
    - 1.16.1の内容をいったんロールバック

## [1.16.1] - 2024/4/2

- **Fixed**
  - Manual ObjectSync
    - 安定性を向上しました。

## [1.16.0] - 2024/4/2

- **Added**
  - Ambient Sound Assistant
    - ImpactEffectを追加しました。

## [1.15.1] - 2024/3/25

- **Fixed**
  - PlayerAudio Master
    - 安定性と、インスタンス人数が少ない時の応答性を向上させました。

## [1.15.0] - 2024/3/24

- **Added**
  - PlayerAudio Master
    - PA Regulator AvatarScaleを追加しました。

- **Fixed**
  - AdvancedWorldSettings
  - PlayerAudio Master
    - アバター音量の設定範囲をVRChatの仕様に合わせました。
  - Manual ObjectSync
    - 内部的な微修正

## [1.14.1] - 2024/3/22

- **Fixed**
  - U#のコンパイルに失敗してビルドできない不具合を修正しました。

## [1.14.0] - 2024/3/21

- **Added**
  - [Ambient Sound Assistant](https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Sound-Assistant)を追加

- **Fixed**
  - AddComponentMenuを整理しました。

## [1.13.3] - 2024/3/14

- **Fixed**
  - PlayerAudio Master
    - PlayerAudio RegulatorのFallbackが機能していなかったのを修正しました。
    - Channel Unmatch ModeのNoneをDefaultに改名しました。

## [1.13.2] - 2024/2/25

- **Fixed**
  - パッケージのAutor NameのTypo修正
  - PlayerAudio Master
    - 一斉にJoinされると機能しないことがある不具合を修正しました。

## [1.13.0] - 2024/2/2

- **Changed**
  - Manual ObjectSync
    - Owner権が移譲された際、Equipが強制的に外れるようにしました(1.12.4の内容が完全でなかったため) [#9](https://github.com/mimyquality/FukuroUdon/issues/9)

## [1.12.4] - 2024/2/1

- **Fixed**
  - Manual ObjectSync
    - オブジェクトをEquipしたプレイヤーがEquipしたまま退室した時にEquipが外れるようにしました。 [#9](https://github.com/mimyquality/FukuroUdon/issues/9)

## [1.12.3] - 2024/1/31

- **Fixed**
  - Manual ObjectSync
    - 初期状態が非アクティブまたはVRCObjectPoolなどでアクティブ状態を管理している場合に、later-joinerに初期位置が同期しないのを修正しました。 [#8](https://github.com/mimyquality/FukuroUdon/pull/8)

## [1.12.0] - 2024/1/26

- **Changed**
  - SwivelChair2
    - Input ModeにDisableを追加しました。これに合わせてTooltipのAnimatorに渡すパラメーターを変更しました。

## [1.11.2] - 2024/1/21

- **Fixed**
  - 一部コンポーネントのInspector上の名称を修正しました。
  - PlayerAudio Master
    - プレイヤー退室後に `PlayerAudioMaster_List_Sample` のリスト表記がズレるバグを修正しました。

## [1.11.0] - 2023/12/22

- **Added**
  - PlayerAudio Master
    - PlayerAudio Regulator Baseを追加しました。無条件にマッチするタイプのPA Regulatorです。
    - 全てのPlayer Audio Regulatorに `Channel Unmatch Mode` を追加しました。

- **Changed**
  - asmdefをリネームしました
  - PlayerAudio Master
    - サンプルのPlayerAudioMaster_PrivateRoom_Sampleプレハブを、PA Regulator Baseを使った形に変更しました。

## [1.10.2] - 2023/12/11

- **Fixed**
  - Manual ObjectSync
    - Unity2021以降のバージョンに対応しました。  
      Unity2019のプロジェクトからUnity2022にMigrateする前にこのバージョンにアップデートしておくことで、自動変換しなくて良くなります。

## [1.10.1] - 2023/11/23

- **Fixed**
  - PlayerAudio Master
    - 一部の内部処理をリファクタリングをしました。
    - VoiceChannelSelecterの最大切替数を10個までに拡張しました。

## [1.10.0] - 2023/11/21

- **Added**
  - PlayerAudio Master
    - サンプルに `PlayerAudioMaster_List_Sample` を追加しました。付属のメニューからリストに入れて1ch～5chを切り替えるサンプルです。

- **Fixed**
  - SwivelChair2
    - ピックアップ付き椅子にて、ピックアップ中に反対の手で座れてしまうのを修正しました。
  - PlayerAudio Master
    - PlayerAudio Regulator Areaコンポーネントをリセットした時に適用されるレイヤーをUIに修正しました。

## [1.9.7] - 2023/11/19

- **Fixed**
  - PlayerAudio Master
    - PlayerAudio Regulator AreaプレハブのレイヤーをUIにしました。

## [1.9.6] - 2023/11/19

- **Fixed**
  - SwivelChair2
    - キャスター移動とピックアップが共存できるようにしました。
    - 上記に伴い、サンプルPrefabを追加しました。
    - CasterコンポーネントにImmobileパラメーターを追加しました。これが有効な間、Casterによる移動処理が無視されます。

## [1.9.5] - 2023/11/16

- **Fixed**
  - SwivelChair2
    - VRモードで操作ガイドが表示されなかったのを修正しました。

## [1.9.4] - 2023/11/12

- **Fixed**
  - SwivelChair2
    - スマホモードでジャンプボタンの長押しが無効のため、椅子から降りる操作をジャンプボタンのダブルタップに変更しました。他のモードでは長押しのままです。

## [1.9.3] - 2023/11/12

- **Fixed**
  - SwivelChair
    - 1.9.0以降でAndroidプラットフォームに切り替えた時、エラーになっていたのを修正しました。

## [1.9.2] - 2023/11/12

- **Changed**
  - SwivelChair2
    - SwivelChair2内にあるピックアップが、座っている間掴めなくなる事があるのを修正しました。

## [1.9.1] - 2023/11/12

- **Fixed**
  - SwivelChair2
    - スイベル回転の初期値がワールド空間準拠になっていたのを修正しました。

## [1.9.0] - 2023/11/11

- **Added**
  - [SwivelChair2](https://github.com/mimyquality/FukuroUdon/wiki/Swivel-Chair-2)を追加
  - Mirror Tuner
    - Add Componentメニューに追加しました。

- **Deprecated**
  - SwivelChair
    - SwivelChair2の公開に伴い、SwivelChairは致命的な不具合を除いて更新を停止します。

## [1.8.1] - 2023/11/9

- **Fixed**
  - 名前空間が被ってエラーになる可能性があった問題に対処しました。

## [1.8.0] - 2023/11/4

- **Added**
  - [Mirror Tuner](https://github.com/mimyquality/FukuroUdon/wiki/Mirror-Tuner)を追加

## [1.7.1] - 2023/10/23

- **Fixed**
  - Grab SlideDoor
    - VRモードでPickupHandleを掴んでもLimitedPosition/LookConstraintが有効にならないバグを修正しました。

## [1.7.0] - 2023/10/23

- **Added**
  - VR Follow HUD
    - LocalPlayer Tracking Trackerコンポーネントを追加しました。

- **Changed**
  - Input Flying System
    - キー入力設定の昇降操作のデフォルトを逆(Eで上昇、Qで下降)にしました。
  - VR Follow HUD
    - VR Follow HUDコンポーネントはLocalPlayer Tracking Trackerを継承したコンポーネントになりました。

## [1.6.9] - 2023/9/24

- **Fixed**
  - Grab SlideDoor
    - PickupHandleの無駄な処理を削減しました。

## [1.6.8] - 2023/9/18

- **Fixed**
  - Grab SlideDoor
    - PickupHandleを掴んだまま動かないでいるとLimitedPosition/LookConstraintが無効になるのを修正しました。

## [1.6.7] - 2023/9/18

- **Fixed**
  - Grab SlideDoor
    - OcclusionPortalの付いたオブジェクトをセットしても、オブジェクトのアクティブも切り替わってしまうのを修正しました

## [1.6.6] - 2023/9/17

- **Fixed**
  - Grab SlideDoor
    - 大量に置いた場合の負荷軽減のため、必要な時だけPickupHandleからLimitedPosition/LookConstraintの有効無効を切り替えるようにしました。LimitedPosition/LookConstraint単体でも引き続き使えます。  
    - 内部処理のリファクタリングをしました。

## [1.6.5] - 2023/9/15

- **Fixed**
  - U#がVRCSDK-WORLDに統合される発表を受けて、依存先をVRCSDK-WORLDのみに変更しました。

## [1.6.4] - 2023/8/21

- **Changed**
  - Manual ObjectSync
    - RespawnHeightY(落下時のリスポーン基準)がVRC Scene Descriptorと連動するようになりました。

## [1.6.1] - 2023/8/20

- **Fixed**
  - Manual ObjectSync
    - Nested Prefabに付いているManual ObjectSyncが再生/ビルド時にエラーになるのを修正しました。

## [1.6.0] - 2023/8/16

- **Changed**
  - Manual ObjectSync
    - 大量に置いた場合の負荷軽減のため、更新処理を管理オブジェクトから配信する方式に変更しました。
  - Swivel Chair
    - 大量に置いた場合の負荷軽減のため、更新処理をサブコンポーネントから配信する方式に変更しました。

## [1.5.2] - 2023/8/2

- **Fixed**
  - Advanced World Settings
    - U# 1.1.9に正式対応しました
    - Avatar Eye Hieghtの設定を固定値ではなく、上限と下限を設定する方式に変更しました

## [1.5.0] - 2023/8/1

- **Added**
  - [Advanced World Settings](https://github.com/mimyquality/FukuroUdon/wiki/Advanced-World-Settings)を追加

## [1.4.4] - 2023/7/5

- **Fixed**
  - Swivel Chair
    - 最初に非アクティブだった場合の同期の安定性を向上しました。

## [1.4.3] - 2023/7/2

- **Fixed**
  - Manual ObjectSync
    - アタッチモード/装着モード切り替え時にisKinematicが切り替わらない事があるのを修正しました。

## [1.4.2] - 2023/6/30

- **Fixed**
  - Manual ObjectSync
    - VRCPickupと共に付けた場合に、Disallow Theftが機能していなかったのを修正しました。

## [1.4.1] - 2023/6/23

- **Fixed**
  - Manual ObjectSync
    - PickupHandプロパティが正しくない値を返すのを修正しました。
    - PickupEventTransferにInteract()を追加しました。これに合わせてVRCPickupを必須コンポーネントから外しました。

## [1.4.0] - 2023/6/20

- **Added**
  - Manual ObjectSync
    - 他のUdonスクリプトにピックアップ系イベントを横流しできる補助スクリプトを追加しました。  
      VRCPickupコンポーネントが付いていなくてもピックアップ系イベントを実行できるようになります。

- **Changed**
  - Manual ObjectSync
    - アタッチモード用のフィールド名を変更しました。

## [1.3.0] - 2023/6/20

- **Added**
  - Manual ObjectSync
    - アタッチモード、ボーンに装着モードを追加しました。
    - 使い方のサンプルとして、関連する補助スクリプトとサンプルプレハブも追加しました。

- **Fixed**
  - Smart Slideshow
    - 次のスライドセットのページ数が現在のページ番号より少ない場合、エラー停止する不具合を修正しました。

## [1.2.1] - 2023/6/11

- **Fixed**
  - Grab SlideDoor
    - 内部処理を見直しました。

## [1.2.0] - 2023/6/4

- **Added**
  - Manual ObjectSync
    - 同期オブジェクト(VRC ObjectSyncまたはManual ObjectSync、SmartObjectSync等の、Respawn()メソッドの存在するUdonスクリプトが付いたオブジェクト)の一括位置リセットができるスイッチを追加しました。

- **Changed**
  - Manual ObjectSync
  - GameObject Celler
    - Add Componentメニューの階層を整理しました。

## [1.1.2] - 2023/5/27

- **Added**
  - PlayerAudio Master
    - ユースケース別のサンプルプレハブを追加しました。使い方は[Wiki](https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master)を見てください。

## [1.1.1] - 2023/4/13

- **Fixed**
  - PlayerAudio Master
    - サンプルプレハブ用のマテリアルがパッケージに含まれていなかったのを修正しました。

## [1.1.0] - 2023/4/13

- **Added**
  - [PlayerAudio Master](https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master)を追加

- **Fixed**
  - SamplesフォルダーのパスがRuntime以下になっていたのを修正しました。

## [1.0.0] - 2023/4/8

- **Changed**
  - VCC 2.1.0からコミュニティーリポジトリーに対応したのに合わせて、[MimyLabリポジトリー](https://vpm.mimylab.com/)を公開しました。VCCからFukuro Udonのバージョン管理ができるようになります。
  - 上記に合わせて[導入手順](https://github.com/mimyquality/FukuroUdon)を書き直しました。

## β版 - ～2023/2/25

- **Added**
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
    - 読み込める画像はImageDownloaderの制限に準拠します。詳細は公式ドキュメント( <https://docs.vrchat.com/docs/image-loading> )を見てください。

- **Fixed**
  - Add Componentメニューにて、関連スクリプトを *Fukuro Udon* カテゴリーに表示されるようにしました。
  - Manual ObjectSync  
    - ピックアップを持ち替えた時に、他人からは持ち替えてないように見える可能性があるのを修正しました。
    - 11/3のアップデートにて、VRCPickupと併用した場合で、ピックアップしたまま退室されると他の人が掴み直せなくなる不具合に対応しました。
    - Rotationの変動チェックがMove Check Spaceを無視していたのを修正しました。
    - 初回のOwner委譲時に、Transform値が全て0になるのを修正しました。
    - 0.2.7バージョンで、later-joiner視点でjoin直後が非アクティブだと同期しなかったのを修正しました。
    - 他人がピックアップした際にオブジェクトが一瞬跳ねる現象を低減しました。

[3.14.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.14.0
[3.13.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.13.0
[3.12.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.12.0
[3.11.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.11.0
[3.10.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.10.1
[3.10.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.10.0
[3.9.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.9.0
[3.8.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.8.1
[3.8.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.8.0
[3.7.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.7.1
[3.7.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.7.0
[3.6.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.6.0
[3.5.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.5.0
[3.4.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.4.0
[3.3.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.3.0
[3.2.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.2.2
[3.2.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.2.1
[3.2.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.2.0
[3.1.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.1.1
[3.1.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.1.0
[3.0.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.0.2
[3.0.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.0.1
[3.0.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/3.0.0
[2.0.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/2.0.1
[2.0.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/2.0.0
[1.24.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.24.1
[1.24.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.24.0
[1.23.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.23.0
[1.22.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.22.1
[1.22.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.22.0
[1.21.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.21.0
[1.20.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.20.0
[1.19.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.19.0
[1.18.3]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.18.3
[1.18.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.18.2
[1.18.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.18.1
[1.18.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.18.0
[1.17.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.17.0
[1.16.4]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.16.4
[1.16.3]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.16.3
[1.16.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.16.2
[1.16.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.16.1
[1.16.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.16.0
[1.15.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.15.1
[1.15.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.15.0
[1.14.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.14.1
[1.14.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.14.0
[1.13.3]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.13.3
[1.13.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.13.2
[1.13.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.13.0
[1.12.4]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.12.4
[1.12.3]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.12.3
[1.12.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.12.0
[1.11.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.11.2
[1.11.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.11.0
[1.10.2]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.10.2
[1.10.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.10.1
[1.10.0]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.10.0
[1.9.7]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.9.7
[1.9.6]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.9.6
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
[1.6.1]: https://github.com/mimyquality/FukuroUdon/releases/tag/1.6.1
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
