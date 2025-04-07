## ArtNet Binary Format

The data saved by ArtNet Recorder is saved in a proprietary binary format.

The saved binary data is divided into Header and Body, with the Header containing binary identification information and
version information, and the Body containing the actual data.

### Header Section

The Header consists of the following 16 bytes.

| Field | Name           | Size | Description                                                                                                                      |
|-------|----------------|------|----------------------------------------------------------------------------------------------------------------------------------|
| 1     | ID[4]          | int8 | Identifier, 4 characters fixed to `\0xFF, 0x44, 0x4D, 0x58`                                                                      |
| 2     | Version        | int8 | Binary Encode Version (currently `0x02`)                                                                                         |
| 3     | CompressType   | int8 | Compression format of the Body section <br/> - 0x00: Uncompressed<br/> - 0x01: Deflate                                           |
| 4     | BodyEncodeType | int8 | Body Encode Type <br/> - 0x00: DMX<br/> - 0x00: DMX Packet (only supported for Deserialize)<br/> - 0x03: Universe And DMX Values |
| 5     | Reserved[9]    | int8 | Reserved area (filled with 0x00)                                                                                                 |

### Body Section

The Body records DMX data in the following format until the end of the file.

| Field | Name      | Size   | Description                                           |
|-------|-----------|--------|-------------------------------------------------------|
| 1     | Timestamp | int64  | Data timestamp (milliseconds)                         |
| 2     | Universe  | int16  | Universe number                                       |
| 3     | Length    | int16  | Length of DMX data                                    |
| 4     | Data      | int8[] | DMX data. The length is specified by "Field 3 Length" |

> [!NOTE]
> The length of the DMX data recorded in the Body is specified by the Length field and is not fixed.

> [!NOTE]
> The Body section is compressed in the compression format specified by the CompressType field of the Header. Therefore,
> appropriate compression/decompression processing is required during Serialize/Deserialize.
