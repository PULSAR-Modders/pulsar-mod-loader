using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.MPModChecks
{
    public class MPUserDataBlock
    {
        public MPUserDataBlock(string PMLVersion, MPModDataBlock[] ModData)
        {
            this.PMLVersion = PMLVersion;
            this.ModData = ModData;
        }

        public MPUserDataBlock()
        {
            this.PMLVersion = string.Empty;
            this.ModData = null;
        }

        public string PMLVersion { get; }
        public MPModDataBlock[] ModData { get; }
    }
}
