using VRC.SDK3.Data;
using VRC.Udon.Serialization.OdinSerializer;


[assembly: RegisterFormatter(typeof(DataTokenFormatter))]
public class DataTokenFormatter : BaseFormatter<DataToken>
{
    //DataTokenFormatter allows for Udon to serialize Data containers in field initializers and embed inside Udon assembly.
    //This class exists in both the SDK project and the client project. Both versions should be identical and any changes to one needs to be duplicated to the other.

    private static readonly Serializer<object> _referenceReaderWriter = Serializer.Get<object>();
    
    protected override void DeserializeImplementation(ref DataToken value, IDataReader reader)
    {
        reader.ReadByte(out byte _type);
        TokenType Type = (TokenType)_type;
        switch (Type)
        {
            case TokenType.Boolean:
            {
                reader.ReadBoolean(out bool innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.SByte:
            {
                reader.ReadSByte(out sbyte innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.Byte:
            {
                reader.ReadByte(out byte innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.Short:
            {
                reader.ReadInt16(out short innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.UShort:
            {
                reader.ReadUInt16(out ushort innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.Int:
            {
                reader.ReadInt32(out int innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.UInt:
            {
                reader.ReadUInt32(out uint innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.Long:
            {
                reader.ReadInt64(out long innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.ULong:
            {
                reader.ReadUInt64(out ulong innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.Float:
            {
                reader.ReadSingle(out float innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.Double:
            {
                reader.ReadDouble(out double innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.String:
            {
                reader.ReadString(out string innerValue);
                value = new DataToken(innerValue);
                break;
            }
            case TokenType.DataDictionary:
            {
                value = new DataToken((DataDictionary)_referenceReaderWriter.ReadValue(reader));
                break;
            }
            case TokenType.DataList:
            {
                value = new DataToken((DataList)_referenceReaderWriter.ReadValue(reader));
                break;
            }
            case TokenType.Reference:
            {
                value = new DataToken(_referenceReaderWriter.ReadValue(reader));
                break;
            }
            case TokenType.Error:
            {
                reader.ReadByte(out byte innerValue);
                value = new DataToken((DataError)innerValue);
                break;
            }
        }
    }

    protected override void SerializeImplementation(ref DataToken value, IDataWriter writer)
    {
        writer.WriteByte("_type", (byte)value.TokenType);
        switch (value.TokenType)
        {
            case TokenType.Boolean:
            {
                writer.WriteBoolean("_boolean", value.Boolean);
                break;
            }
            case TokenType.SByte:
            {
                writer.WriteSByte("_sbyte", value.SByte);
                break;
            }
            case TokenType.Byte:
            {
                writer.WriteByte("_byte", value.Byte);
                break;
            }
            case TokenType.Short:
            {
                writer.WriteInt16("_short", value.Short);
                break;
            }
            case TokenType.UShort:
            {
                writer.WriteUInt16("_ushort", value.UShort);
                break;
            }
            case TokenType.Int:
            {
                writer.WriteInt32("_int", value.Int);
                break;
            }
            case TokenType.UInt:
            {
                writer.WriteUInt32("_uint", value.UInt);
                break;
            }
            case TokenType.Long:
            {
                writer.WriteInt64("_long", value.Long);
                break;
            }
            case TokenType.ULong:
            {
                writer.WriteUInt64("_ulong", value.ULong);
                break;
            }
            case TokenType.Float:
            {
                writer.WriteSingle("_float", value.Float);
                break;
            }
            case TokenType.Double:
            {
                writer.WriteDouble("_double", value.Double);
                break;
            }
            case TokenType.String:
            {
                writer.WriteString("_string", value.String);
                break;
            }
            case TokenType.DataDictionary:
            {
                _referenceReaderWriter.WriteValue(value.DataDictionary, writer);
                break;
            }
            case TokenType.DataList:
            {
                _referenceReaderWriter.WriteValue(value.DataList, writer);
                break;
            }
            case TokenType.Reference:
            {
                _referenceReaderWriter.WriteValue(value.Reference, writer);
                break;
            }
            case TokenType.Error:
            {
                writer.WriteByte("_error", (byte)value.Error);
                break;
            }
        }
    }
}