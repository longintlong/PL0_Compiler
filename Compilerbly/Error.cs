using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL0_Compiler
{
    public class echerror
    {
        public int errnum { get; set; }
        public int line { get; set; }
        public echerror(int num ,int line)
        {
            errnum = num;
            this.line = line;
        }
    }
    public class Error
    {
        public Compiler compiler;
        public Dictionary<int, string> errordict = new Dictionary<int, string>();//错误结构
        public List<echerror> errlst = new List<echerror>(); 
        public Error(Compiler c)
        {
            compiler = c;
            errordict.Add(1, "常数说明赋值符号应为\"=\"");
            errordict.Add(2, "常数说明\"=\"后应是数字");
            errordict.Add(3, "常数说明中标识符后应是\"=\"");
            errordict.Add(4, "const, var, procedure 后应是标识符");
            errordict.Add(5, "缺少了\',\' 或\';\'");
            errordict.Add(6, "过程说明后的符号应是语句开始符或过程定义符");
            errordict.Add(7, "应是语句开始符");
            errordict.Add(8, "程序体内的语句部分的后继符不正确");
            errordict.Add(9, "程序结尾应为\'.\'");
            errordict.Add(10, "语句之间缺少\';\'");
            errordict.Add(11, "标识符未说明");
            errordict.Add(12, "不可向常量或过程赋值，赋值语句左值标识符属性应是变量");
            errordict.Add(13, "赋值语句赋值符号应是\':=\'");
            errordict.Add(14, "call 后应为标识符");
            errordict.Add(15, "call 不可调用常量或变量，应为过程");
            errordict.Add(16, "条件语句中缺少\'then\'");
            errordict.Add(17, "语句结束缺少\'end\' 或\';\'");  
            errordict.Add(18, "while 语句中缺少\'do\'");
            errordict.Add(19, "语句后的符号不正确");
            errordict.Add(20, "应为关系运算符");
            errordict.Add(21, "表达式内不可有过程标识符");
            errordict.Add(22, "表达式中缺少\')\'");
            errordict.Add(23, "因子后为非法符号");
            errordict.Add(24, "表达式不能以此符号开始");
            errordict.Add(25, "repeat 语句中缺少until");
            errordict.Add(26, "程序层次嵌套层数过多，最多为3");
            errordict.Add(31, "数过大");
            errordict.Add(32, "read语句括号中的标识符不是变量");
            errordict.Add(33, "语句缺少\'(\'");
            errordict.Add(34, "语句缺少\')\'");
            errordict.Add(35, "read 语句缺少变量");
            errordict.Add(36, "程序之外出现字母");
        }
        public void adderror(int errnum)
        {
            echerror e = new echerror(errnum,compiler.la.line);
            errlst.Add(e);
        }
    }
}
