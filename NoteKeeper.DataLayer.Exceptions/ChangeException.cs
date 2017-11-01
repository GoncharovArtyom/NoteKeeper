using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NoteKeeper.DataLayer.Exceptions
{
    [Serializable]
    public class ChangeException<TValue>:Exception, ISerializable
    {
        public Guid Id { get; set; }
        public string TypeName { get; set; }
        public string FieldName { get; set; }
        public TValue NewValue { get; set; }

        public ChangeException() { }

        public ChangeException(string message) : base(message) { }

        public ChangeException(string message, Exception innerException) : base(message, innerException) { }

        protected ChangeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Id = (Guid)info.GetValue("Id", typeof(Guid));
            TypeName = info.GetString("TypeName");
            FieldName = info.GetString("FieldName");
            NewValue = (TValue)info.GetValue("NewValue", typeof(TValue));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Id", Id);
            info.AddValue("TypeName", TypeName);
            info.AddValue("FieldName", FieldName);
            info.AddValue("NewVAlue", NewValue);
        }
    }
    
}
