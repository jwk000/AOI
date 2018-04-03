using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AOI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
        }


        Point XAxisBegin = new Point(2, 2);
        Point XAxisEnd = new Point(2, 2);
        Point YAxisBegin = new Point(2, 2);
        Point YAxisEnd = new Point(2, 2);

        List<Sprite> all = new List<Sprite>();
        LinkedList<Brace> listX = new LinkedList<Brace>();
        LinkedList<Brace> listY = new LinkedList<Brace>();

        void AddSprite(int id, int sight, int x, int y)
        {
            Sprite sp = new Sprite();

            sp.id = id;
            sp.sight = sight;
            sp.x = new Brace { pos = x, braceType = BraceType.center, owner = sp };
            sp.y = new Brace { pos = y, braceType = BraceType.center, owner = sp };
            sp.left = new Brace { pos = x - sight, braceType = BraceType.open, owner = sp };
            sp.right = new Brace { pos = x + sight, braceType = BraceType.close, owner = sp };
            sp.up = new Brace { pos = y - sight, braceType = BraceType.open, owner = sp };
            sp.down = new Brace { pos = y + sight, braceType = BraceType.close, owner = sp };
            sp.views = new List<Sprite>();
            sp.views.Add(sp);
            sp.rect = new Rectangle(sp.left.pos, sp.up.pos, 2 * sight, 2 * sight);

            all.Add(sp);
            AddX(sp.x);
            AddX(sp.left);
            AddX(sp.right);
            AddY(sp.y);
            AddY(sp.up);
            AddY(sp.down);

            foreach (var one in all)
            {
                if (one == sp) continue;
                //sp在one的视野
                if (sp.x.pos > one.left.pos &&
                    sp.x.pos < one.right.pos &&
                    sp.y.pos < one.down.pos &&
                    sp.y.pos > one.up.pos)
                {
                    one.views.Add(sp);
                }
                //one在sp的视野
                if (one.x.pos > sp.left.pos &&
                    one.x.pos < sp.right.pos &&
                    one.y.pos < sp.down.pos &&
                    one.y.pos > sp.up.pos)
                {
                    sp.views.Add(one);
                }
            }

            Invalidate();
        }

        void AddX(Brace b)
        {
            if (listX.Count == 0)
            {
                listX.AddFirst(b);
            }
            else
            {
                var n = listX.First;
                while (n != null)
                {
                    if (b.pos < n.Value.pos)
                    {
                        break;
                    }
                    n = n.Next;
                }
                if (n == null)
                {
                    listX.AddLast(b);
                }
                else
                {
                    listX.AddBefore(n, b);
                }
            }
        }

        void AddY(Brace b)
        {
            if (listY.Count == 0)
            {
                listY.AddFirst(b);
            }
            else
            {
                var n = listY.First;
                while (n != null)
                {
                    if (b.pos < n.Value.pos)
                    {
                        break;
                    }
                    n = n.Next;
                }
                if (n == null)
                {
                    listY.AddLast(b);
                }
                else
                {
                    listY.AddBefore(n, b);
                }
            }
        }

        void MoveSprite(Sprite sp, int x, int y)
        {
            MoveX(sp, x);
            MoveY(sp, y);
            Invalidate();
        }

        EnterExitSet otherSet = new EnterExitSet();
        EnterExitSet mySet = new EnterExitSet();
        void MoveX(Sprite sp, int x)
        {
            if (x == sp.x.pos) return;
            if (x < sp.x.pos)
            {
                //左移
                var c = listX.Find(sp.x);
                for (var n = c.Previous; n != null && n.Value.pos > x; n = c.Previous)
                {
                    if (sp.y.pos > n.Value.owner.up.pos && sp.y.pos < n.Value.owner.down.pos)
                    {
                        if (n.Value.braceType == BraceType.close)
                        {
                            //移入
                            otherSet.Enter(n.Value.owner);
                        }
                        if (n.Value.braceType == BraceType.open)
                        {
                            //移除
                            otherSet.Exit(n.Value.owner);
                        }
                    }
                    listX.Remove(c);
                    c = listX.AddBefore(n, sp.x);
                }
            }
            else
            {
                //右移
                var c = listX.Find(sp.x);
                for (var n = c.Next; n != null && n.Value.pos < x;)
                {
                    if (sp.y.pos > n.Value.owner.up.pos && sp.y.pos < n.Value.owner.down.pos)
                    {
                        if (n.Value.braceType == BraceType.open)
                        {
                            //移入
                            otherSet.Enter(n.Value.owner);
                        }
                        if (n.Value.braceType == BraceType.close)
                        {
                            //移除
                            otherSet.Exit(n.Value.owner);
                        }
                    }
                    listX.Remove(c);
                    c = listX.AddAfter(n, sp.x);
                    n = c.Next;
                }
            }

            foreach (var kv in otherSet.set)
            {
                if (kv.Value > 0)
                {
                    kv.Key.views.Add(sp);
                }
                else
                {
                    kv.Key.views.Remove(sp);
                }
            }
            otherSet.Clear();

            //open
            var cleft = listX.Find(sp.left);
            int newleft = sp.left.pos + x - sp.x.pos;
            if (newleft < sp.left.pos)
            {
                for (var n = cleft.Previous; n != null && n.Value.pos > newleft; n = cleft.Previous)
                {
                    if (n.Value.braceType == BraceType.center && n.Value.owner.y.pos > sp.up.pos && n.Value.owner.y.pos < sp.down.pos)
                    {
                        //移入
                        mySet.Enter(n.Value.owner);
                    }
                    listX.Remove(cleft);
                    cleft = listX.AddBefore(n, sp.left);
                }
            }
            else
            {
                for (var n = cleft.Next; n != null && n.Value.pos < newleft; n = cleft.Next)
                {
                    if (n.Value.braceType == BraceType.center && n.Value.owner.y.pos > sp.up.pos && n.Value.owner.y.pos < sp.down.pos)
                    {
                        //移出
                        mySet.Exit(n.Value.owner);
                    }
                    listX.Remove(cleft);
                    cleft = listX.AddAfter(n, sp.left);
                }
            }

            //close
            var cright = listX.Find(sp.right);
            int newright = sp.right.pos + x - sp.x.pos;
            if (newright < sp.right.pos)
            {
                for (var n = cright.Previous; n != null && n.Value.pos > newright; n = cright.Previous)
                {
                    if (n.Value.braceType == BraceType.center && n.Value.owner.y.pos > sp.up.pos && n.Value.owner.y.pos < sp.down.pos)
                    {
                        //移出
                        mySet.Exit(n.Value.owner);
                    }
                    listX.Remove(cright);
                    cright = listX.AddBefore(n, sp.right);
                }
            }
            else
            {
                for (var n = cright.Next; n != null && n.Value.pos < newright; n = cright.Next)
                {
                    if (n.Value.braceType == BraceType.center && n.Value.owner.y.pos > sp.up.pos && n.Value.owner.y.pos < sp.down.pos)
                    {
                        //移入
                        mySet.Enter(n.Value.owner);
                    }
                    listX.Remove(cright);
                    cright = listX.AddAfter(n, sp.right);
                }
            }

            foreach (var kv in mySet.set)
            {
                if (kv.Value > 0)
                {
                    sp.views.Add(kv.Key);
                }
                else
                {
                    sp.views.Remove(kv.Key);
                }
            }
            mySet.Clear();

            sp.x.pos = x;
            sp.left.pos = newleft;
            sp.right.pos = newright;
            sp.rect.X = newleft;
        }

        void MoveY(Sprite sp, int y)
        {
            if (y == sp.y.pos) return;
            if (y < sp.y.pos)
            {
                //上移
                var c = listY.Find(sp.y);
                for (var n = c.Previous; n != null && n.Value.pos > y; n = c.Previous)
                {
                    if (sp.x.pos > n.Value.owner.left.pos && sp.x.pos < n.Value.owner.right.pos)
                    {
                        if (n.Value.braceType == BraceType.close)
                        {
                            //移入
                            otherSet.Enter(n.Value.owner);
                        }
                        if (n.Value.braceType == BraceType.open)
                        {
                            //移出
                            otherSet.Exit(n.Value.owner);
                        }
                    }
                    listY.Remove(c);
                    c = listY.AddBefore(n, sp.y);
                }
            }
            else
            {
                //下移
                var c = listY.Find(sp.y);
                for (var n = c.Next; n != null && n.Value.pos < y; n = c.Next)
                {
                    if (sp.x.pos > n.Value.owner.left.pos && sp.x.pos < n.Value.owner.right.pos)
                    {
                        if (n.Value.braceType == BraceType.open)
                        {
                            //移入
                            otherSet.Enter(n.Value.owner);
                        }

                        if (n.Value.braceType == BraceType.close)
                        {
                            //移出
                            otherSet.Exit(n.Value.owner);
                        }
                    }
                    listY.Remove(c);
                    c = listY.AddAfter(n, sp.y);
                }

            }

            foreach (var kv in otherSet.set)
            {
                if (kv.Value > 0)
                {
                    kv.Key.views.Add(sp);
                }
                else
                {
                    kv.Key.views.Remove(sp);
                }
            }
            otherSet.Clear();

            //open
            var cup = listY.Find(sp.up);
            int newup = sp.up.pos + y - sp.y.pos;
            if (newup < sp.up.pos)
            {
                for (var n = cup.Previous; n != null && n.Value.pos > newup; n = cup.Previous)
                {
                    if (n.Value.braceType == BraceType.center && n.Value.owner.x.pos > sp.left.pos && n.Value.owner.x.pos < sp.right.pos)
                    {
                        //移入
                        mySet.Enter(n.Value.owner);
                    }
                    listY.Remove(cup);
                    cup = listY.AddBefore(n, sp.up);
                }
            }
            else
            {
                for (var n = cup.Next; n != null && n.Value.pos < newup; n = cup.Next)
                {
                    if (n.Value.braceType == BraceType.center && n.Value.owner.x.pos > sp.left.pos && n.Value.owner.x.pos < sp.right.pos)
                    {
                        //移出
                        mySet.Exit(n.Value.owner);
                    }
                    listY.Remove(cup);
                    cup = listY.AddAfter(n, sp.up);
                }
            }

            //close
            var cdown = listY.Find(sp.down);
            int newdown = sp.down.pos + y - sp.y.pos;
            if (newdown < sp.down.pos)
            {
                for (var n = cdown.Previous; n != null && n.Value.pos > newdown; n = cdown.Previous)
                {
                    if (n.Value.braceType == BraceType.center && n.Value.owner.x.pos > sp.left.pos && n.Value.owner.x.pos < sp.right.pos)
                    {
                        //移出
                        mySet.Exit(n.Value.owner);
                    }
                    listY.Remove(cdown);
                    cdown = listY.AddBefore(n, sp.down);
                }
            }
            else
            {
                for (var n = cdown.Next; n != null && n.Value.pos < newdown; n = cdown.Next)
                {
                    if (n.Value.braceType == BraceType.center && n.Value.owner.x.pos > sp.left.pos && n.Value.owner.x.pos < sp.right.pos)
                    {
                        //移入
                        mySet.Enter(n.Value.owner);
                    }
                    listY.Remove(cdown);
                    cdown = listY.AddAfter(n, sp.down);
                }
            }

            foreach (var kv in mySet.set)
            {
                if (kv.Value > 0)
                {
                    sp.views.Add(kv.Key);
                }
                else
                {
                    sp.views.Remove(kv.Key);
                }
            }
            mySet.Clear();

            sp.y.pos = y;
            sp.up.pos = newup;
            sp.down.pos = newdown;
            sp.rect.Y = newup;
        }


        int maxSpriteId = 0;
        Random random = new Random();
        void SpawnSprite()
        {
            maxSpriteId++;
            int sight = random.Next(10, 50);
            int x = random.Next(0, this.ClientSize.Width);
            int y = random.Next(0, this.ClientSize.Height);
            AddSprite(maxSpriteId, sight, x, y);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            XAxisEnd.X = this.ClientSize.Width;
            YAxisEnd.Y = this.ClientSize.Height;

            g.DrawLine(Pens.Black, XAxisBegin, XAxisEnd);
            g.DrawLine(Pens.Black, YAxisBegin, YAxisEnd);

            foreach (var sp in all)
            {
                if (sp == selectSprite)
                {
                    g.DrawLine(Pens.Green, sp.x.pos, sp.y.pos, 2, sp.y.pos);
                    g.DrawLine(Pens.Green, sp.x.pos, sp.y.pos, sp.x.pos, 2);
                    g.DrawLine(Pens.DarkRed, sp.left.pos, sp.up.pos, sp.left.pos, 2);
                    g.DrawLine(Pens.DarkRed, sp.left.pos, sp.up.pos, 2, sp.up.pos);
                    g.DrawLine(Pens.DarkRed, sp.right.pos, sp.up.pos, sp.right.pos, 2);
                    g.DrawLine(Pens.DarkRed, sp.left.pos, sp.down.pos, 2, sp.down.pos);

                }
                else if (selectSprite != null && selectSprite.views.Contains(sp))
                {
                    g.DrawLine(Pens.Gray, sp.left.pos, sp.up.pos, sp.left.pos, 2);
                    g.DrawLine(Pens.Gray, sp.left.pos, sp.up.pos, 2, sp.up.pos);
                    g.DrawLine(Pens.Gray, sp.right.pos, sp.up.pos, sp.right.pos, 2);
                    g.DrawLine(Pens.Gray, sp.left.pos, sp.down.pos, 2, sp.down.pos);

                }
                else
                {
                    g.DrawLine(Pens.LightGray, sp.left.pos, sp.up.pos, sp.left.pos, 2);
                    g.DrawLine(Pens.LightGray, sp.left.pos, sp.up.pos, 2, sp.up.pos);
                    g.DrawLine(Pens.LightGray, sp.right.pos, sp.up.pos, sp.right.pos, 2);
                    g.DrawLine(Pens.LightGray, sp.left.pos, sp.down.pos, 2, sp.down.pos);

                }
            }

            foreach (var sp in all)
            {
                if (sp == selectSprite)
                {
                    g.DrawRectangle(Pens.Red, sp.rect);
                    g.FillEllipse(Brushes.Red, sp.x.pos - 1, sp.y.pos - 1, 2, 2);

                }
                else if (selectSprite != null && selectSprite.views.Contains(sp))
                {
                    g.DrawRectangle(Pens.Red, sp.rect);
                    g.FillEllipse(Brushes.Red, sp.x.pos - 1, sp.y.pos - 1, 2, 2);

                }
                else
                {
                    g.DrawRectangle(Pens.Black, sp.rect);
                    g.FillEllipse(Brushes.Black, sp.x.pos - 1, sp.y.pos - 1, 2, 2);
                }
                g.DrawString(sp.id.ToString(), new Font(FontFamily.GenericMonospace, 12), Brushes.Green, sp.x.pos, sp.y.pos);
            }

        }

        Sprite selectSprite = null;
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            foreach (var sp in all)
            {
                if (e.X > sp.left.pos && e.X < sp.right.pos && e.Y > sp.up.pos && e.Y < sp.down.pos)
                {
                    selectSprite = sp;
                    Invalidate();
                    return;
                }
            }
        }


        bool leftDown = false;
        bool rightDown = false;
        bool upDown = false;
        bool downDown = false;
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            switch (e.KeyCode)
            {
                case Keys.Left: leftDown = false; break;
                case Keys.Right: rightDown = false; break;
                case Keys.Up: upDown = false; break;
                case Keys.Down: downDown = false; break;
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                SpawnSprite();
                return;
            }

            if (selectSprite == null) return;
            int x = selectSprite.x.pos;
            int y = selectSprite.y.pos;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftDown = true;
                    break;
                case Keys.Right:
                    rightDown = true;
                    break;
                case Keys.Up:
                    upDown = true;
                    break;
                case Keys.Down:
                    downDown = true;
                    break;
            }
            if (leftDown)
            {
                x -= 2;
                if (x < 0)
                {
                    x = this.ClientSize.Width;
                }
            }
            if (rightDown)
            {
                x += 2;
                if (x > this.ClientSize.Width)
                {
                    x = 0;
                }

            }
            if (upDown)
            {
                y -= 2;
                if (y < 0)
                {
                    y = this.ClientSize.Height;
                }
            }
            if (downDown)
            {
                y += 2;
                if (y > this.ClientSize.Height)
                {
                    y = 0;
                }
            }

            MoveSprite(selectSprite, x, y);
        }
    }
}
