using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteKeeper.DataLayer.Exceptions
{
    [Serializable]
    public class CreateRelationException : Exception, ISerializable
    {
        public string RelationName { get; set; }
        public string FirstItemTypeName { get; set; }
        public string SecondItemTypeName { get; set; }
        public Guid FirstItemId { get; set; }
        public Guid SecondItemId { get; set; }

        public CreateRelationException() { }

        public CreateRelationException(string message) : base(message) { }

        public CreateRelationException(string message, Exception innerException) : base(message, innerException) { }

        protected CreateRelationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            RelationName = info.GetString("RelationName");
            FirstItemTypeName = info.GetString("FirstItemTypeName");
            SecondItemTypeName = info.GetString("SecondItemTypeName");
            FirstItemId = (Guid)info.GetValue("FirstItemId", typeof(Guid));
            SecondItemId = (Guid)info.GetValue("SecondItemId", typeof(Guid));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("RelationName", RelationName);
            info.AddValue("FirstItemTypeName", FirstItemTypeName);
            info.AddValue("SecondItemTypeName", SecondItemTypeName);
            info.AddValue("FirstItemId", FirstItemId);
            info.AddValue("SecondItemId", SecondItemId);
        }
    }
}
