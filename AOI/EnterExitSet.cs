using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOI
{
    class EnterExitSet
    {
        public Dictionary<Sprite, int> set = new Dictionary<Sprite, int>();

        public void Enter(Sprite sp)
        {
            if (set.ContainsKey(sp))
            {
                set[sp]++;
            }
            else
            {
                set.Add(sp, 1);
            }
        }

        public void Exit(Sprite sp)
        {
            if (set.ContainsKey(sp))
            {
                set[sp]--;
            }
            else
            {
                set.Add(sp, -1);
            }
        }

        public void Clear()
        {
            set.Clear();
        }
    }
}
