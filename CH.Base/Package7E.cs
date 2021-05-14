using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH.Base
{
    public class Package7E
    {
        public static byte[] Pack(byte[] bs)
        {
            List<byte> cmd = new List<byte>();
            cmd.Add(0x7e);
            foreach (byte b in bs)
            {
                if (b == 0x7e)
                {
                    cmd.Add(0x7d);
                    cmd.Add(0x5e);
                }
                else if (b == 0x7d)
                {
                    cmd.Add(0x7d);
                    cmd.Add(0x5d);
                }
                else
                {
                    cmd.Add(b);
                }
            }
            cmd.Add(0x7e);
            return cmd.ToArray();
        }
        public static void UnPack(byte[] buffers, ref List<byte[]> cmds, ref int removeCount)
        {
            List<byte> cmd = new List<byte>();
            bool findHead = false;
            removeCount = 0;
            for (int i = 0; i < buffers.Length; i++)
            {
                if (!findHead)
                {
                    if (buffers[i] == 0x7e)
                    {
                        //找到包头
                        findHead = true;
                        removeCount = i;
                    }
                    else
                    {
                        removeCount = i + 1;
                    }
                    continue;
                }

                if (buffers[i] == 0x7e)
                {
                    //找到包尾
                    removeCount = i + 1;
                    if (cmd.Count > 0)
                    {
                        cmds.Add(cmd.ToArray());
                        cmd.Clear();
                    }
                    findHead = false;
                    continue;
                }

                if (buffers[i] == 0x7d && (i + 1) < buffers.Length)
                {
                    if (buffers[i + 1] == 0x5e)
                    {
                        cmd.Add(0x7e);
                        i++;
                    }
                    else if (buffers[i + 1] == 0x5d)
                    {
                        cmd.Add(0x7d);
                        i++;
                    }
                }
                else
                {
                    cmd.Add(buffers[i]);
                }
            }
        }
    }
}
