///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules to extract type information     //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Project #3                                   //
// Author:      Yilin Cui, ycui21@syr.edu                            //
// Origin:      Dr. Jim Fawcett's website                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains code for type inforamtion extraction
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectType rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   - DetectType rule
 *   - DetectDelegate rule
 *   - DetectPubDeclar rule
 *   - DetectUsingNameSpace rule
 *   - DetectLeavingScope rule
 *   - DetectNewOperator rule
 *   - DetectProperty rule
 *   - DetectEnd rule
 *   - DetectInheritance rule
 * 
 *   
 *   Three actions - some are specific to a parent rule:
 *   - PushStack
 *   - SaveAll
 *   - CollectNamespace
 *   - CollectAliases
 *   - PopStack
 *   - PrintFunction
 *   - CollectMember
 *   - PrintSemi
 *   - CollectReturn
 *   - CollectNew
 *   - SaceDeclar
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 * Also procides an SCCoperation to produce dependency information needed
 * to produce SCC.
 * Also provides two builder class:
 * - BuildDepedencyAnalizer
 * - BuildTyoeAnalyzer
 * 
 * Required files:
 * Display.cs, Element.cs, SemiExp.cs, Toker.cs, TypeTable.cs, CsGraph.cs
 * IRuleandAction.cs, Parser.cs, RulesAndActions.cs, ScopeStack.cs
 * 
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 10/29/2018
 * - first release
 *   refactor from Dr Jim Fawcett's package
 *
 */
using CsGraph;
using Lexer;
using System;
using System.Collections.Generic;
using System.Text;
using TypeFile;
namespace CodeAnalysis
{
    //////////////////////////////////////////////////////////////
    //SCCoperation: used for produce a table for SCC construction
    //

    class SCCoperation : Operation<string, string>
    {
        private Dictionary<string, List<string>> depTable;
        public SCCoperation(ref Dictionary<string, List<string>> dic)
        {
            depTable = dic;
        }

        //---------------<Save node to the table>---------------
        override public bool doNodeOp(CsNode<string, string> node)
        {
            if (depTable.ContainsKey(node.ToString())) return false;
            List<string> temp = new List<string>();
            depTable.Add(node.ToString(), temp);
            return true;
        }

        //---------------<Save edge to the table>---------------
        public override bool doEdgeOp(string edgeVal)
        {
            int i = edgeVal.IndexOf('-');
            string parent = edgeVal.Substring(0, i);
            string child = edgeVal.Substring(i + 1, edgeVal.Length-i-1);
            depTable[parent].Add(child);
            return true;

        }
    }

    ///////////////////////////////////////////////////////////////////
    // Repository class
    // - Specific to each application
    // - holds results of processing
    // - ScopeStack holds current state of scope processing
    // - List<Elem> holds start and end line numbers for each scope
    // - getInstance will return the instance of the repository
    // - graph_ store dependency information
    // - nameSpace stores the namespaces every file use
    // - aliasTable stores aliases the type it represents
    // - typeTable_ stores type informations
    ///////////////////////////////////////////////////////////////////

    public class Repository
    {
        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();
        internal CsGraph<string, string> graph_ ;
        public TypeTable typeTable { get; set; }
        public Dictionary<string, List<string>> aliasTable { get; set; }
        public Dictionary<string,List<string>> nameSpace { get; set; }

        static Repository instance;

        public Repository()
        {
            if (graph_ == null)
                graph_ = new CsGraph<string, string>("Dependency Graph");
            if (typeTable == null)
                typeTable = new TypeTable();
            if (aliasTable == null)
                aliasTable = new Dictionary<string, List<string>>();
            if (nameSpace == null)
                nameSpace = new Dictionary<string, List<string>>();
        }

        //----< provides all code access to Repository >-------------------
        //----A lazy not-thread-safe singleton, will update later
        public static Repository getInstance()
        {
            if (instance == null)
                instance = new Repository();
            return instance;
        }
       

        //----< provides all actions access to current semiExp >-----------

        public ITokenCollection semi
        {
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source

        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount(); }
        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }

        //----< enables recursively tracking entry and exit from scopes >--

        public int scopeCount
        {
            get;
            set;
        }

        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }

        // the locations table is the result returned by parser's actions
        // in this demo

        public List<Elem> locations
        {
            get { return locations_; }
            set { locations_ = value; }
        }

        //-------------<Add node to the graph>-----------
        public void addNode(CsNode<string, string> node)
        {
            graph_.addNode(node);
        }

        //-------------------<Add type collected to the type table>---------------
        public bool addType(TypeInfo info, string type)
        {
            return typeTable.Add(type, info);
        }

        //-------------------<Add type collected to the type table>---------------
        public void addTypes(List<TypeInfo> ti,string type)
        {
            if (typeTable.Contains(type))
            {
                foreach (var info in ti)
                {
                   typeTable[type].Add(info);
                }
            }
            else
            {
                typeTable.table().Add(type, ti);
            }
        }

        //-----------------<Print out the type information collected>---------------
        public void printTypeTable()
        {
            typeTable.print();
        }

        //-------------------<Add alias collected to the alias table>--------------
        public void addAlias(string alias,string type)
        {
            if(aliasTable.ContainsKey(alias))
            {
                aliasTable[alias].Add(type);
            }
            else
            {
                List<string> temp = new List<string>();
                temp.Add(type);
                aliasTable.Add(alias, temp);
            }
        }

        //------------------<walk the graph>-------------------------
        public void WalkTheGraph()
        {
            graph_.walk();
        }

        //------------------<Set the graph operation to SCC collection>------------
        public void Setoperation(ref Dictionary<string, List<string>> dic)
        {
            graph_.setOperation(new SCCoperation(ref dic));
        }

        //------------------<Add namespace used by individual file>--------------
        public void addNameSpace(string file,string nameSp)
        {
            if(nameSpace.ContainsKey(file))
            {
                nameSpace[file].Add(nameSp);
            }
            else
            {
                List<string> temp = new List<string>();
                temp.Add(nameSp);
                nameSpace.Add(file, temp);
            }
        }

        //------------------<Print the namespace collected from files>-----------------
        public void printNameSpace()
        {
            foreach(KeyValuePair<string,List<string>>pair in nameSpace)
            {
                Console.WriteLine("File name:\n{0}\nNamespace:", pair.Key);
                foreach(string ns in pair.Value)
                {
                    Console.Write("{0} ",ns);
                }
                Console.Write("\n");
             
            }
        }

        //----------------------<Map the alias to the type table>---------------------
        public void MapAlias()
        {
            foreach(KeyValuePair<string,List<string>>pair in aliasTable)
            {
                foreach(string type in pair.Value)
                {
                    if(typeTable.Contains(type))
                    {
                        foreach(TypeInfo info in typeTable[type])
                        {
                            typeTable.Add(pair.Key, info);
                        }
                    }
                }
            }
        }

        //----------------<Reset the repository,prepare for new round of operation>-----------
        public void Reset()
        {
            typeTable.clear();
            graph_.Reset();
            aliasTable.Clear();
            nameSpace.Clear();
        }

       


        //--------------------<Add dependecy based on the type retrieved>---------------
        public void AddDep(string type, ref CsNode<string,string>currentNode)
        {
            if (type == "Constructor") return;
            if (!typeTable.Contains(type)) return;
            if (typeTable[type].Count == 1)
            {
                CsNode<string, string> child = new CsNode<string, string>(typeTable[type][0].file);
                child.nodeValue = child.name;
                if (child.name != currentNode.name && !currentNode.ContainsChild(child.name))
                {
                    StringBuilder edge = new StringBuilder();
                    edge.Append(currentNode.name).Append("-").Append(child.name);
                    currentNode.addChild(child, edge.ToString());
                }
            }
            else
            {
                foreach (TypeInfo ti in typeTable[type])
                {
                    if (nameSpace[type].Contains(ti.nameSpace))
                    {
                        CsNode<string, string> child = new CsNode<string, string>(typeTable[type][0].file);
                        child.nodeValue = child.name;
                        if (child.name != currentNode.name && !currentNode.ContainsChild(child.name))
                        {
                            StringBuilder edge = new StringBuilder();
                            edge.Append(currentNode.name).Append(" - ").Append(child.name);
                            currentNode.addChild(child, edge.ToString());
                        }
                    }
                }
            }

        }



    }
    ///////////////////////////////////////////////////////////////////
    // Define Actions
    ///////////////////////////////////////////////////////////////////
    // - PushStack
    // - SaveAll
    // - CollectNamespace
    // - CollectAliases
    // - PopStack
    // - PrintFunction
    // - CollectMember
    // - PrintSemi
    // - CollectReturn
    // - CollectNew
    // - SaceDeclar

    ///////////////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope
    // - pushes element with type and name onto stack
    // - records starting line number

    public class PushStack : AAction
    {
        public PushStack(Repository repo)
        {
            repo_ = Repository.getInstance();
        }

        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action PushStack");
            ++repo_.scopeCount;
            Elem elem = new Elem();
            elem.type = semi[0];     // expects type, i.e., namespace, class, struct, ..
            elem.name = semi[1];     // expects name
            elem.beginLine = repo_.semi.lineCount() - 1;
            elem.endLine = 0;        // will be set by PopStack action
            elem.beginScopeCount = repo_.scopeCount;
            elem.endScopeCount = 0;  // will be set by PopStack action
            repo_.stack.push(elem);

            // display processing details if requested

            if (AAction.displayStack)
                repo_.stack.display();
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount() - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }

            // add starting location if namespace, type, or function

            if (elem.type == "control" || elem.name == "anonymous")
                return;
            repo_.locations.Add(elem);
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Save all the dependency information collected when reach the end

    public class SaveAll:AAction
    {
        private CsNode<string, string> currentNode_;
        public SaveAll(ref CsNode<string, string> node)
        {
            repo_ = Repository.getInstance();
            currentNode_ = node;
        }
        public override void doAction(ITokenCollection semi)
        {
            if(repo_.semi.isDone())
            {
                repo_.addNode(currentNode_);
                repo_.graph_.startNode=currentNode_;
            }
        }
    }

    /////////////////////////////////////////////////
    // Collect the namespaces used by a file
    //
    public class CollectNamespace : AAction
    {
        private string file_ = null;
        public CollectNamespace(Repository repo,string file)
        {
            repo_ = Repository.getInstance();
            file_ = file;
        }
        public override void doAction(ITokenCollection semi)
        {
            StringBuilder nameSP = new StringBuilder();
            foreach (string tok in semi)
            {
                if (tok == "System") break;
                nameSP.Append(tok);
            }
            if (nameSP.Length == 0)
            { repo_.addNameSpace(file_, ""); return; }
            repo_.addNameSpace(file_, nameSP.ToString());
           
        }
    }
    
    ///////////////////////////////////////////////////////////////////
    // collect aliases and the type name
    public class CollectAliases : AAction
    {
        public CollectAliases(Repository repo)
        {
            repo_ = Repository.getInstance();
        }
        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action CollectAliases");
            repo_.addAlias(semi[1], semi[2]);
        }
    }


    ///////////////////////////////////////////////////////////////////
    // pops scope info from stack when leaving scope
    // - records end line number and scope count
    // - extract the type information and store in type table

    public class PopStack : AAction
    {
        private string file_;
        public PopStack(Repository repo, string file)
        {
            repo_ = Repository.getInstance();
            file_ = file;
        }

        //------------<Private help function, extract the type information to type table
        private void extractType(Elem elem)
        {
            if (elem.type == "class" || elem.type == "struct" || elem.type == "enum" || elem.type == "interface" || elem.type == "delegate")
            {
                TypeInfo tempT = new TypeInfo();
                tempT.file = file_;
                StringBuilder type = new StringBuilder();
                for (int i = repo_.stack.count - 1; i >= 0; --i)
                {
                    if (repo_.stack[i].type != "namespace")
                    {
                        type.Insert(0, ".");
                        type.Insert(0, repo_.stack[i].name);

                    }
                    else
                    {
                        tempT.nameSpace = repo_.stack[i].name;
                        break;
                    }
                }
                type.Append(elem.name);
                repo_.addType(tempT, type.ToString());
            }
        }

        public override void doAction(ITokenCollection semi)
        {

            Display.displayActions(actionDelegate, "action SaveDeclar");
            Elem elem;
            try
            {
                elem = repo_.stack.pop();
                TypeInfo tempT = new TypeInfo();
                tempT.file = file_;
                extractType(elem);
                for (int i = 0; i < repo_.locations.Count; ++i)
                {
                    Elem temp = repo_.locations[i];
                    if (elem.type == temp.type)
                    {
                        if (elem.name == temp.name)
                        {
                            if ((repo_.locations[i]).endLine == 0)
                            {
                                (repo_.locations[i]).endLine = repo_.semi.lineCount();
                                (repo_.locations[i]).endScopeCount = repo_.scopeCount;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                return;
            }

            if (AAction.displaySemi)
            {
                Lexer.ITokenCollection local = Factory.create();
                local.add(elem.type).add(elem.name);
                if (local[0] == "control")
                    return;

                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount());
                Console.Write("leaving  ");
                string indent = new string(' ', 2 * (repo_.stack.count + 1));
                Console.Write("{0}", indent);
                this.display(local); // defined in abstract action
            }
        }
    }

    ///////////////////////////////////////////////////////////////////
    // action to print function signatures - not used in demo

    public class PrintFunction : AAction
    {
        public PrintFunction(Repository repo)
        {
            repo_ = Repository.getInstance();
        }
        public override void display(Lexer.ITokenCollection semi)
        {
            Console.Write("\n    line# {0}", repo_.semi.lineCount() - 1);
            Console.Write("\n    ");
            for (int i = 0; i < semi.size(); ++i)
            {
                if (semi[i] != "\n")
                    Console.Write("{0} ", semi[i]);
            }
            
        }
        public override void doAction(ITokenCollection semi)
        {
            this.display(semi);
        }
    }

    ///////////////////////////////////////////////////////////////////
    // CollectMember action, collect data member of the class

    public class CollectMember : AAction
    {
        private CsNode<string, string> currentNode_;
        public CollectMember(ref CsNode<string,string> node)
        {
            repo_ = Repository.getInstance();
            currentNode_ = node;
        }
        public override void doAction(ITokenCollection semi)
        {
            repo_.AddDep(semi[0],ref currentNode_);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // ITokenCollection printing action, useful for debugging

    public class PrintSemi : AAction
    {
        public PrintSemi(Repository repo)
        {
            repo_ = Repository.getInstance();
        }
        public override void doAction(ITokenCollection semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount() - 1);
            this.display(semi);
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////
    // action to retrive the dependecny information based on the return type of function
    public class CollectReturn : AAction
    {
        private CsNode<string, string> currentNode_;
        public CollectReturn(ref CsNode<string,string > node)
        {
            currentNode_ = node;
            repo_ = Repository.getInstance();
        }
        public override void doAction(ITokenCollection semi)
        {

            int i = 2;
            while(i<semi.size())
            {
                StringBuilder type = new StringBuilder();
                if(semi[i]=="<"|| semi[i] == ">" || semi[i] == ">>" || semi[i] == ",")
                {
                    i++; continue;
                }
                while (i < semi.size() && semi[i] != "<" && semi[i] != ">" && semi[i] != ">>" && semi[i] != ",")
                { type.Append(semi[i]); i++; }
                repo_.AddDep(type.ToString(), ref currentNode_);
                i++;
            }

        }
    }

    ///////////////////////////////////////////////////////////////////
    // Collect type with new operator
    public class CollectNew : AAction
    {
        private CsNode<string, string> currentNode_;
        public CollectNew(ref CsNode<string, string> node)
        {
            currentNode_ = node;
            repo_ = Repository.getInstance();
        }
        public override void doAction(ITokenCollection semi)
        {
            StringBuilder type = new StringBuilder();
            type.Append(semi[0]);
            int i = 1;
            while (i < semi.size())
            {
                type.Append(".");
                type.Append(semi[i]);
                i++;
            }
            repo_.AddDep(type.ToString(), ref currentNode_);

        }
    }

    ///////////////////////////////////////////////////////////////////
    // display public declaration

    public class SaveDeclar : AAction
    {
        public SaveDeclar(Repository repo)
        {
            repo_ = Repository.getInstance();
        }
        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action SaveDeclar");
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.beginLine = repo_.lineCount;
            elem.endLine = elem.beginLine;
            elem.beginScopeCount = repo_.scopeCount;
            elem.endScopeCount = elem.beginScopeCount;
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.lineCount - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            repo_.locations.Add(elem);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Define Rules
    ///////////////////////////////////////////////////////////////////
    // - DetectNameSpace
    // - DetectFunction
    // - DetectType
    // - DetectDelegate
    // - DetectPubDeclar
    // - DetectUsingNameSpace
    // - DetectLeavingScope 
    // - DetectNewOperator
    // - DetectProperty
    // - DetectEnd rule
    // - DetectInheritance

    ///////////////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectNamespace");
            int index;
            semi.find("namespace", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }


    ///////////////////////////////////////////////////////////////////
    // rule to detect alias declarations
    public class DetectAlias : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectAlias");
            int index;
            if(semi.find("using",out index)&&!semi.find("System",out index)&&semi.find("=",out index))
            {
                ITokenCollection local = Factory.create();
                local.add("alias").add(semi[index - 1]);
                StringBuilder ConcreType = new StringBuilder();
                string alias = semi[index - 1];
                index++;
                while(semi[index]!=";"&& semi[index] !="<")
                {
                    ConcreType.Append(semi[index]);
                    index++;
                }
                string type = ConcreType.ToString();
                local.add(ConcreType.ToString());
                doActions(local);
                return true;
            }
            return false;
        }

    }
    ///////////////////////////////////////////////////////////////////
    // rule to dectect type definitions

    public class DetectType : ARule
    {
        
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectClass");
            
                int indexCL;
                semi.find("class", out indexCL);
                int indexIF;
                semi.find("interface", out indexIF);
                int indexST;
                semi.find("struct", out indexST);
                int indexEN;
                semi.find("enum", out indexEN);
                int index = Math.Max(indexCL, indexIF);
                index = Math.Max(index, indexST);
                if (index != -1 && semi.size() > index + 1)
                {
                    ITokenCollection local = Factory.create();
                    // local semiExp with tokens for type and name
                    local.add(semi[index]).add(semi[index + 1]);
                    doActions(local);
                    return true;
                }
                return false;

        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to dectect delegate
    public class DetectDelegate : ARule
    {

        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectClass");
            int i;
            if (semi.find("public", out i) || semi.find("internal", out i))//may not need here
            {
                int index;
                semi.find("delegate", out index);
                if (index != -1)
                {
                    ITokenCollection local = Factory.create();
                    // local semiExp with tokens for type and name
                    local.add(semi[index]).add(semi[index + 2]);
                    doActions(local);
                    return true;
                }
                return false;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to dectect function definitions

    public class DetectFunction : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }

        //---------------<Extract the semi expression of returned type>-------------
        private ITokenCollection GetReturn(ITokenCollection semi,string functionName)
        {
            ITokenCollection current=Factory.create();            
            List<string> decoration = new List<string>{"public","private","internal","protected","static","virtual","static","override" };
            for(int i=0;i<semi.size();++i)
            {
                if (decoration.Contains(semi[i]))
                    continue;
                if (semi[i] != functionName)
                {

                    current.add(semi[i]);
                    
                }
                else break;
            }
            if (current.size() == 0)
                current.add("Consturctor");
          
            return current;

        }
        //---------------<Extract the semi expression of parameter>-------------
        private ITokenCollection getPara(ITokenCollection semi)
        {
            ITokenCollection current = Factory.create();
            int index;
            semi.find("(", out index);
            index++;
            while(index<semi.size())
            {
                if (semi[index]!= "}") 
                    current.add(semi[index]);
                index++;
            }
            return current;
        }
       
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectFunction");
            if (semi[semi.size() - 1] != "{")
                return false;

            int index;
            semi.find("(", out index);
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                ITokenCollection local = Factory.create();
                local.add("function").add(semi[index - 1]);
                ITokenCollection returnType = GetReturn(semi, semi[index - 1]);
                int i = 0;
                while(i<returnType.size())
                {
                    local.add(returnType[i]);
                    i++;
                }
                ITokenCollection para = getPara(semi);
                int j = 0;
                while(j<para.size())
                {
                    local.add(para[j]);
                    j++;
                }
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those

    public class DetectAnonymousScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectAnonymousScope");
            int index;
            semi.find("{", out index);
            if (index != -1)
            {
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                local.add("control").add("anonymous");
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect public declaration

    public class DetectPubDeclar : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            
            Display.displayRules(actionDelegate, "rule   DetectPubFuncDeclar");
            int index;
            semi.find(";", out index);
            if (index != -1)
            {
                semi.find("public", out index);
                if (index == -1)
                {
                    
                    return true;
                }
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                //local.displayNewLines = false;
                local.add("public " + semi[index + 1]).add(semi[index + 2]);

                semi.find("=", out index);
                if (index != -1)
                {
                    doActions(local);
                    return true;
                }
                semi.find("(", out index);
                if (index == -1)
                {
                    doActions(local);
                    return true;
                }
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    //detect using namespace
    public class DetectUsingNamespace : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectUsingNamespace");
            int index;
            if(!semi.find("=",out index)&&semi.find("using",out index))
            {
                ITokenCollection local = Factory.create();
                index++;
                while(semi[index]!=";")
                {
                    local.add(semi[index]);
                    index++;
                }
                doActions(local);
                return true;
            }
            return false;
        }

    }
    ///////////////////////////////////////////////////////////////////
    // detect leaving scope

    public class DetectLeavingScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectLeavingScope");
            int index;
            semi.find("}", out index);
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }
    //////////////////////////////////////////////
    //Detect new operator
    //
    public class DetectNewOperator : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectNewOperator");
            int index;
            if(semi.find("=", out index) && semi.find("new", out index))
            {
                ITokenCollection local1 = Factory.create();
                index++;
                while (index<semi.size()&&semi[index] != "(")
                { local1.add(semi[index]);
                    index++;
                }
                index = local1.size()-1;
                while (index >= 0)
                {
                    List<string> acc = new List<string> { "public", "private", "internal", "static" };
                    //public List<CsEdge<CsNode<List<CsEdge<string,string>>,A.string>, string>> dummy {;
                    if (local1[index] == "<" || local1[index] == ">>" || local1[index] == ">" || local1[index] == "," || acc.Contains(local1[index]))
                    { index--; continue; }
                    ITokenCollection local = Factory.create();
                    StringBuilder temp = new StringBuilder();
                    while (index >= 0 && local1[index] != "<" && local1[index] != ">" && local1[index] != "," && !acc.Contains(local1[index]))
                    {
                        temp.Insert(0, local1[index]);
                        index--;
                    }
                    local.add(temp.ToString());
                    doActions(local);
                    index--;

                }
                return true;
            }
            return false;
        }
    }

    ////////////////////////////////////////////
    //Detect the class member declaration
    public class DetectProperty : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule    DetectMember");
            int index;
            semi.find("{", out index);
            if (index < 2) return false;
            string[] Dec = { "namespace","class","interface","struct","enum","(" ,"]"};
            List<string> acc = new List<string>{ "public", "private", "internal", "static" };
            foreach(string term in Dec)
            {
                if (semi.find(term, out index))
                    return false;
            }
            semi.find("{", out index);
            index -= 2;
            while(index>=0)
            {
                //public List<CsEdge<CsNode<List<CsEdge<string,string>>,A.string>, string>> dummy {;
                if (semi[index] == "<" || semi[index] == ">>" || semi[index] == ">" || semi[index] == ","|| acc.Contains(semi[index]))
                { index--; continue; }
                ITokenCollection local = Factory.create();
                StringBuilder temp = new StringBuilder();
                while(index >= 0&&semi[index] != "<" && semi[index] != ">" && semi[index] != ","&&!acc.Contains(semi[index]))
                {
                    temp.Insert(0, semi[index]);
                    index--;
                }
                local.add(temp.ToString());
                doActions(local);
                index--;
                

            }
            return true;

        }
    }


    //////////////////////////////////////////////
    // Detect the end of the file

    public class DetectEnd:ARule
    {
        public override bool test(ITokenCollection semi)
        {
            if (semi.isDone())
            {
                doActions(semi);
                return true;
            }
            return false;
            
        }
    }
   
    ///////////////////////////////////////////////////
    // Detect inheritance
    //
    public class DetectInheritance:ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int indexIT;
            semi.find("interface", out indexIT);
            int indexCL;
            semi.find("class", out indexCL);
            if (Math.Max(indexIT, indexCL) < 0) return false;
            int indexInh;
            if(semi.find(":",out indexInh))
            {
                indexInh++;
                ITokenCollection local = Factory.create();
                StringBuilder inhClass = new StringBuilder();
                while(indexInh<semi.size()&&semi[indexInh] !="{"&&semi[indexInh]!="<")
                {
                    inhClass.Append(semi[indexInh]);
                    indexInh++;
                }
                local.add("Inheritance").add("class").add(inhClass.ToString());
                doActions(local);
                return true;
            }
            return false;

        }
    }
 

    ///////////////////////////////////////////////////////////////////
    // BuildDependencyAnalyzer Class
    ///////////////////////////////////////////////////////////////////

    public class BuildDependencyAnalyzer
    {
        Repository repo = Repository.getInstance();
        private string file_ = null;
        private CsNode<string, string> currentNode;

        //------------------<Build up dependency analyzer>---------------
        public BuildDependencyAnalyzer(Lexer.ITokenCollection semi, 
        string file)
        {
            repo.semi = semi;
            file_ = file;
            currentNode = new CsNode<string, string>(file);
            currentNode.nodeValue = file;
            currentNode.name = file;       
            
        }
        public virtual Parser build()
        {         
            Parser parser = new Parser();
            AAction.displaySemi = false;
            AAction.displayStack = false;
            DetectProperty detectME = new DetectProperty();
            CollectMember collectME = new CollectMember(ref currentNode);
            detectME.add(collectME);
            parser.add(detectME);      
            DetectNewOperator detectNO = new DetectNewOperator();
            detectNO.add(collectME);
            parser.add(detectNO);
            DetectFunction detectFN = new DetectFunction();
            CollectReturn collectRT = new CollectReturn(ref currentNode);
            detectFN.add(collectRT);
            parser.add(detectFN);
            DetectInheritance detectIN = new DetectInheritance();
            detectIN.add(collectRT);
            parser.add(detectIN);
            DetectEnd detectEN = new DetectEnd();
            SaveAll sa = new SaveAll(ref currentNode);
            detectEN.add(sa);
            parser.add(detectEN);
            return parser;
        }
       
    }
    ///////////////////////////////////////////////////////////////////
    // BuildTypeAnalyzer class
    ///////////////////////////////////////////////////////////////////

    public class BuildTypeAnalyzer
    {
        Repository repo = Repository.getInstance();
        private string file_ = null;


        //--------------<Build up type anlyzer>--------------
        public BuildTypeAnalyzer(Lexer.ITokenCollection semi, string file)
        {
            repo.semi = semi;
            file_ = file;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();
            PopStack pop = new PopStack(repo, file_);
            AAction.displaySemi = false;
            AAction.displayStack = false; 
            PushStack push = new PushStack(repo);
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);
            DetectType detectTP = new DetectType();
            detectTP.add(push);
            parser.add(detectTP);
            DetectDelegate detectDE = new DetectDelegate();
            detectDE.add(push);
            detectDE.add(pop);
            parser.add(detectDE);
            DetectFunction detectFN = new DetectFunction();
            detectFN.add(push);
            parser.add(detectFN);
            DetectAnonymousScope anon = new DetectAnonymousScope();
            anon.add(push);
            parser.add(anon);
            DetectAlias detectAL = new DetectAlias();
            CollectAliases collectAL = new CollectAliases(repo);
            detectAL.add(collectAL);
            parser.add(detectAL);
            DetectLeavingScope leave = new DetectLeavingScope();
            leave.add(pop);
            parser.add(leave);
            DetectUsingNamespace detectUN = new DetectUsingNamespace();
            CollectNamespace collectNM = new CollectNamespace(repo, file_);
            detectUN.add(collectNM);
            parser.add(detectUN);
            return parser;
        }


    }
}

