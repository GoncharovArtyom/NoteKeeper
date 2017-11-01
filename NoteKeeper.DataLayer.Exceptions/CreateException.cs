using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;

namespace NoteKeeper.DataLayer.Exceptions
{
    [Serializable]
    public class CreateException<TItem> : Exception, ISerializable
    {
        public TItem Item { get; set; }

        public CreateException() { }

        public CreateException(string message) : base(message) { }

        public CreateException(string message, Exception innerException) : base(message, innerException) { }

        protected CreateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Item = (TItem)info.GetValue("Item", typeof(TItem));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Item", Item);
        }
    }
}
