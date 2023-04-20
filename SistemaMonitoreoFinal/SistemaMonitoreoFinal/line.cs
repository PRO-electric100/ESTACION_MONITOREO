using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMonitoreoFinal
{
    class Line
    {
        public Color color;
        public float width;
        public decimal[] from = new decimal[4];
        public decimal[] to = new decimal[4];
        public decimal[,] matrix = FactoryMatrix.getIdentity();

        public Line()
        {
            from[0] = 0;
            from[1] = 0;
            from[2] = 0;
            from[3] = 1;
            to[0] = 0;
            to[1] = 0;
            to[2] = 0;
            to[3] = 1;
        }
    }
}
