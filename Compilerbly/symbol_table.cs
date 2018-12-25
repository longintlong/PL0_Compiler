using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL0_Compiler
{
    /// <summary>
    /// 符号表记录
    /// </summary>
    public class record
    {
        //constant variable procedure
        public string name { get; set; }
        public string kind { get; set; }
        public int val { get; set; }
        public int level { get; set; }
        public int adr { get; set; }
        public int size { get; set; }
    }

    public class symbol_table
    {
        public  Compiler compiler;
        public  int point;
        public List<record> stable =new List<record>();//符号表
        public int[] dx = new int[4];//层次入口数组
        public symbol_table(Compiler C)
        {
            compiler = C;
            point = 0;
            dx[0] = 0;
            record stentry = new record();
            stentry.name = "";
            stable.Add(stentry);
        }
        //添加到符号表中
        public void add(symlist sym)
        {
            syntax_Analysis  sa = compiler.sa;
            if (sa.level > point)
            {
                point++;
                dx[point] = dx[point - 1];
            }
            else if (sa.level < point)
            {
                //int delnum = dx[point] - dx[point - 1];
                //for (int i = 0; i < delnum; i++)
                //    stable.RemoveAt(stable.Count - 1);
                //point--;
            }
            dx[point]++;
            record re = new record();
            re.name = compiler.la.word;
            if(sym==symlist.constsym)
            {
                re.kind = "constant";
                re.val = compiler.la.num;
                stable.Add(re);
            }
            else if(sym==symlist.varsym)
            {
                re.kind = "variable";
                re.level = sa.level;
                //获取地址还没写re.adr = 
                re.adr = compiler.sa.getadd();
                stable.Add(re);
            }
            else if(sym==symlist.proceduresym)
            {
                re.kind = "procedure";
                re.level = sa.level;
                stable.Add(re);
            }
        }
        //查表
        public int position(string s)
        {
            for(int i=dx[point];i>0;i--)
            {
                if (stable[i].name.Equals(s))
                    return i;
            }
            return -1;
        }
    }
}
