using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class FullFileDetailsMessage
    {
        [DataMember]
        public string MusicType;
        [DataMember]
        public ulong Size;
        [DataMember]
        public uint BitRate;

        public FullFileDetailsMessage(string type, ulong size, uint bitrate)
        {
            MusicType = type;
            Size = size;
            BitRate = bitrate;
        }
    }
}
