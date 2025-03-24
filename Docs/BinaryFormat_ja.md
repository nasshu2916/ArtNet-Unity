## ArtNet バイナリフォーマット

ArtNet Recorder で保存するデータは独自のバイナリフォーマットで保存されます。

保存されるバイナリデータは Header と Body に分かれており、Header にはバイナリの識別情報やバージョン情報が、Body
には実際のデータが記録される。

### Header

Header 部は以下の 16 byte で構成されます。

| Field | Name         | Size | Description                          |
|-------|--------------|------|--------------------------------------|
| 1     | ID[4]        | int8 | 識別子 `\0xFF, 0x44, 0x4D, 0x58` の4文字固定 |
| 2     | Version      | int8 | バイナリの Encode Version (現在は `0x02`)    |
| 3     | Reserved[11] | int8 | 予約領域 (0x00 で埋める)                     |

### Body

Body は以下のフォーマットで DMX データをファイルの終端まで繰り返し記録する。

| Field | Name      | Size   | Description                             |
|-------|-----------|--------|-----------------------------------------|
| 1     | Timestamp | int64  | データのタイムスタンプ(ミリ秒)                        |
| 2     | Universe  | int16  | Universe の番号                            |
| 3     | Length    | int16  | DMX データの長さ                              |
| 4     | Data      | int8[] | DMX データ。長さは \"Field 3 の Length\" で指定される |

> [!NOTE]
> Body で記録する DMX データ長は Length の field で指定され、固定長ではない。
