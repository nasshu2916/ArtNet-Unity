# ArtNet-Unity ChangeLog

## Unreleased

### Added

- ArtTimeCode (OpTimeCode) パケットの受信・デシリアライズ・シリアライズ対応を追加
- ArtSync(OpSync) パケットの受信・デシリアライズ・シリアライズ対応を追加
- ArtAddress (OpAddress) パケットの受信・デシリアライズ・シリアライズ対応を追加
- ArtTodRequest (OpTodRequest) パケットの受信・デシリアライズ・シリアライズ対応を追加
- ArtTodData (OpTodData) パケットの受信・デシリアライズ・シリアライズ対応を追加
- ArtTodControl (OpTodControl) パケットの受信・デシリアライズ・シリアライズ対応を追加
- ArtRdm (OpRdm) パケットの受信・デシリアライズ・シリアライズ対応を追加

## v0.2.0

### Added

- UnityRecorder に ArtNet 保存する拡張機能を追加
- Recorder/Player の設定を ScriptableObject で保存する機能追加
- Recorder の保存形式に AnimationClip を追加
- Recorder のバイナリ形式に圧縮オプションを追加
- Recorder 保存時にフレームを間引くオプションを追加
- Recorder 保存先を Wildcard で指定可能に変更
- ユニットテストの導入
- 独自の Logger クラスを追加

### Changed

- Recorder/Player を別 Window に分離
- Universe を 0 始まりに統一する

## v0.1.3

### Added

- DMX Recorder/Player のエディタ拡張を追加

## v0.1.2

### Added

- ArtNet Tester を追加

### Changed

- ArtNet パケットの受信処理のリファクタリング

## v0.1.1

### Changed

- DMX Viewer を UIToolkit に変更
