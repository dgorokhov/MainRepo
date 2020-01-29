using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace GCalc1
{
    /// <summary>
    /// КОНСОЛЬНЫЙ КАЛЬКУЛЯТОР. Версия 0.1.
    /// Очень легко расширяется. В следующей версии напишу как.
    /// Пока поддерживает операции -+/*, возведение в степень ^.  
    /// Используйте "," в качестве десятичной точки. 
    /// 
    /// 
    /// ОБЩАЯ АРХИТЕКТУРА ПРОГРАММЫ
    /// Захотелось написать что-нибудь небольшое но интересное чтобы потренироваться в ООП и прочувствовать 
    /// весь SOLID в действии. Кажется получилось. Я добавил пояснения к коду, так что... добро пожаловать. 
    /// Конструктивная критика приветствуется.
    /// 
    /// Итак,
    /// Задача стояла следующим образом: нужен консольный, текстовый калькулятор, который бы умел считать
    /// длинные выражение с множеством скобок, видел ошибки, поддерживал числа в двоичной, шестнадцатеричной,
    /// ... а может и в 5-ичной (почему бы нет!) форме, имел ЛЕГКО РАСШИРЯЕМЫЙ список поддерживаемых функций
    /// например таких: найти минимальное из всех min (12,13,22,88...) - с любым настраиваемым числом параметров. Может матрицы?... 
    /// В общем главное - чтобы  расширить впоследствии функциональность такого девайса было легко 
    /// (все знают как иногда это сложно делать) 
    /// 
    /// 
    /// Давайте посмотрим на строку, которую этот калькулятор должен "переварить"
    ///  23.55 + 110101101b/0x12 - 28^(11*0.12) + aprogress(1,0.33,100)
    /// 
    /// aprogress - арифм прогрессия от 1 с шагом 0.33, 100 членов.
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// АБСТРАКТНЫЕ ТИПЫ
    /// NodeType - БАЗОВЫЙ ПРЕДОК.
    /// Является общей абстракцией (абстрактным классом без экземпляров) или интерфейсом.
    /// На базе этого класса стоится вся иерархия классов. Этот класс определяет основные свойства 
    /// смысловой единицы вычисления для краткости названной "узлом".
    /// т.е. например нужно вычислить 18*(19+20/3)^2. Узлом первого уровня является все выражение.
    /// Первый узел имеет дочерние узлы "18", "*", "19+20/3", "^", "2". Последний в данном случае 
    /// третий уровень это "19", "+", "20", "/", "3" котоорые могут быть вычислены непосрдственно.
    /// 
    /// </summary>
    public class NodeType
    {
        public NodeType(string symbolset, int len) { _symbolset = symbolset; _len = len; }
        public virtual bool CanBeFollowedBy(string symbols)
        {
            //Person match = personnel.Find((Person p) => { return p.ID == 3; });
            string match = _FollowedBy.Find((string s) => { return s == symbols; });
            if (match != null)
                return true;
            else
                return false;
        }
        public virtual int AmI(string substr)
        {
            int result = -1;
            if (_len == 0)
            {
                int i = 0;
                while (i <= substr.Length - 1)
                {
                    result = _symbolset.IndexOf(substr.Substring(i, 1));
                    if (result == -1)  break;
                    i++;
                }
                return i - 1;
            }
            if (_len == 1)
            {
                result = _symbolset.IndexOf(substr.Substring(0, 1));
                if (result == -1)
                    return -1;
                else
                    return 0;
            }

            return -1;  //error 
        }
        public virtual double Exec(Node node) { return 0.0; }

        protected List<string> _FollowedBy;
        protected string _symbolset;
        protected int _len;
    }

    // ПРОСТОЙ ТИП - КОТОРЫЙ ПОСЛЕ ВЫПОЛЕНИЯ AmI ПРОИЗВОДИТ ЧИСЛО.   
    public class Number : NodeType
    {
        public Number(string symbolset, int len)
            : base(symbolset, len)
        {
            _FollowedBy = new List<string>() { "GCalc1.BinaryOperation", "GCalc1.ArgumentDelimiter" };
        }
        /*public override bool CanBeFollowedBy(string symbols) {
            string match = _FollowedBy.Find((string s) => { return s == symbols; });
            if (match == null || symbols.IndexOf("(") != -1)
                return false;
            else
                return true;
        
        }
         * */
    }

    /// <summary>
    /// класс Operation ОПЕРАЦИЯ - математический (обычно) символ, которое обозначает математическую операцию. 
    /// унарную если берет 1 аргумент, бинарную (+-/*) - если 2, иногда используется тернарная (операция)
    /// ПРИМЕРЫ: (унарная операция 23! - "!" это факториал 23,  ~1011101b - операция NOT двоичного числа)
    /// В отличие от класса Compound который "ожидает увидеть" свои операнды заключенными в скобки перечисленными
    /// через ; у класса Operation операнды стоят рядом (один справа , другой слева от символа операции
    /// </summary>
    public class Operation : NodeType
    {
        public Operation(string symbolset, int len) : base(symbolset, len) { }
    }

    /// <summary>
    /// СЛОЖНЫЙ ОБЪЕКТ, Т.Е. ВЫЧИСЛЯЕМОЕ ВЫРАЖЕНИЕ (ВЫРАЖЕНИЕ ВКЛЮЧ ЩЕЕ ПРОСТЫЕ ТИПЫ (числа, операции  
    /// и др вычисляемые выражения) В СКОБКАХ 
    /// И БЕЗ НИХ, ФУНКЦИЯ 
    /// </summary>
    public class ArgumentDelimiter : NodeType
    {
        public ArgumentDelimiter()
            : base(";", 1)
        {
            _FollowedBy = new List<string>() { "GCalc1.DecimalNumber", "GCalc1.Compound" };
        }
    }
    public class Compound : NodeType
    {
        public Compound(string opensymbol, string closesymbol)
            : base(opensymbol + closesymbol, 1)
        {
            _FollowedBy = new List<string>() { "GCalc1.BinaryOperation", "GCalc1.ArgumentDelimiter" };
            _opensymbol = opensymbol;
            _closesymbol = closesymbol;
        }
        public string OpenSymbol { get { return _opensymbol; } }
        public string CloseSymbol { get { return _closesymbol; } }
        //public Compound(string symbolset, int len) : base(symbolset, len) { }
        public override double Exec(Node node)
        {
            List<Node> kids = node.kids;
            foreach (Node nd in node.kids)
            {
                double r = nd.Exec();
                if (r == Double.NaN)
                    return Double.NaN;
            }
            //вычислить бинарные операции
            //1. Сортировка - самые приоритетные операции вначале
            //2. Выполнение
            if (kids.Count == 1) { return kids[0].Exec(); }
            if (kids.Count == 2) { throw new FormatException("Unknown parsing error!"); }

            double temp = 0.0;
            while (kids.Count > 3)
            {
                if (((BinaryOperation)kids[1].nodetype).priority < ((BinaryOperation)kids[3].nodetype).priority)
                {
                    temp = ((BinaryOperation)kids[3].nodetype).Execute(kids[2].result, kids[4].result);
                    kids.RemoveAt(2);
                    kids.RemoveAt(2);
                    kids[2].result = temp;
                }
                else
                {
                    temp = ((BinaryOperation)kids[1].nodetype).Execute(kids[0].result, kids[2].result);
                    kids.RemoveAt(0);
                    kids.RemoveAt(0);
                    kids[0].result = temp;
                }
            }

            temp = ((BinaryOperation)kids[1].nodetype).Execute(kids[0].result, kids[2].result);
            kids[2].result = temp;
            return kids[2].result;
        }

        public override int AmI(string substr)
        {
            if (substr.IndexOf(_opensymbol) == 0 ||
                substr.IndexOf(_closesymbol) == 0)
            {
                return 0;
            }
            else return -1;

        }
        protected string _opensymbol;
        protected string _closesymbol;
    }

    /// <summary>
    /// ОБЪЕКТ ЯВЛ-СЯ ОДНИМ ИЗ ВИДОВ NODETYPE, 
    /// </summary>
    public class Node
    {
        public Node(NodeType nodetype, string str) { _nodetype = nodetype; _str = str; }
        public NodeType nodetype { get { return _nodetype; } }
        public List<Node> kids = null;
        public void AddKid(Node node)
        {
            if (kids == null)
                kids = new List<Node>();
            kids.Add(node);
        }
        public string str { get { return _str; } set { _str = value; } }
        public double result = Double.NaN;
        public double Exec()
        {
            double r = _nodetype.Exec(this);
            this.result = r;
            return r;
        }
        public int RecursiveLevel = 0;

        protected string _str;
        private NodeType _nodetype;

    }


    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////
    /// </summary>
    class DecimalNumber : Number
    {
        public DecimalNumber()
            : base(",0123456789", 0)
        {
            _FollowedBy = new List<string>() { "GCalc1.BinaryOperation", "GCalc1.ArgumentDelimiter" };
        }
        
        /*
         * public override int AmI(string substr)
        {
            int result;
            if (substr.Substring(0,1) == "-") 
                result = base.AmI(substr.Substring(1,substr.Length-1))+1;
            else
                if (_symbolset.IndexOf(substr.Substring(0,1)) != -1) 
                    result =base.AmI(substr);
                else result=-1;

            return result;
        }
         * 
         */
        public override double Exec(Node node)
        {
            return Convert.ToDouble(node.str);
        }
    }

    enum Priority { prLow, prMiddle, prHigh }

    class BinaryOperation : Operation
    {
        public BinaryOperation(string operation)
            : base(operation, 1)
        {
            _FollowedBy = new List<string>() { "GCalc1.DecimalNumber", "GCalc1.Compound", "GCalc1.Function" };
            _operation = operation;
            switch (_operation)
            {
                case "+": _priority = Priority.prLow; break;
                case "-": _priority = Priority.prLow; break;
                case "*": _priority = Priority.prMiddle; break;
                case "/": _priority = Priority.prMiddle; break;
                case "^": _priority = Priority.prHigh; break;
                default:
                    break; // must be error
            }
        }
        private string _operation;
        private Priority _priority;
        public Priority priority
        {
            get { return _priority; }
            set { }
        }


        public double Execute(double oper1, double oper2)
        {
            double result = 0.0;
            switch (_operation)
            {
                case "+": result = oper1 + oper2; break;
                case "-": result = oper1 - oper2; break;
                case "*": result = oper1 * oper2; break;
                case "/": result = oper1 / oper2; break;
                case "^": result = Math.Pow(oper1, oper2); break;
                default:
                    result = Double.NaN;
                    break;
            }
            return result;
        }

    }

    /// <summary>
    // РЕАЛЬНЫЕ ТИПЫ
    /// </summary>

    class MainNodeType : Compound
    {
        public MainNodeType() : base(".", ".") { }
    }
    // + - / *  ^


    class Function : Compound
    {
        protected int _callcounter = 0;
        public Function(string name, int argcount)
            : base(name, ")")
        {
            _FollowedBy = new List<string>() { "GCalc1.BinaryOperation" };
            this._name = name;
            this._argcount = argcount;
        }
        public string _name;
        public int _argcount;

        public override bool CanBeFollowedBy(string symbols)
        {
            //:base CanBeFollowedBy(symbols) 
            _callcounter++;
            switch (_name)
            {
                case "min(": if (_callcounter % 2 != 0)
                    {
                        if (symbols != ";")
                            return false;
                        else
                            return true;
                    }
                    else { return true; }
                    break;
                default:
                    break;
            }
            return true;
        }
        public override int AmI(string str)
        {
            // если не совпало имя
            if (str.Substring(0, this._name.Length) != this._name)
            {
                return -1;
            }
            else
            {
                return _name.Length - 1;
            }

        }
        public override double Exec(Node node)
        {

            double result = double.NaN;
            switch (_name)
            {
                case "sqr(": result = node.kids[0].Exec() * node.kids[0].Exec(); break;
                case "sqrt(": result = Math.Sqrt(node.kids[0].Exec()); break;
                case "min(":
                    double nk1 = node.kids[0].Exec();
                    double nk2 = node.kids[1].Exec();
                    if (nk1 > nk2) return nk2; else return nk1;
                    break;

                default:
                    break;
            }

            return result;
        }

    }


    public class Calculator
    {

        public Calculator() { }
        Node MainNode;
        private List<NodeType> _nodetypes = new List<NodeType>();
        public void RegisterNodeType(NodeType node)
        {
            _nodetypes.Add(node);
        }

        /// </summary>
        /// <param name="str">СТРОКА</param>
        /// <param name="nextpos">ВОЗВРАЩАЕТ nodetype найденного с НАЧАЛА строки, pos=ПОСЛЕДНЯЯ СОВПАВШАЯ+1</param>
        /// <returns></returns>
        protected NodeType ScreeningNodeType(ref string str, ref int pos)
        {
            //string substr = str.Substring(pos);
            int hypo_len = -1;  //"стартовая длина" найденного Node вначале пооиска
            int maxpos = -1;  //перемення куда будет записываться макс знач длины строки
            NodeType type = null;
            foreach (NodeType el in _nodetypes)
            {
                maxpos = el.AmI(str);
                //maxpos содержит индекс последнего имвола строки str удовлетвор-го NodeType
                if ((maxpos != -1) && (hypo_len < maxpos))
                {   //node type is recognized
                    hypo_len = maxpos;
                    type = el;
                    break;
                }
            }
            if (hypo_len == -1)
            {   //Parsing Error!
                pos = -1;
                return null;
            }
            else
            {
                pos = +hypo_len + 1;
                return type;
            }
        }


        public void Parse(ref string str)
        {
            //ParseNext("("+str+")");
            str += ".";
            MainNode = new Node(new MainNodeType(), str);
            ParseKids(ref MainNode, ref str);
        }

        //процедура парсинга. Создает дерево из соответствующих узлов NodeType, которое потом
        //вычисляет
        public void ParseKids(ref Node selfnode, ref string str)
        {
            NodeType prevnodetype = null;
            NodeType nodetype = selfnode.nodetype;
            NodeType kidnodetype = null;
            string prevfoundstr = "";
            int nextpos = 0;
            while (str.Length > 0 && str.Substring(0, 1) != ((Compound)selfnode.nodetype).CloseSymbol)
            {

                kidnodetype = ScreeningNodeType(ref str, ref nextpos);

                if (kidnodetype == null)
                    throw new FormatException("Parsing error, unknown symbols: " + str);

                string foundstr = str.Substring(0, nextpos);

                if (prevnodetype != null)
                {
                    if (!prevnodetype.CanBeFollowedBy(kidnodetype.GetType().ToString()))
                        throw new FormatException("Parsing error: " + prevfoundstr + " can't be followed by " + foundstr);
                }

                Node kidnode = new Node(kidnodetype, foundstr);
                if (!(kidnodetype is ArgumentDelimiter))
                {
                    selfnode.AddKid(kidnode);
                    //nextpos++;
                }
                str = str.Substring(nextpos, str.Length - nextpos);
                prevnodetype = kidnodetype;
                prevfoundstr = foundstr;

                if (kidnodetype is Compound)
                {
                    kidnode.RecursiveLevel = selfnode.RecursiveLevel + 1;
                    ParseKids(ref kidnode, ref str);//, Compound.CloseSymbol);
                    nextpos = 1;
                }
            }

            if (str.Length == 0)
            {
                if (selfnode.RecursiveLevel > 0)
                {
                    throw new FormatException("Parsing error: " +
                        ((Compound)selfnode.nodetype).CloseSymbol + " not found !");
                }
            }
            else
            {
                str = str.Substring(1);
            }


            foreach (Node el in selfnode.kids)
            { Console.WriteLine(selfnode.RecursiveLevel.ToString() + "  " + el.str); }

        }
        
        public void CorrectHangingMinuses(ref string r)
        {
            //заменить висящие "-" на 0-
            int i = 0;
            if (r.Substring(0, 1) == "-") r = "0" + r;
            while (i < r.Length)
            {
                if ((r.Substring(i, 1) == "-") && (r.Substring(i - 1, 1) == "("))
                {
                    r = r.Substring(0, i) + "0" + r.Substring(i, r.Length - i);
                }
                i++;
            }
        }

        public double Exec()
        {
            return MainNode.Exec();
        }
        class Program
        {
            static void Main(string[] args)
            {

                Calculator Calc = new Calculator();
                Calc.RegisterNodeType(new DecimalNumber());
                Calc.RegisterNodeType(new ArgumentDelimiter());
                Calc.RegisterNodeType(new BinaryOperation("+"));
                Calc.RegisterNodeType(new BinaryOperation("-"));
                Calc.RegisterNodeType(new BinaryOperation("*"));
                Calc.RegisterNodeType(new BinaryOperation("/"));
                Calc.RegisterNodeType(new BinaryOperation("^"));
                Calc.RegisterNodeType(new Compound("(", ")"));
                Calc.RegisterNodeType(new Function("sqr(", 1));
                Calc.RegisterNodeType(new Function("sqrt(", 1));
                Calc.RegisterNodeType(new Function("min(", 1));

                
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("Calculator:");
                    String r = Console.ReadLine();
                    
                    try
                    {
                        r = r.Replace(" ", "");
                        if (r.Length > 1)
                        {
                            Calc.CorrectHangingMinuses(ref r);
                            Calc.Parse(ref r);
                            Console.WriteLine("Result: " + Convert.ToString(Calc.Exec()));
                        }
                        else
                            exit = true;

                    }
                    catch (FormatException fex)
                    { Console.WriteLine(fex.Message); }
                    catch (Exception fex)
                    { Console.WriteLine(fex.Message); }
                    finally
                    {

                    }
                }
                //string OnExit = Console.ReadLine();
            }
        }
    }

}
