using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PL0_Compiler
{
    public class Compiler
    {
        //源程序文件
        public string  source { get; private set; }
        //词法分析器
        public lexical_analysis la { get; private set; }
        //语法，语义分析器
        public syntax_Analysis sa { get; private set; }
        //符号表管理
        public symbol_table st { get; private set; }
        ////错误处理
        public Error error { get; private set; }
        ////目标代码生成
        public Pcode pc { get; private set; }
        public Compiler(string path)
        {
            source = path;
            la = new lexical_analysis(this);
            sa = new syntax_Analysis(this);
            st = new symbol_table(this);
            error = new Error(this);
            pc = new Pcode();
        }
        //编译
        public void compile()
        {
            if (source != null && la != null && sa != null && st != null)
            {
                sa.analysis();
            }
        }


    }
}
