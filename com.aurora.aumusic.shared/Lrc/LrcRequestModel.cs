using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.Lrc
{
    [DataContract]
    public class LrcRequestModel
    {
        [DataMember]
        public int count;
        [DataMember]
        public int code;
        [DataMember]
        public LrcUrlModel[] result;

    }
}
