using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class FileNotFindMessage
    {
        [DataMember]
        public string MainKey;

        public FileNotFindMessage(string mainkey)
        {
            MainKey = mainkey;
        }
    }
}
