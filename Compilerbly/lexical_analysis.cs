using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace PL0_Compiler
{
    /* 
     * 关键字[1-17),
     * 分界符[17-22)
     * 单字符运算符[22,29)
     * 双字符运算符[29,33)，
     * 常量33,
     * 标识符34。
    */
    public enum symlist
    {
        //关键字16
        beginsym =1, endsym, constsym,
        varsym, proceduresym, oddsym,
        ifsym, thensym, elsesym,
        callsym, whilesym, dosym,
        repeatsym, untilsym, readsym, writesym,
        //分隔符5
        LParenthesis, RParenthesis, comma, semicolon, period,//, ; .
        //运算符11
        plus, minus, times, division, equality, LessThanE, MoreThanE,
        becomes,LessThan, MoreThan,inequality,
        //数值1
        number,
        //标识符变量1
        iden,
        nul = 0,
        end=-1
    }
    public class lexical_analysis
    {
        private int i;//当前字符
        public int line;//当前单词所在行
        public string chnum;
        private Compiler compiler;
        public string word;//当前存储的单词
        public int num;//如果是常数，存储当前单词所表示的常数
        private string code;//源码
        public symlist sym;//类别
        public lexical_analysis(Compiler c)
        {
            compiler = c;
            code = c.source;
            chnum = "";
            i = 0;
            line = 1;
            word = "";
        }

        public static string[] reserveword = new string[16]{
        "begin", "end","const","var","procedure" ,"odd","if","then","else","call",
        "while","do","repeat","until","read","write"
        };
        public static char[] single_operator = new char[7]{
        '+', '-', '*', '/', '=', '<', '>'
        };
        public static string[] double_operator = new string[4] {
            "<>",">=","<=",":="
        };
        public static char[] orDelimiter = new char[5]{
        '(', ')', ',',';' , '.'
        };

        bool isletter(char f)
        {
            if ((f >= 'a' && f <= 'z') || (f >= 'A' && f <= 'Z'))
                return true;
            return false;
        }
        bool isnum(char f)
        {
            if ((f >= '0' && f <= '9'))
                return true;
            return false;
        }
        int isreserveword(string s)
        {
            for (int i = 0; i < 16; i++)
            {
                if (reserveword[i]==s)
                {
                    return i + 1;
                }
            }
            return 0;
        }
        int isdelimiter(char word)
        {
            for (int i = 0; i < 5; i++)
            {
                if (word == orDelimiter[i])
                {
                    return i + 17;
                }
            }
            return 0;
        }
        int isoperator(char word)
        {
            for (int i = 0; i < 7; i++)
            {
                if (single_operator[i]==word)
                    return 22 + i;
            }
            return 0;
        }
        //int isconst(string word)
        //{
        //    for (int i = 0; i < (word.Length); i++)
        //    {
        //        if (word[i] > '9' || word[i] < '0') return 0;
        //    }
        //    return 33;
        //}
        //int issame(char f, char s)
        //{
        //    string str = "" + f + s;
        //    if(isoperator(str)>0)
        //    {
        //        return 1;
        //    }
        //    bool a = ((f >= '0' && f <= '9') || isletter(f)) ? true : false;
        //    bool b = ((s >= '0' && s <= '9') || isletter(s)) ? true : false;
        //    if (a == b)
        //        return 1;
        //    return 0;
        //}
        //bool isiden(string word)
        //{
        //    if (!isletter(word[0])) return false;
        //    for (int i = 1; i < word.Length; i++)
        //    {
        //        if (issame(word[i], word[i])==0) return false;
        //    }
        //    return true;
        //}
        //bool typeword(string word)
        //{
        //    int a;
        //    if ((a = isreserveword(word))>0) { syn = a; return true; }
        //    //else if(Isdelimiter(word)){syn = 2;}
        //    else if ((a = isconst(word))>0) { syn = a; return true; }
        //    else if ((a = isoperator(word))>0) { syn = a; return true; }
        //    //普通标识符
        //    else if (isiden(word)) { syn = 34; return true; }
        //    else
        //    {
        //        //Console.WriteLine(word + "compile error");
        //        return false;
        //    }
        //}
        //public string print(string word)
        //{
        //    string output="";
        //    if (syn < 17)
        //    {
        //        output = string.Format("{0,-12}", word) + string.Format("{0,-12}", "关键字") + string.Format("{0,-12}", word) + '\r' + '\n';
        //    }
        //    if(syn>=17&&syn<22)
        //    {
        //        output = string.Format("{0,-12}", word) + string.Format("{0,-12}", "分界符") + string.Format("{0,-12}", word) + '\r' + '\n';
        //    }
        //    if (syn >= 22 && syn <29)
        //    {
        //        output = string.Format("{0,-12}", word) + string.Format("{0,-12}", "单字符运算符") + string.Format("{0,-12}", word) + '\r' + '\n';
        //    }
        //    if (syn >= 29 && syn < 33)
        //    {
        //        output = string.Format("{0,-12}", word) + string.Format("{0,-12}", "双字符运算符") + string.Format("{0,-12}", word) + '\r' + '\n';
        //    }
        //    if (syn ==34)
        //    {
        //        output = string.Format("{0,-12}", word) + string.Format("{0,-12}", "标识符") + string.Format("{0,-12}", word) + '\r' + '\n';
        //    }
        //    return output;
        //}
        public symlist getsym()
        {
            Error er = compiler.error;
            int syn_num;
            //word = "";

            if (i > code.Length - 1)
            {
                return symlist.end;
            }
                char c =  code[i];
            //跳过空格
            while(c==' '||c=='\n'||c=='\t'||c=='\r')
            {
                if (c == '\n')
                    line++;
                i++;
                if (i > code.Length - 1)
                {
                    return symlist.end;
                }
                c = code[i];
            }
            //标识符
            if(isletter(c))
            {
                word = "";
                do
                {
                    word += c;
                    i++;
                    if (i > code.Length - 1)
                    {
                        return symlist.end;
                    }
                    c = code[i];
                } while (isletter(c) || isnum(c));
                syn_num = isreserveword(word);
                if(syn_num>0)
                {
                    sym = (symlist)syn_num;
                }
                else
                {
                    return symlist.iden;
                }
            }

            //数字
            else if(isnum(c))
            {
                chnum = "";
                do
                {
                    chnum += c;
                    i++;
                    if (i > code.Length - 1)
                    {
                        return symlist.end;
                    }
                    c = code[i];
                } while (isnum(c));
                try
                {
                    num = Convert.ToInt32(chnum);
                }
                catch (OverflowException e)
                {
                    er.adderror(31);
                    num = 0;
                }
                sym = symlist.number;
            }

            //分界符
            else if((syn_num=isdelimiter(c))>0)
            {
                sym= (symlist)syn_num;
                i++;
            }
            //三个双目操作
            else if(c==':')
            {
                i++;
                if (i > code.Length - 1)
                {
                    return symlist.end;
                }
                c = code[i];
                if (c == '=') sym = symlist.becomes;
                else sym = symlist.nul;
                i++;
            }
            else if(c=='<') //"<>",">=","<=",":="
            {
                i++;
                if (i > code.Length - 1)
                {
                    return symlist.end;
                }
                c = code[i];
                if (c == '>') sym = symlist.inequality;
                else if (c == '=') sym = symlist.LessThan;
                else{ sym = symlist.LessThanE;i--;}
                i++;
            }
            else if(c=='>')
            {
                i++;
                if (i > code.Length - 1)
                {
                    return symlist.end;
                }
                c = code[i];
                if (c == '=') sym = symlist.MoreThan;
                else { sym = symlist.MoreThanE; i--; }
                i++;
            }
             //单目操作
            else if ((syn_num = isoperator(c)) > 0)
            {
                sym = (symlist)syn_num;
                i++;
            }

            return sym;
        }

    }
}
