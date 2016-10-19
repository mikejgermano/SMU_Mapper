using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace SMU_Mapper.Classes
{



    public static class Extensions
    {


        public static string xmlFile = "_xml";
        public static string queryName = "_query";
        public static string AttrChkQueryName = "_AttrQuery";

        public static StringBuilder Convert(this Script map, int i)
        {
            StringBuilder MapCode = new StringBuilder();

             MapCode.AppendFormat(@"try
             {{
                 {0}
             }}
             catch(System.Exception ex)
             {{
                 throw new System.Exception(""Error in Script :"" + {1} + ""\nError: "" + ex.Message);}}", map.data,i.ToString());

            return MapCode;
        }

        public static StringBuilder Convert(this Map map, int i)
        {
            StringBuilder MapCode = new StringBuilder();

            string query = "";
            //Query
            query = "{0} = from el in _xml.Elements(_ns + \"{1}\") {2} select el;";
            //Joins

            //Where
            string where = "";
            if (map.srccheck != null)
            {
                string ifs = map.srccheck.@if;

                //build Where
                where = ConvertIf("el", map.srccheck);

            }


            string queryBuild = String.Format(query, queryName, map.a, where);

            MapCode.AppendLine(queryBuild);

            //Map-Class Name change

            if (map.a != map.b)
            {
                MapCode.AppendFormat("foreach(var el in {0}){{ el.Name = _ns + \"{1}\";el.SetAttributeValue(\"object_type\",\"{2}\");}}", Extensions.queryName, map.b,map.b);
                query = "{0} = from el in _xml.Elements(_ns + \"{1}\") {2} select el;";
                queryBuild = String.Format(query, queryName, map.b, where);
                MapCode.AppendLine(queryBuild);
            }


            //Iteration
            StringBuilder Tasks = new StringBuilder();

            if (map.Items != null)
            {
                foreach (var task in map.Items)
                {
                    switch (task.GetType().Name)
                    {
                        case "MapAttr":
                            {
                                string q = ConvertMapAttr((MapAttr)task);
                                Tasks.AppendLine(q);
                                break;
                            }
                        case "MapAttribute":
                            {
                                string q = ConvertMapAttribute((MapAttribute)task);
                                Tasks.AppendLine(q);

                                break;
                            }
                        case "MapAttrCheck":
                            {
                                string q = ConvertMapAttrCheck((MapAttrCheck)task);
                                Tasks.AppendLine(q);

                                break;
                            }
                    }
                }


            }

            MapCode.AppendLine(Tasks.ToString());

            return MapCode;
        }

        private static string ConvertMapAttrCheck(MapAttrCheck mCheck)
        {
            string value = mCheck.@if;
            value = ConvertIf("el", mCheck);


            //Iteration
            StringBuilder Tasks = new StringBuilder();


            Tasks.AppendFormat("{0} = {1}.Where(el=>{2});", AttrChkQueryName, queryName, value);
            Tasks.AppendFormat("foreach(var el in {0}){{", Extensions.AttrChkQueryName);
            foreach (var task in mCheck.Items)
            {
                switch (task.GetType().Name)
                {
                    case "AttrCheckAttr":
                        {
                            string q = ConvertMapAttr((AttrCheckAttr)task);
                            Tasks.AppendLine(q);
                            break;
                        }
                    case "AttrCheckAttribute":
                        {
                            string q = ConvertMapAttribute((AttrCheckAttribute)task);
                            Tasks.AppendLine(q);

                            break;
                        }
                }
            }

            Tasks.Append("}");


            return Tasks.ToString();
        }

        private static string ConvertMapAttribute(MapAttribute mAttribute)
        {
            string value = mAttribute.value;

            value = _ConvertAttributeString("attr.Element", value);
            value = _ConvertVariableString("", value);

            string sb = "foreach(var attr in  {0}.Where(attr => attr.Attribute(\"{1}\") != null).Select(x=> new {{xAttr = x.Attribute(\"{2}\"),Element = x }})){{attr.xAttr.SetValue({3});}}";

            string queryBuild = String.Format(sb, queryName, mAttribute.name, mAttribute.name, value.Replace("'", "\""));

            return queryBuild;
        }

        private static string ConvertMapAttribute(AttrCheckAttribute mAttribute)
        {
            string value = mAttribute.value;

            value = _ConvertAttributeString("el", value);

            string sb = "if(el.Attribute(\"{0}\") != null){{var attr = el.Attribute(\"{0}\");attr.SetValue({1});}}";

            string queryBuild = String.Format(sb, mAttribute.name, value.Replace("'", "\""));

            return queryBuild;
        }

        private static string ConvertMapAttr(MapAttr mAttr)
        {

            if (mAttr.copy == null)
            {
                string sb = "foreach(var attr in  {0}.Where(attr => attr.Attribute(\"{1}\") != null).Select(x=> new {{xAttr = x.Attribute(\"{1}\"),Element = x }})){{attr.Element.SetAttributeValue(\"{2}\",attr.xAttr.Value);attr.xAttr.Remove();}}";

                string queryBuild = String.Format(sb, queryName, mAttr.a, mAttr.b);

                return queryBuild;
            }
            else
            {
                string sb = "foreach(var attr in  {0}.Where(attr => attr.Attribute(\"{1}\") != null).Select(x=> new {{xAttr = x.Attribute(\"{1}\"),Element = x }})){{attr.Element.SetAttributeValue(\"{2}\",attr.xAttr.Value);}}";

                string queryBuild = String.Format(sb, queryName, mAttr.a, mAttr.b);

                return queryBuild;
            }


        }

        private static string ConvertMapAttr(AttrCheckAttr mAttr)
        {

            if (mAttr.copy == null)
            {
                string sb = "if(el.Attribute(\"{0}\") != null){{var attr = el.Attribute(\"{0}\");el.SetAttributeValue(\"{1}\",attr.Value);attr.Remove();}}";

                string queryBuild = String.Format(sb, mAttr.a, mAttr.b);

                return queryBuild;
            }
            else
            {
                string sb = "if(el.Attribute(\"{0}\") != null){{var attr = el.Attribute(\"{0}\");el.SetAttributeValue(\"{1}\",attr.Value);}}";

                string queryBuild = String.Format(sb, mAttr.a, mAttr.b);

                return queryBuild;
            }


        }

        private static string ConvertIf(string alias, SrcCheck mval)
        {
            string val = mval.@if;

            if (val == "") return "";

            val = _CleanCodeString(alias, val);

            return val.Insert(0, "where ");
        }

        private static string ConvertIf(string alias, MapAttrCheck mval)
        {
            string val = mval.@if;

            if (val == "") return "";

            val = _CleanCodeString(alias, val);

            return val;
        }

        public static List<string> AltMerge(List<string> a, List<string> b)
        {
            int c1 = 0, c2 = 0;
            List<string> res = new List<string>();

            while (c1 < a.Count() || c2 < b.Count())
            {
                if (c1 < a.Count())
                    res.Add((string)a[c1++]);
                if (c2 < b.Count())
                    res.Add((string)b[c2++]);
            }
            return res;
        }

        private static string _ConvertAttributeString(string alias, string val)
        {
            if (val.Contains("@"))
            {
                string pattern = @"@([\w]*)";
                string result = Regex.Replace(val, pattern, alias + ".Attribute(\"" + "$1" + "\").Value");

                return result;
            }
            else
                return val;
        }

        private static string _ConvertVariableString(string alias, string val)
        {
            if (val.Contains("$"))
            {
                string pattern = @"\$([\w]*)";
                string result = Regex.Replace(val, pattern, alias + "VarLookup(\"" + "$1" + "\")");

                return result;
            }
            else
                return val;
        }

        private static string _CleanCodeString(string alias, string val)
        {
            if (val == "") return "";

            Dictionary<string, string> OperandsD = new Dictionary<string, string>()
            {
                {"AND", "&&"},
                {"OR", "||"}
            };

            val = _ConvertAttributeString(alias, val);
            val = val.Replace("'", "\"");

            var conditions = val.Split(new string[] { " and ", " AND ", " or ", " OR" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            MatchCollection matches = Regex.Matches(val.ToUpper(), @"\b[\w']*\b");

            List<string> OperandsC = new List<string>();

            foreach (var matchE in matches)
            {
                string match = matchE.ToString();
                if (OperandsD.ContainsKey(match))
                    OperandsC.Add(OperandsD[match]);
            }


            for (int i = 0; i < conditions.Count(); i++)
            {
                string opt = "";
                string right = "";
                string left = conditions[i].Split(new string[] { "=", "!=" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                if (conditions[i].Contains("!=")) opt = "!="; else if (conditions[i].Contains("=")) opt = "="; else opt = "";

                if (opt != "")
                    right = conditions[i].Split(new string[] { "=", "!=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                if (opt != "!=")
                    opt = opt.Replace("=", "==");

                right = right.Replace("'", "\"");

                conditions[i] = left + opt + right;
            }

            val = String.Join(" ", AltMerge(conditions.ToList(), OperandsC));

            return val;
        }


    }



    class MapConverter
    {

        private StringBuilder MapCode = new StringBuilder();
        private string LookupCode;
        private string VariableCode;

        public void ConvertMaps(Maps maps)
        {

            LookupCode = ConvertLookups(maps);
            VariableCode = ConvertVariables(maps.header);


            int i = 1;
            foreach (object m in maps.Items)
            {
                if (m.GetType().Name == "Map")
                {
                    MapCode.AppendLine("\n\n// Map :" + i.ToString());

                    MapCode.AppendLine(((Map)m).Convert(i).ToString());
                    i++;
                }
                else//Scripts
                {

                    MapCode.AppendLine("\n\n// Script :" + i.ToString());
                    MapCode.AppendLine(((Script)m).Convert(i).ToString());
                    MapCode.Append(";");
                    i++;
                }

            }


        }

        private string ConvertLookups(Maps mMap)
        {

            var arLookup = mMap.header.lookup.Select(x => "new string[2] {\"" + x.name + "\",@\"" + x.file + "\"}");
            string list = string.Join(",", arLookup);

            return list;

        }

        private string ConvertVariables(Header h)
        {
            var arVar = h.variable.Select(x => "{\"" + x.name + "\",@\"" + x.value + "\"}");
            string list = string.Join(",", arVar);

            return list;
            
        }

        public CompilerResults Compile(string outFile)
        {
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
            var parameters = new CompilerParameters(new[] { "System.dll", "mscorlib.dll", "System.Core.dll", "System.Xml.dll", "System.Xml.Linq.dll" }, outFile, false);
            parameters.GenerateExecutable = true;

            StringBuilder Code = new StringBuilder();

            Code.AppendFormat(@"
            using System.Linq;
            using System.Xml.Linq;
            using System.Collections;
            using System.Collections.Generic;
            class Program 
            {{
                public static Dictionary<string, Dictionary<string, string>> _lookups = LoadAllLookups({3});

                public static Dictionary<string,string> _variables = new Dictionary<string,string>{{{5}}};

                public static void Main(string[] args) 
                {{
                    XElement {0} = XElement.Load(args[0]);
                    XNamespace _ns = {0}.GetDefaultNamespace();
                    IEnumerable<XElement> {1} =  Enumerable.Empty<XElement>();
                    IEnumerable<XElement> {4} =  Enumerable.Empty<XElement>();

                   {2}

                    {0}.Save(args[1]);
                    
                }}", Extensions.xmlFile, Extensions.queryName, MapCode, LookupCode, Extensions.AttrChkQueryName,VariableCode);
            

            Code.Append(@" 
                private static Dictionary<string, string> LoadLookup(string path)
                {
                        Dictionary<string, string> LoadLookup_d = new Dictionary<string, string>();
                        try
                        {
                            LoadLookup_d = System.IO.File.ReadAllLines(path).Select(x => x.Split('|')).ToDictionary(x => x[0], x => x[1]);
      
                            if(LoadLookup_d.Count() == 0)
                            System.Console.WriteLine(""Warning - "" + System.IO.Path.GetFileName(path) + "" is empty"");
                        }
                        catch(System.IO.FileNotFoundException nf)
                        {
                            System.Console.WriteLine(""Error - loading lookups, couldn't find the lookup file.\nCheck that your path is correct:\n"" + nf.FileName);
                            System.Environment.Exit(1);
                        }
                        catch (System.IndexOutOfRangeException e)
                        {
                            System.Console.WriteLine(""Error - loading lookup in "" + System.IO.Path.GetFileName(path) +  ""\n...most likely key/value pair is malformed"");
                            System.Environment.Exit(1);
                        }

                        return LoadLookup_d;
                }

                private static Dictionary<string, Dictionary<string, string>> LoadAllLookups(params string[][] lookups)
                {
                    Dictionary<string, Dictionary<string, string>> d = new Dictionary<string, Dictionary<string, string>>();

                    foreach (string[] lookup in lookups)
                    {
                        d.Add(lookup[0], LoadLookup(lookup[1]));
                    }

                    return d;
                }

                private static string Lookup(string name, string key)
                {

                    Dictionary<string,string> d1;
                    string value = null;

                    if (_lookups.TryGetValue(name, out d1)) 
                    {
                        d1.TryGetValue(key, out value);
                    }

                    return value ?? key;

                }

             private static string VarLookup(string name)
             {
                    string value = null;

                    if (!_variables.TryGetValue(name, out value))
                    {
                        //variable not found
                    }
                    return value;

            }
            }");

            CompilerResults results = csc.CompileAssemblyFromSource(parameters,
            Code.ToString());

            System.IO.File.WriteAllText("mapper.cs", Code.ToString());

            results.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(
                error.Line + ": " + error.ErrorText
                ));


            return results;
        }

        
    }
}
