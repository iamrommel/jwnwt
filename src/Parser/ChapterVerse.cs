using System.Runtime.Serialization;

namespace Parser
{
    [DataContract]
    public class ChapterVerse
    {
        [DataMember(Name = "cpc")]
        public int Chapter { get; set; }


        [DataMember(Name = "cpu")]
        public int UpperVerse { get { return 20; } set { } }
    }
}