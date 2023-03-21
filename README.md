# ArtNet-Unity

A tool to receive ArtNet in Unity(C#).
Receive DMX512 via ArtNet.

![dmx_receive](Docs/dmx_receive.gif)

## Install

Install via Unity Package Manager.

※ Required Unity version is 2021.3.1 or later.

```
https://github.com/nasshu2916/ArtNet-Unity.git?path=/Assets/ArtNet#v0.1.1
```

or develop branch

```
https://github.com/nasshu2916/ArtNet-Unity.git?path=/Assets/ArtNet#develop
```

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
