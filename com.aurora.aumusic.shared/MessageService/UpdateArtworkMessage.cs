using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class UpdateArtworkMessage
    {
        [DataMember]
        public byte[] ByteStream;

        public UpdateArtworkMessage(byte[] stream)
        {
            ByteStream = stream;
        }
    }
}
