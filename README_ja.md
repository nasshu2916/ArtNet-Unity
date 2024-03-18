# ArtNet-Unity

Unity(C#) で ArtNet を扱うためのライブラリです。ArtNet のパケットを受信し、Callback から受信したデータを処理することができます。

エディタ拡張から ArtNet の受信の確認状況の確認ができます。また、受信したパケットを保存し、送信することも可能です。

![dmx_receive](Docs/dmx_receive.gif)

## インストール方法

Unity Package Manager でインストールできます。

1. Unity の `Window` > `Package Manager` を開きます。
2. `+` ボタンをクリックし、`Add package from git URL` を選択します。
3. 以下の URL を入力し、`Add` ボタンをクリックします。

```
https://github.com/nasshu2916/ArtNet-Unity.git?path=/Assets/ArtNet#master
```

> [!NOTE]
> Unity 2021.3.1 以上が必要です。

## 使用方法

![artnet_receiver](Docs/artnet_receiver.png)

1. `ArtNet` プレハブをシーンに追加するか、`ArtNetReceiver` を GameObject に追加します。
2. `ArtNetReceiver` の callback プロパティに受信したデータを処理するスクリプトを設定します。
3. `ArtNetReceiver` の `autoStart` プロパティを有効にするか、`StartReceive` メソッドを呼び出します。

## Support OpCode

- OpPoll
- OpPollReply
- OpDmx

## エディタ拡張の機能
### ArtNetTester

Game を実行せずに ArtNet の受信状況を確認するためのエディタ拡張です。
Window 表示時は ArtNet のパケットを受信しないため、 `Start Receive ArtNet Packet` ボタンを押すことで受信を開始します。

このエディタ拡張では、最後に ArtNet のパケットを受信した時刻と OP Code の内容を確認できます。
また、Universe 単位で最後に受信した DMX の値も確認できます。

### DmxRecorder

Game を実行せずに ArtNet のパケットを保存、送信することができるエディタ拡張です。

#### Recorder タブ
Recorder タブでは、受信可能な ArtNet のパケットをバイナリデータとして保存することができます。

出力先のファイル名とフォルダを指定し、`Start` ボタンを押すと、受信した ArtNet のパケットを保存します。
既に同じ名前のファイルが存在する場合、自動的に上書きされます。

#### Sender タブ
Sender タブでは、Recorder で保存したファイルを送信することができます。

出力先の IP アドレスを指定し、`Start` ボタンを押すと、指定した IP アドレスに ArtNet のパケットを送信します。
また、以下の設定を行うことができます。

- ループで送信するかどうか
- 保存時の Sequence を使用するかどうか
- 送信速度

### DmxManagerViewer

DmxManager Class で管理している DMX の値を確認するためのエディタ拡張です。このエディタ拡張は Game を実行している時のみ有効です。

## Test Software

- QLC+
- MagicQ
- dot2 on PC
