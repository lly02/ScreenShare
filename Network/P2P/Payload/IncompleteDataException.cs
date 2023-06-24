using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShare.Network.P2P.Payload
{
    public class IncompleteDataException : Exception
    {
        public IncompleteDataException() 
        {
        }

        public IncompleteDataException(string message) 
            : base(message) 
        { 
        }

        public IncompleteDataException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
