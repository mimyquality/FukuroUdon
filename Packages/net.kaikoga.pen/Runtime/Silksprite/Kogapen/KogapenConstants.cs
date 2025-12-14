namespace Silksprite.Kogapen
{
    public static class KogapenConstants
    {
        public const int MaxPlayerCount = 82;

        // > Manual sync is limited to roughly 49 Kilobytes per serialization.
        public const int MaxPacketSize = 2048;
        public const int HeaderSize = 3;
        
        // Header format:
        // [ ]       | x        | y        | z        | 
        // [0]    Id | command  | playerId | strokeId |
        // [1]  Size | newSize  | writeOfs | writeLen |
        // [2] Color | stroke.r | stroke.g | stroke.b |
        // [i]  Body | s[i].x   | s[i].y   | s[i].z   |

        public const int IdOffsetFromHead = 0;
        public const int SizeOffsetFromHead = 1;
        public const int ColorOffsetFromHead = 2;
        
        public const int IdOffsetFromBody = IdOffsetFromHead - HeaderSize;
        public const int SizeOffsetFromBody = SizeOffsetFromHead - HeaderSize;
        public const int ColorOffsetFromBody = ColorOffsetFromHead - HeaderSize;
    }
}