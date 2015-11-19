using System.Runtime.Serialization;

namespace com.aurora.aumusic.shared.Lrc
{
    [DataContract]
    public class LrcUrlModel
    {
        [DataMember]
        public int aid;
        [DataMember]
        public int artist_id;
        [DataMember]
        public int sid;
        [DataMember]
        public string lrc;
        [DataMember]
        public string song;
    }
}