using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PL0_Compiler;
using System.IO;

namespace Interpret
{
    class Program
    {
        static void Main(string[] args)
        {
            interpreter inter = new interpreter(args[0]);

            inter.interpret();

            Console.WriteLine("请按任意键退出...");
            Console.ReadKey();
        }
    }
    class interpreter
    {
        private int[] stack = new int[200];//运行栈
        private int badd;//栈基址
        private List<CODE> pcode = new List<CODE>();

        public interpreter(string codelst)
        {
            //Console.WriteLine(codelst);
            //Console.WriteLine("");
            if (codelst!="")
            {
                try
                {
                    string[] pCodes = codelst.Split('\n');
                    //for (int i = 0; i <= pCodes.Length - 1; i++)
                    //{
                    //    Console.WriteLine(i.ToString() + " " + pCodes[i] + " " + pCodes[i].Length.ToString());
                    //}
                    for (int i = 0; i < pCodes.Length - 1; i++)
                    {
                        string cl = pCodes[i];
                        string[] ops = cl.Split('*');
                        CODE ins = new CODE();
                        ins.op = ops[0];
                        ins.l = Convert.ToInt32(ops[1]);
                        ins.a = Convert.ToInt32(ops[2]);
                        pcode.Add(ins);
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //获得层差数为d的入口地址
        public int getadd(int d)
        {
            int b = badd;
            while (d > 0)
            {
                b = stack[b];
                d--;
            }
            return b;
        }

        /// <summary>
        /// 解释执行
        /// </summary>
        public void interpret()
        {
            badd = 1;
            string opc;
            int l, a, i, t;
            i = 0;
            t = 0;
            stack[1] = 0;
            stack[2] = 0;
            stack[3] = 0;
            do
            {
                opc = pcode[i].op;
                l = pcode[i].l;
                a = pcode[i].a;
                i++;
                switch (opc)
                {
                    //将a放到栈顶
                    case "LIT":
                        t++;
                        stack[t] = a;
                        break;
                    case "OPR":
                        switch (a)
                        {
                            case 0:
                                t = badd - 1;
                                i = stack[t + 3];
                                badd = stack[t + 2];
                                break;
                            case 1:
                                stack[t] = -stack[t];
                                break;
                            case 2:
                                t--;
                                stack[t] = stack[t] + stack[t + 1];
                                break;
                            case 3:
                                t--;
                                stack[t] = stack[t] - stack[t + 1];
                                break;
                            case 4:
                                t--;
                                stack[t] = stack[t] * stack[t + 1];
                                break;
                            case 5:
                                t--;
                                stack[t] = stack[t] / stack[t + 1];
                                break;
                            case 6:
                                stack[t] = stack[t] % 2;
                                break;
                            case 7:
                                t--;
                                stack[t] = stack[t] % stack[t + 1];
                                break;
                            case 8:
                                t--;
                                stack[t] = (stack[t] == stack[t + 1]) ? 1 : 0;
                                break;
                            case 9:
                                t--;
                                stack[t] = (stack[t] != stack[t + 1]) ? 1 : 0;
                                break;
                            case 10:
                                t--;
                                stack[t] = (stack[t] < stack[t + 1]) ? 1 : 0;
                                break;
                            case 11:
                                t--;
                                stack[t] = (stack[t] >= stack[t + 1]) ? 1 : 0;
                                break;
                            case 12:
                                t--;
                                stack[t] = (stack[t] > stack[t + 1]) ? 1 : 0;
                                break;
                            case 13:
                                t--;
                                stack[t] = (stack[t] <= stack[t + 1]) ? 1 : 0;
                                break;
                        }
                        break;
                    //取变量放在栈顶
                    case "LOD":
                        t++;
                        stack[t] = stack[getadd(l) + a];
                        break;
                    //将栈顶存入
                    case "STO":
                        stack[getadd(l) + a] = stack[t];
                        t--;
                        break;
                    //调用过程
                    case "CAL":
                        stack[t + 1] = getadd(l);
                        stack[t + 2] = badd;
                        stack[t + 3] = i;
                        badd = t + 1;
                        i = a;
                        break;
                    //增加数据栈顶指针
                    case "INT":
                        t = t + a;
                        break;
                    //无条件跳转到a
                    case "JMP":
                        i = a;
                        break;
                    //条件跳转a
                    case "JPC":
                        if (stack[t] == 0)
                            i = a;
                        t--;
                        break;
                    //读数据并存入变量
                    case "RED":
                        int num = Convert.ToInt32(Console.ReadLine());
                        stack[getadd(l) + a] = num;
                        break;
                    case "WRT":
                        Console.WriteLine(stack[t]);
                        t++;
                        break;
                }
            } while (i != 0);

        }
    }

}
