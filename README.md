# ArtNet-Unity

A tool to receive ArtNet in Unity(C#).
DMX data can be received, recorded, and played back.

[日本語](./README_ja.md)

![dmx_receive](Docs/dmx_receive.gif)

## Install

You can install it with Unity Package Manager.

1. Open `Window` > `Package Manager` in Unity.
2. Click the `+` button and select `Add package from git URL`.
3. Enter the following URL and click the `Add` button.

```
https://github.com/nasshu2916/ArtNet-Unity.git?path=/Assets/ArtNet#master
```

> [!NOTE]
> Unity 2021.3.1 or later is required.

## Usage

![artnet_receiver](Docs/artnet_receiver.png)

1. Add `ArtNet` prefab to your scene or `ArtNetReceiver` to GameObject.
2. set your script to `ArtNetReceiver`'s callback property. (select `Editor or Runtime`)
3. start `ArtNetReceiver`'s `autoStart` property or call `StartReceive` method.

## Support OpCode

- OpPoll
- OpPollReply
- OpDmx

## Test Software

- QLC+
- MagicQ
- dot2 on PC
