using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nave.Network
{
    public interface ICodecable
    {
        void Decode(Codec codec);

        void Encode(Codec codec);
    }
}
