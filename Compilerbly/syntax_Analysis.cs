using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL0_Compiler
{
    public class syntax_Analysis
    {
        private  Compiler compiler;
        public  int level;                      //当前的层数
        public int[] dx = new int[4];           //地址分配索引数组
        public  symlist sym;                    //当前单词的类别
        public List<symlist> declareSymSet = new List<symlist>();
        public List<symlist> stateSymSet = new List<symlist>();
        public List<symlist> factorSymSet = new List<symlist>();

        public syntax_Analysis(Compiler c)
        {
            compiler = c;
            //声明开始符号
            declareSymSet.Add(symlist.constsym);
            declareSymSet.Add(symlist.varsym);
            declareSymSet.Add(symlist.proceduresym);
            //语句开始符号
            stateSymSet.Add(symlist.beginsym);
            stateSymSet.Add(symlist.callsym);
            stateSymSet.Add(symlist.whilesym);
            stateSymSet.Add(symlist.ifsym);
            stateSymSet.Add(symlist.repeatsym);
            //因子开始符号
            factorSymSet.Add(symlist.iden);
            factorSymSet.Add(symlist.number);
            factorSymSet.Add(symlist.LParenthesis);
        }
        //变量的地址
        public int getadd()
        {
            return dx[level];
        }
        public void getsym()
        {
            sym = compiler.la.getsym();
        }
        public void check(List<symlist> l1,List<symlist> l2,int num)
        {
            Error er = compiler.error;
            if(!l1.Contains(sym)&&(sym!=symlist.end))
            {
                //error处理
                er.adderror(num);
                l1.AddRange(l2);
                //l1 = l1.Union(l2).ToList<symlist>();
                while (!l1.Contains(sym)&&(sym!=symlist.end))
                    getsym();
            }
        }
        //语法分析入口
        public void analysis()
        {
            List<symlist> blocksym = new List<symlist>();//当前块的follow集
            blocksym.AddRange(declareSymSet);
            blocksym.AddRange(stateSymSet);
            blocksym.Add(symlist.period);
            level = -1;
            getsym();
            program(blocksym);
        }
        //<程序> ::= <分程序>.
        public void program(List<symlist> lst)
        {
            Error er = compiler.error;
            subprogram(lst);
            if(sym==symlist.period)
            {
                getsym();
                if (sym != symlist.end)
                    er.adderror(36);
            }
            else 
            er.adderror(9);
            //if(sym!=symlist.period&&sym!=symlist.end)
            //{
            //    //error 
            //    er.adderror(9);
            //}
            //else if(sym==symlist.period&&sym!=symlist.end)
            //{
            //    while(sym!=symlist.end)
            //    {
            //        getsym();
            //        if (sym >= (symlist)0)
            //            er.adderror(36);
            //    }
            //}
           
        }
        //<分程序>::=[<常量说明部分>][变量说明部分>][<过程说明部分>]<语句>
        public void subprogram(List<symlist> followlst)
        {
            List<symlist> nextlst = new List<symlist>();
            symbol_table st = compiler.st;
            Pcode pcode =compiler.pc;
            Error er = compiler.error;
            int ti, ch;
            level++;
            dx[level] = 3;
            ti = st.dx[st.point];
            st.stable[ti].adr = pcode.cx;
            pcode.gen("JMP", 0, 0);
            //判断是否分程序的层次超过最大层数3
            if (level > 3)
                er.adderror(26);
            do
            {
                //<常量说明部分> ::= const<常量定义>{,<常量定义>};
                if (sym == symlist.constsym)
                {
                    do
                    {
                        getsym();
                        decconst();
                    } while (sym == symlist.comma);
                    if (sym == symlist.semicolon)
                        getsym();
                    else//少了一个分号
                        er.adderror(5); 
                }
                //<变量说明部分>::= var<标识符>{,<标识符>};
                if (sym==symlist.varsym)
                {
                    do
                    {
                        getsym();
                        decvar();
                    } while (sym == symlist.comma);
                    if (sym == symlist.semicolon)
                        getsym();
                    else//少了一个分号
                        er.adderror(5);
                }
                //<过程说明部分> ::= <过程首部><分程序>;{<过程说明部分>}
                //<过程首部> ::= procedure<标识符>;
                while (sym==symlist.proceduresym)
                {
                    getsym();
                    if (sym == symlist.iden)
                    {
                        st.add(symlist.proceduresym);//加入符号表
                        getsym();
                    }
                    else//不是标识符
                        er.adderror(4);
                    if (sym == symlist.semicolon)
                        getsym();
                    else
                        er.adderror(5);
                    nextlst.AddRange(followlst);
                    nextlst.Add(symlist.semicolon);
                    subprogram(nextlst);
                    if (sym == symlist.semicolon)
                    {
                        getsym();
                        nextlst.Clear();
                        nextlst.AddRange(followlst);
                        nextlst.Add(symlist.iden);
                        nextlst.Add(symlist.proceduresym);
                        check(nextlst, followlst,6);
                    }
                    else
                        er.adderror(5);
                }
                nextlst.Clear();
                nextlst.AddRange(stateSymSet);
                nextlst.Add(symlist.iden);
                check(nextlst, declareSymSet,7);
            } while (declareSymSet.Contains(sym));

            pcode.pcdeolst[st.stable[ti].adr].a = pcode.cx;
            st.stable[ti].adr = pcode.cx;
            st.stable[ti].size = dx[level];
            ch = pcode.cx;
            pcode.gen("INT", 0, dx[level]);
            nextlst.Clear();
            nextlst.AddRange(followlst);
            nextlst.Add(symlist.semicolon);
            nextlst.Add(symlist.endsym);
            statement(nextlst);
            pcode.gen("OPR", 0, 0);
            check(nextlst, new List<symlist>(), 8);
            level--;
        }
        //<常量定义> ::= <标识符>=<无符号整数>
        public void decconst()
        {
            Error er = compiler.error;
            symbol_table st = compiler.st;
            if(sym==symlist.iden)
            {
                getsym();
                if (sym == symlist.equality || sym == symlist.becomes)
                {
                    if (sym == symlist.becomes)
                        er.adderror(1);
                    getsym();
                    if (sym == symlist.number)
                    {
                        st.add(symlist.constsym);  //加入符号表
                        getsym();
                    }
                    else//常数声明标识符后不是数字
                        er.adderror(2);
                }
                else//不是等号
                    er.adderror(3);
            }
            else   //不是标识符
            {
                er.adderror(4);
            }
        }
        //<变量说明部分>::= var<标识符>{,<标识符>};
        public void decvar()
        {
            Error er = compiler.error;
            symbol_table st = compiler.st;
            if (sym == symlist.iden)
            {
                st.add(symlist.varsym);//加入符号表
                dx[level]++;//分配地址
                getsym();
            }
            else//不是标识符
                er.adderror(4);
        }
        //<语句> ::= <赋值语句>|<条件语句>|<当型循环语句>|<过程调用语句>|<读语句>|<写语句>|<复合语句>|<重复语句>|<空>
        public void statement(List<symlist> followlst)
        {
            Error er = compiler.error;
            symbol_table st = compiler.st;
            lexical_analysis la = compiler.la;
            Pcode pc = compiler.pc;
            List<symlist> nextlst = new List<symlist>();
            List<symlist> n2 = new List<symlist>();
        
            int index, cx1, cx2;
            switch (sym)
            {
                //<赋值语句> ::= <标识符>:=<表达式>
                case symlist.iden:
                    index = st.position(la.word);
                    if (index == -1)//未声明
                        er.adderror(11);
                    else if (st.stable[index].kind != "variable")
                    {
                        er.adderror(12);
                        index = -1;
                    }
                    getsym();
                    if(sym==symlist.becomes)
                        getsym();
                    else//不是:=
                        er.adderror(13);
                    expression(followlst);
                    if(index!=-1)//生成pcode
                    {
                        int l = level - st.stable[index].level;
                        int a = st.stable[index].adr;
                        pc.gen("STO", l, a);
                    }
                    break;
                //<过程调用语句>::=call<标识符>
                case symlist.callsym:
                    getsym();
                    if(sym==symlist.iden)
                    {
                        index = st.position(la.word);
                        if (index == -1)
                            er.adderror(11);
                        else if (st.stable[index].kind != "procedure")
                        {
                            index = -1;
                            er.adderror(15);
                        }
                        else//生成pcode
                        {
                            int l = level - st.stable[index].level;
                            int a = st.stable[index].adr;
                            pc.gen("CAL", l, a);
                        }
                        getsym();
                    }
                    else
                        er.adderror(14);
                    break;
                //<复合语句> ::= begin<语句>{;<语句>}end
                case symlist.beginsym:
                    getsym();
                    nextlst.Clear();
                    nextlst.AddRange(followlst);
                    n2.Clear();
                    n2.AddRange(stateSymSet);
                    n2.Add(symlist.semicolon);
                    statement(nextlst);
                    while (n2.Contains(sym))
                    {
                        if (sym == symlist.semicolon)
                        {
                            getsym();
                        }
                        else
                            er.adderror(10);
                        statement(nextlst);
                    }
                    if (sym == symlist.endsym)
                        getsym();
                    else//少了end
                    { er.adderror(17); }
                    break;
                //<条件语句> ::= if<条件>then<语句>[else<语句>]    
                case symlist.ifsym:
                    getsym();
                    nextlst.Clear();
                    nextlst.AddRange(followlst);
                    nextlst.Add(symlist.thensym);
                    nextlst.Add(symlist.dosym);
                    ifstatement(nextlst);
                    if(sym==symlist.thensym)
                        getsym();
                    else
                        er.adderror(16);
                    cx1 = pc.cx;
                    pc.gen("JPC", 0, 0);
                    nextlst.Clear();
                    nextlst.AddRange(followlst);
                    nextlst.Add(symlist.elsesym);
                    statement(nextlst);
                    if (sym == symlist.elsesym)
                    {
                        getsym();
                        cx2 = pc.cx;
                        pc.gen("JMP", 0, 0);
                        statement(followlst);
                        pc.pcdeolst[cx1].a = cx2 + 1;
                        pc.pcdeolst[cx2].a = pc.cx;
                    }
                    else
                        pc.pcdeolst[cx1].a = pc.cx;
                    break;
                 // <当型循环语句 > ::= while< 条件 >do< 语句 >
                case symlist.whilesym:
                    cx1 = pc.cx;
                    getsym();
                    nextlst.Clear();
                    nextlst.AddRange(followlst);
                    nextlst.Add(symlist.dosym);
                    ifstatement(nextlst);
                    cx2 = pc.cx;
                    pc.gen("JPC", 0, 0);
                    if(sym==symlist.dosym)
                        getsym();
                    else
                        er.adderror(18);//error
                    statement(followlst);
                    pc.gen("JMP", 0, cx1);
                    pc.pcdeolst[cx2].a = pc.cx;
                    break;
                //<读语句> ::= read'('<标识符>{,<标识符>}')'
                case symlist.readsym:
                    getsym();
                    if (sym == symlist.LParenthesis)
                    {
                        do
                        {
                            getsym();
                            if (sym == symlist.iden)
                                index = st.position(la.word);
                            else
                            {
                                index = -1;
                                er.adderror(32);//error
                            }
                            if (index>0&&st.stable[index].kind != "variable")
                            {
                                getsym();
                                er.adderror(32);
                            }
                            else if(sym==symlist.RParenthesis||sym==symlist.semicolon)
                            {
                                er.adderror(35);
                                getsym();
                            }
                            else
                            {
                                int l = level - st.stable[index].level;
                                int a = st.stable[index].adr;
                                pc.gen("RED",l,a);
                                getsym();
                            }
                        } while (sym == symlist.comma);
                    }
                    else { er.adderror(33); }//error
                    if(sym==symlist.RParenthesis)
                        getsym();
                    else
                    {
                        er.adderror(34);
                        while (!followlst.Contains(sym))
                            getsym();
                    }
                    break;
                //<写语句> ::= write'('<标识符>{,<标识符>}')'
                case symlist.writesym:
                    getsym();
                    nextlst.Clear();
                    nextlst.AddRange(followlst);
                    nextlst.Add(symlist.RParenthesis);
                    nextlst.Add(symlist.comma);
                    if (sym == symlist.LParenthesis)
                    {
                        do
                        {
                            getsym();
                            expression(nextlst);
                            pc.gen("WRT", 0, 0);
                        } while (sym == symlist.comma);
                        if (sym == symlist.RParenthesis)
                        {
                            getsym();
                        }
                        else { er.adderror(34); }//error
                    }
                    else { er.adderror(33); }//error
                    break;
                //<重复语句> ::= repeat<语句>{;<语句>}until<条件>
                case symlist.repeatsym:
                    getsym();
                    nextlst.Clear();
                    nextlst.AddRange(followlst);
                    nextlst.Add(symlist.untilsym);
                    cx1 = pc.cx;
                    statement(nextlst);
                    n2.Clear();
                    n2.AddRange(stateSymSet);
                    n2.Add(symlist.semicolon);
                    while(n2.Contains(sym))
                    {
                        if (sym == symlist.semicolon)
                            getsym();
                        else { er.adderror(10); }//error}
                         statement(nextlst);
                    }
                    if(sym==symlist.untilsym)
                    {
                        getsym();
                        ifstatement(followlst);
                        pc.gen("JPC", 0, cx1);
                    }
                    else { er.adderror(25); }//error
                    break;
            }
            check(followlst, new List<symlist>(),19);
        }
        //<表达式> ::= [+|-]<项>{<加法运算符><项>}
        public void expression(List<symlist> followlst)
        {
            Pcode pc = compiler.pc;
            List<symlist> nextlst = new List<symlist>();
            nextlst.AddRange(followlst);
            nextlst.Add(symlist.plus);
            nextlst.Add(symlist.minus);
            symlist symm = sym;
            if (sym==symlist.minus||sym==symlist.plus)
            {
                getsym();
                item(nextlst);
                if (symm == symlist.minus)//取反
                    pc.gen("OPR", 0, 1);
            }
            else 
                item(nextlst);
            while (sym == symlist.minus || sym == symlist.plus)
            {
                symm = sym;
                getsym();
                item(nextlst);
                if (symm == symlist.plus)
                    pc.gen("OPR", 0, 2);
                else
                    pc.gen("OPR", 0, 3);
            }
        }
        //<条件> ::= <表达式><关系运算符><表达式>|odd<表达式>
        public void ifstatement(List<symlist> followlst)
        {
            Pcode pc = compiler.pc;
            List<symlist> nextlst = new List<symlist>();
            List<symlist> n2 = new List<symlist>();
            Error er = compiler.error;
            nextlst.Add(symlist.equality);
            nextlst.Add(symlist.LessThan);
            nextlst.Add(symlist.LessThanE);
            nextlst.Add(symlist.MoreThan);
            nextlst.Add(symlist.MoreThanE);
            nextlst.Add(symlist.inequality);
            if(sym!=symlist.oddsym)
            {
                n2.AddRange(followlst);
                n2.AddRange(nextlst);
                expression(n2);
                if(!nextlst.Contains(sym))
                {
                    er.adderror(20);
                }
                else
                {
                    symlist symm = sym;
                    getsym();
                    expression(followlst);
                    switch(symm)
                    {
                        case symlist.equality:
                            pc.gen("OPR", 0, 8);
                            break;
                        case symlist.LessThan:
                            pc.gen("OPR", 0, 13);
                            break;
                        case symlist.LessThanE:
                            pc.gen("OPR", 0, 10);
                            break;
                        case symlist.MoreThan:
                            pc.gen("OPR", 0, 12);
                            break;
                        case symlist.MoreThanE:
                            pc.gen("OPR", 0, 11);
                            break;
                        case symlist.inequality:
                            pc.gen("OPR", 0, 9);
                            break;
                    }
                }
            }
            else if(sym==symlist.oddsym)
            {
                getsym();
                expression(followlst);
                pc.gen("OPR", 0, 6);
            }
        }
        //<项> ::= <因子>{<乘法运算符><因子>}
        public void item(List<symlist> followlst)
        {
            Pcode pc = compiler.pc;
            List<symlist> nextlst = new List<symlist>();
            nextlst.AddRange(followlst);
            nextlst.Add(symlist.times);
            nextlst.Add(symlist.division);
            factor(nextlst);
            symlist symm;
            while (sym == symlist.times || sym == symlist.division)
            {
                symm = sym;
                getsym();
                factor(nextlst);
                if (symm == symlist.times)
                    pc.gen("OPR", 0, 4);
                else
                    pc.gen("OPR", 0, 5);
            }
        }
        //<因子> ::= <标识符>|<无符号整数>|'('<表达式>')'
        public void factor(List<symlist> followlst)
        {
            Pcode pc = compiler.pc;
            List<symlist> nextlst = new List<symlist>();
            symbol_table st = compiler.st;
            lexical_analysis la = compiler.la;
            Error er = compiler.error;
            int index;
            nextlst.AddRange(factorSymSet);
            check(nextlst,followlst,24);
            while(factorSymSet.Contains(sym))
            {
                if (sym == symlist.iden)
                {
                    index = st.position(la.word);
                    if (index == -1) er.adderror(11);
                    else
                    {
                        record re = st.stable[index];
                        string kind = re.kind;
                        if (kind == "constant")
                            pc.gen("LIT", 0, re.val);
                        else if (kind == "variable")
                        {
                            int l = level - re.level;
                            int a = re.adr;
                            pc.gen("LOD", l, a);
                        }
                        else
                            er.adderror(21);
                    }
                    getsym();
                }
                else if (sym == symlist.number)
                {
                    pc.gen("LIT", 0, la.num);
                    getsym();
                }
                else if (sym == symlist.LParenthesis)
                {
                    getsym();
                    followlst.Add(symlist.RParenthesis);
                    expression(followlst);
                    if (sym == symlist.RParenthesis) getsym();
                    else { er.adderror(22); }//error
                }
                check(followlst, factorSymSet,23); 
            }
        }

    }
}
