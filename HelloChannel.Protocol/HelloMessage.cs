using System.Text;

namespace HelloChannel.Protocol;

/// <summary>
/// Message types for the Hello virtual channel.
/// </summary>
public enum MessageType : byte
{
    /// <summary>Server (VM) → Client (Host Desktop)</summary>
    Hello = 1,

    /// <summary>Client (Host Desktop) → Server (VM)</summary>
    Hi = 2
}

/// <summary>
/// Binary message format for the HELLO virtual channel.
/// Wire format: [payloadLength:u32][messageType:u8][textLength:u32][textBytes...]
/// payloadLength = 1 (type) + 4 (textLength) + textBytes.Length
/// </summary>
public class HelloMessage
{
    public MessageType Type { get; set; }
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Serializes this message into a byte array with length-prefixed framing.
    /// </summary>
    public byte[] Serialize()
    {
        byte[] textBytes = Encoding.UTF8.GetBytes(Text);
        uint payloadLen = (uint)(1 + 4 + textBytes.Length);

        byte[] buffer = new byte[4 + payloadLen];
        int offset = 0;

        // Payload length (everything after this field)
        BitConverter.GetBytes(payloadLen).CopyTo(buffer, offset);
        offset += 4;

        // Message type
        buffer[offset] = (byte)Type;
        offset += 1;

        // Text length
        BitConverter.GetBytes((uint)textBytes.Length).CopyTo(buffer, offset);
        offset += 4;

        // Text bytes
        textBytes.CopyTo(buffer, offset);

        return buffer;
    }

    /// <summary>
    /// Deserializes a full framed message (including the 4-byte length prefix).
    /// </summary>
    public static HelloMessage Deserialize(byte[] data)
    {
        if (data.Length < 9)
            throw new ArgumentException("Data too short for a valid framed message.");

        int offset = 4; // skip payload length
        return DeserializePayload(data, offset, data.Length - 4);
    }

    /// <summary>
    /// Deserializes just the payload portion (without the length prefix).
    /// Used by the DVC client callback which receives raw payload bytes.
    /// </summary>
    public static HelloMessage DeserializePayload(byte[] data, int startOffset, int length)
    {
        if (length < 5)
            throw new ArgumentException("Payload too short for a valid message.");

        int offset = startOffset;

        var type = (MessageType)data[offset];
        offset += 1;

        uint textLen = BitConverter.ToUInt32(data, offset);
        offset += 4;

        string text = Encoding.UTF8.GetString(data, offset, (int)textLen);

        return new HelloMessage { Type = type, Text = text };
    }

    public override string ToString() => $"[{Type}] {Text}";
}
