using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TypeFile
{
    
    ////////////////////////////////////////////////////////////
    // struct TypeInfo: holds the information of certain type
    // name for the file where the type belongs
    // nameSpace for the nameSpace where the type is declared

    public class TypeInfo
    {
        public string file { get; set; }
        public string nameSpace { get; set; }
        

        // -------------------<Print the type information, for demonstration use>-----------------
        public void print()
        {
            Console.WriteLine("File: {0,-20}, Namespace: {1,-20}", file, nameSpace);
        }
        
    }

    //////////////////////////////////////////////////////////////////////////
    // class TypeTable holds the type information across the different files
    // each type has its' own list abot the files and namespace it's declared
    // 

    public class TypeTable
    {
        //--------------<Container for type table>------------------
        public Dictionary<string, List<TypeInfo>> typeTable_ { get; set; } = null;

        //--------------<Constructor>------------------
        public TypeTable()
        {
            typeTable_ = new Dictionary<string, List<TypeInfo>>();
        }
        public int size()
        {
            return typeTable_.Count;
        }
        

        //---------------<Add type nad its' information to type table>------------
        public bool Add(string type,TypeInfo info)
        {
            if(typeTable_.ContainsKey(type))
            {
                typeTable_[type].Add(info);
                return true;
            }
            List<TypeInfo> temp = new List<TypeInfo>();
            temp.Add(info);
            typeTable_.Add(type, temp);
            return true;

        }

        //-----------<Access type information in the table>-------------
        public List<TypeInfo> this[string type]
        {
            get
            {
                return typeTable_[type];
            }
            set
            {
                typeTable_[type] = value;
            }
        }

        //---------------<Does type table contains certain type?>-----------
        public bool Contains(string type)
        {
            return typeTable_.ContainsKey(type);
        }

        //--------------<Print the type table, for demonstration use>-----------
        public void print()
        {
            foreach( KeyValuePair<string, List<TypeInfo>> kvp in typeTable_)
            {
                Console.WriteLine("Type name:{0}: ", kvp.Key);
                foreach(TypeInfo tf in kvp.Value)
                {
                    tf.print();
                }
                Console.WriteLine("================================================");

            }
        }
#if TEST_TYPETABLE
        static void Main()
        {
            TypeInfo tf1 = new TypeInfo();
            tf1.file = "dummy1.cs";
            tf1.nameSpace = "dummy1";
            TypeInfo tf2 = new TypeInfo();
            tf2.file = "dummy2.cs";
            tf2.nameSpace = "dummy2";
            TypeInfo tf3 = new TypeInfo();
            tf3.file = "dummy3.cs";
            tf3.nameSpace = "dummy3";
            TypeInfo tf4 = new TypeInfo();
            tf4.file = "dummy4.cs";
            tf4.nameSpace = "dummy4";
            TypeTable tb = new TypeTable();
            tb.Add("DummyClass1", tf1);
            tb.Add("DummyClass1", tf2);
            tb.Add("DummyClass2", tf3);
            tb.Add("DummyClass2", tf4);
            Console.WriteLine("Demonstrate TypeTable\n==================================\n");
            tb.print();
            Console.ReadLine();
        }

#endif

    }

}


