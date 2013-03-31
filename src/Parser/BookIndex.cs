using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Parser
{
    [DataContract]
    public class BookIndex
    {
        [DataMember(Name = "c")]
        public string Code { get; set; }

        [DataMember(Name = "n")]
        public string Name { get; set; }

        [DataMember(Name = "mcb")]
        public int MaximumChapterOfBook { get; set; }

        [DataMember(Name = "cp")]
        public int Chapter { get; set; }

        [DataMember(Name = "mvc")]
        public int MaximumVerseOfChapter { get; set; }

        [DataMember(Name = "to")]
        public int TraditionalOrder { get; set; }
         
    }
}