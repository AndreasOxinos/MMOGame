using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Base
{
    public enum GameOperationResponse : byte
    {
        Invalid, 
        Error,
        FataError,
        Success
    }
}
