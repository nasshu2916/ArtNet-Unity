# ArtNet-Unity

Unity(C#) で ArtNet を扱うためのライブラリです。

エディタ拡張として、ArtNet の受信状況の確認や ArtNet DMX パケットの保存、送信を行うことができる機能もあります。

![dmx_receive](https://github.com/user-attachments/assets/08afbbf8-4892-478c-9feb-4b8f74832e6d)

https://github.com/nasshu2916/ArtNet-Unity/assets/13119954/36851801-4f0a-4a2a-98aa-0b4659588a59

## インストール方法

Unity Package Manager でインストールできます。

1. Unity の `Window` > `Package Manager` を開きます。
2. `+` ボタンをクリックし、`Add package from git URL` を選択します。
3. 以下の URL を入力し、`Add` ボタンをクリックします。

```
https://github.com/nasshu2916/ArtNet-Unity.git?path=/Assets/ArtNet#master
```

> [!NOTE]
> Unity 2021.3.1 (C# 9) 以降が必要です。

## 使用方法

<img width="400" alt="artnet_receiver" src="https://github.com/user-attachments/assets/7823e286-f0c5-4135-ac4f-94f40caa843c" />


1. `ArtNet` プレハブをシーンに追加するか、`ArtNetReceiver` を GameObject に追加します。
2. `ArtNetReceiver` の callback プロパティに受信したデータを処理するスクリプトを設定します。
3. `ArtNetReceiver` の `autoStart` プロパティを有効にするか、`StartReceive` メソッドを呼び出します。

## Support OpCode

- OpPoll
- OpPollReply
- OpDmx
- OpSync
- OpAddress
- OpTodRequest
- OpTodData
- OpTodControl
- OpRdm
- OpTimeCode

## エディタ拡張

### ArtNetTester

![dmx_tester](https://github.com/user-attachments/assets/9d48aae9-6909-40c6-8377-660fe97b3b7b)

Editor を再生せずに ArtNet の受信状況の確認ができるエディタ拡張です。

Window 表示時は ArtNet のパケットを受信していません。
そのため、`Start Receive ArtNet Packet` ボタンを押すことで受信させる必要があります。

このエディタ拡張では、最後に ArtNet パケットを受信した時刻と OP Code の内容を確認できます。
また、Universe 単位で最後に受信した DMX の値も確認できます。

### DmxRecorder

![dmx_recorder](https://github.com/user-attachments/assets/bc99d798-23af-41bc-82f6-445458ff9949)

Editor を再生せずに ArtNet のパケットを保存できるエディタ拡張です。

録画開始ボタンを押すことで、ArtNet パケットの受信と保存が開始され、停止ボタンを押すまでの間に受信した ArtNet パケットを
Animation Clip か Binary 形式で保存できます。

録画した Binary ファイルは [独自形式](Docs/BinaryFormat_ja.md) で保存され、DmxPlayer を使うことで録画した ArtNet
パケットを送信することが可能です。

録画した Animation Clip は Animation コンポーネントを使って再生することができ、Binary ファイルはエディタ拡張の DmxPlayer を使って再生することができます。

### DmxPlayer

![dmx_player](https://github.com/user-attachments/assets/a5c51fa4-c0d0-4964-9db3-abb32c4601df)


DmxRecorder で保存した ArtNet パケットを送信するためのエディタ拡張です。

DmxRecorder で保存した ArtNet パケットを再送信する機能で、ループ再生や再生速度の変更が可能です。
送信先に関しては、複数の IP アドレス・ポートを指定することが可能です。

### DmxManagerViewer

DmxManager Class で管理している DMX の値を確認するためのエディタ拡張です。このエディタ拡張は Game を実行している時のみ有効です。

### Unity Recorder カスタムソース

[Unity Recorder](https://docs.unity3d.com/Packages/com.unity.recorder@3.0/manual/index.html)
をインストールしている場合、Unity Recorder のカスタムソースとして受信した ArtNet パケットを保存する機能が追加されます。

![dmx_receive](https://github.com/user-attachments/assets/32ae8407-9963-4b37-9c4b-2ef010509e51)

指定した DmxManager が受信した ArtNet パケットを Unity Recorder で保存することができます。
保存形式と保存オプションは [DmxRecorder](#dmxrecorder) と同じで、Binary 形式と Animation Clip 形式で保存することができます。
