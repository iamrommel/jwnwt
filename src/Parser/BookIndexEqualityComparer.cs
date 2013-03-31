using System.Collections.Generic;

namespace Parser
{
    public class BookIndexEqualityComparer : IEqualityComparer<BookIndex>
    {

        public bool Equals(BookIndex b1, BookIndex b2)
        {
            if (b1.Code == b2.Code)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public int GetHashCode(BookIndex bx)
        {
            //int hCode =  bx.Code.GetHashCode() ^  bx.UpperChapter ^ bx.UpperVerse;
            return bx.Code.GetHashCode();
        }

    }
}