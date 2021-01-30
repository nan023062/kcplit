using System;
using System.Collections.Generic;

namespace Nave.Network
{
    public interface IProperty
    {
        string name { get; }

        /// <summary>
        /// 读取缓冲去数据，并更新数据
        /// </summary>
        void ReadBufferAndChangeValues(Codec c);

        /// <summary>
        /// 检查数据变化，并编码到缓冲区
        /// </summary>
        bool CheckChangedAndWriteToBuffer(Codec c);
    }
}
