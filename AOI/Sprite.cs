using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOI
{
    class Sprite
    {
        public int id;
        public int sight;//视野
        public Brace x, y, left, right, up, down;
        public List<Sprite> views;

        public Rectangle rect;
    }
}
