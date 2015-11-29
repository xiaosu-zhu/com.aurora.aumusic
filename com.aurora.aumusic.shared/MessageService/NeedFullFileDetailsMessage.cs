using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class NeedFullFileDetailsMessage
    {
        [DataMember]
        public string MainKey;

        public NeedFullFileDetailsMessage(string mainkey)
        {
            this.MainKey = mainkey;
        }
    }
}
