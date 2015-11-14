using com.aurora.aumusic.shared.Albums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class RefreshStateMessage
    {
        
        public RefreshStateMessage(RefreshState refresh)
        {
            this.Refresh = refresh;
        }

        [DataMember]
        public RefreshState Refresh;
    }
}
