using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL0_Compiler
{

    public class CODE
    {
        public string op{get;set;}
        public int l { get; set; }
        public int a { get; set; }
    }
    
    public class Pcode
    {
        private Compiler compiler;
        public List<CODE> pcdeolst= new List<CODE>();//存放pcode代码
        public int cx;//代码的位置
        public int badd;//基地址
        public int[] stack = new int[500];//运行栈

        public Pcode()
        {
            cx = 0;

        }
        //生成Pcode 指令
        public void gen(string op,int l,int a)
        {
            CODE instru = new CODE();
            instru.op = op;
            instru.l = l;
            instru.a = a;
            pcdeolst.Add(instru);
            cx++;
        }
        //获得层差数为d的入口地址
        public int getadd(int d)
        {
            int b = badd;
            while(d>0)
            {
                b = stack[b];
                d--;
            }
            return b;
        }


    }

}
