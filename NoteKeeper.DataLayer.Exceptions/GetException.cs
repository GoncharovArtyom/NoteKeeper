using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NoteKeeper.DataLayer.Exceptions
{
    public class GetException:Exception, ISerializable
    {
        public string ItemName { get; set; }

        public GetException() { }
        public GetException(string message):base(message) { }
        public GetException(string message, Exception innerEx):base(message, innerEx) { }

        protected GetException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ItemName = info.GetString("ItemName");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ItemName", ItemName);
        }
    }
}
