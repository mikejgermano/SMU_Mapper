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

        public static Dictionary<string, string[]> refTable = new Dictionary<string, string[]>{
        {"owning_user",         new string[2]{"User","user_id"}},
        {"last_mod_user",       new string[2]{"User","user_id"}},
        {"owning_group",        new string[2]{"Group","name"}},
        {"uom_tag",             new string[2]{"UnitOfMeasure","symbol"}},
        {"volume_tag",          new string[2]{"ImanVolume","volume_name"}},
        //{"release_status_list", new string[2]{"ReleaseStatus","name"}},
        {"dataset_type",        new string[2]{"DatasetType","datasettype_name"}},
        {"relation_type",       new string[2]{"ImanType","type_name"}},
        {"tool_used",           new string[2]{"Tool","object_name"}}
        };

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
            query = "{0} = from {3} in _xml.Elements(_ns + \"{1}\") {4} {2} select {3};";
            //Joins
            string joins = "";
            if(map.srcjoin != null)
            joins = ConvertJoin(map.a, map.srcjoin);

            //Where
            string where = "";
            if (map.srccheck != null)
            {
                string ifs = map.srccheck.@if;

                //build Where
                where = ConvertIf(map.a, map.srccheck);

            }


            string queryBuild = String.Format(query, queryName, map.a, where,map.a,joins);

            MapCode.AppendLine(queryBuild);

            //Start the itteration
            MapCode.AppendFormat("foreach(var {0} in {1}){{", map.b, Extensions.queryName);

            //Map-Class Name change

            if (map.a != map.b)
            {
                MapCode.AppendFormat(" {1}.Name = _ns + \"{1}\";{1}.SetAttributeValue(\"object_type\",\"{2}\");", Extensions.queryName, map.b,map.b);
                /*query = "{0} = from {1} in _xml.Elements(_ns + \"{1}\") {2} select {1};";
                queryBuild = String.Format(query, queryName, map.b, "");
                MapCode.AppendLine(queryBuild);*/
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
                                string q = ConvertMapAttr((MapAttr)task,map.b);
                                Tasks.AppendLine(q);
                                break;
                            }
                        case "MapAttribute":
                            {
                                string q = ConvertMapAttribute((MapAttribute)task,map.b);
                                Tasks.AppendLine(q);

                                break;
                            }
                        case "MapAttrCheck":
                            {
                                string q = ConvertMapAttrCheck((MapAttrCheck)task,map.b);
                                Tasks.AppendLine(q);

                                break;
                            }
                    }
                }


            }

            MapCode.AppendLine(Tasks.ToString());
            MapCode.AppendLine("}");
            return MapCode;
        }

        private static string ConvertMapAttrCheck(MapAttrCheck mCheck,string alias)
        {
            string value = mCheck.@if;
            value = ConvertIf(alias, mCheck);


            //Iteration
            StringBuilder Tasks = new StringBuilder();


            //Tasks.AppendFormat("{0} = {1}.Where(el=>{2});", AttrChkQueryName, queryName, value);
            Tasks.AppendFormat("if({0}){{", value);
            foreach (var task in mCheck.Items)
            {
                switch (task.GetType().Name)
                {
                    case "AttrCheckAttr":
                        {
                            string q = ConvertMapAttr((AttrCheckAttr)task,alias);
                            Tasks.AppendLine(q);
                            break;
                        }
                    case "AttrCheckAttribute":
                        {
                            string q = ConvertMapAttribute((AttrCheckAttribute)task,alias);
                            Tasks.AppendLine(q);

                            break;
                        }
                }
            }

            Tasks.Append("}");


            return Tasks.ToString();
        }

        private static string ConvertMapAttribute(MapAttribute mAttribute,string alias)
        {
            string value = mAttribute.value;

            

            value = _ConvertAttributeString(alias, value);
            value = _ConvertVariableString("", value);

            //Check if reference attribute
            if (refTable.ContainsKey(mAttribute.name) || mAttribute.name == "release_status_list")
            {

                if(mAttribute.name == "release_status_list")
                    value = String.Format(@"_SetRelSts({0}.Attribute(""{1}""),{2})",alias, mAttribute.name,value);
                else
                    value = String.Format(@"_GetRef(""{0}"",{1})", mAttribute.name, value);

            }

            string sb = "{0}.SetAttributeValue(\"{1}\",{2});";

            string queryBuild = String.Format(sb, alias, mAttribute.name, value.Replace("'", "\""));

            return queryBuild;
        }

        private static string ConvertMapAttribute(AttrCheckAttribute mAttribute,string alias)
        {
            string value = mAttribute.value;

            value = _ConvertAttributeString(alias, value);
            value = _ConvertVariableString("", value);

            //Check if reference attribute
            if (refTable.ContainsKey(mAttribute.name) || mAttribute.name == "release_status_list")
            {

                if (mAttribute.name == "release_status_list")
                    value = String.Format(@"_SetRelSts({0}.Attribute(""{1}""),{2})", alias, mAttribute.name, value);
                else
                    value = String.Format(@"_GetRef(""{0}"",{1})", mAttribute.name, value);

            }

            string sb = "{0}.SetAttributeValue(\"{1}\",{2});";

            string queryBuild = String.Format(sb, alias, mAttribute.name, value.Replace("'", "\""));

            return queryBuild;
        }

        private static string ConvertMapAttr(MapAttr mAttr,string alias)
        {

            if (mAttr.copy == null)
            {
                string sb = "{0}.SetAttributeValue(\"{1}\",{0}.Attribute(\"{2}\").Value);{0}.Attribute(\"{2}\").Remove();";

                string queryBuild = String.Format(sb, alias, mAttr.b, mAttr.a);

                return queryBuild;
            }
            else
            {
                string sb = "{0}.SetAttributeValue(\"{1}\",{0}.Attribute(\"{2}\").Value);";

                string queryBuild = String.Format(sb, alias, mAttr.b, mAttr.a);

                return queryBuild;
            }


        }

        private static string ConvertMapAttr(AttrCheckAttr mAttr,string alias)
        {
            if (mAttr.copy == null)
            {
                string sb = "{0}.SetAttributeValue(\"{1}\",{0}.Attribute(\"{2}\").Value);{0}.Attribute(\"{2}\").Remove();";

                string queryBuild = String.Format(sb, alias, mAttr.b, mAttr.a);

                return queryBuild;
            }
            else
            {
                string sb = "{0}.SetAttributeValue(\"{1}\",{0}.Attribute(\"{2}\").Value);";

                string queryBuild = String.Format(sb, alias, mAttr.b, mAttr.a);

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

        private static string ConvertJoin(string alias, SrcJoin[] mval)
        {
            StringBuilder sb = new StringBuilder();

            foreach(SrcJoin srcjoin in mval)
            {

                string obj = srcjoin.obj;
                string on = srcjoin.on;

                string join = "";

                //patter from XSD
                //join dataset in _xmlfile.Elements() on 

                var groups = Regex.Match(on, @"^([a-zA-z0-9_]+)@([a-zA-z0-9_]+)(?:\s)*?=(?:\s)*?([a-zA-z0-9_]+)@([a-zA-z0-9_]+)$").Groups;

                string leftObj   = groups[1].Value;
                string leftAttr  = groups[2].Value;
                string rightObj  = groups[3].Value;
                string rightAttr = groups[4].Value;

                join = string.Format("join {0} in {1}.Elements(_ns + \"{0}\") on {2}.Attribute(\"{3}\").Value equals {4}.Attribute(\"{5}\").Value",
                    rightObj,xmlFile, leftObj,leftAttr,rightObj,rightAttr);

                sb.AppendLine(join);
            }


            return sb.ToString();
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
                string result = val;

                string patternRefJoin = String.Format(@"([a-zA-z0-9_]+)@({0})",string.Join("|",refTable.Keys.ToArray()));
                string patternRef = String.Format(@"@({0})", string.Join("|", refTable.Keys.ToArray()));

                //check Release Status first
                result = _RelStsReplace(result, @"([a-zA-z0-9_]+)@(release_status_list)");
                result = _RelStsReplace(result, @"@(release_status_list)");

                string patternJoins = @"([a-zA-z0-9_]+)@([a-zA-z0-9_]+)";
                result = _RefReplace(result, patternRefJoin);
                result = Regex.Replace(result, patternJoins,  "$1.Attribute(\"" + "$2" + "\").Value");

                string pattern = @"@([\w]*)";
                result = _RefReplace(result, patternRef);
                result = Regex.Replace(result, pattern, alias + ".Attribute(\"" + "$1" + "\").Value");

                result = result.Replace(".Attribute(\"release_status_list\").Value", ".Attribute(\"release_status_list\")");

                return result;
            }
            else
                return val;
        }

        private static string _RefReplace(string input,string pattern)
        {
            string gRef = "";

            gRef = Regex.Replace(input, pattern, "_GetRefVal(\"" + "$1" + "\"," + "@" + "$1" + ")");

            return gRef;
        }

        private static string _RelStsReplace(string input, string pattern)
        {
            string gRef = "";

            gRef = Regex.Replace(input, pattern, "_GetRelSts(@" + "$1" + ")");

            return gRef;
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

                //existance check
                if(right.ToUpper() == "NULL")
                {
                    left = left.Replace(".Value", "");
                }

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

        private string ConvertRefTable()
        {
            var refTbl = Extensions.refTable.ToList();
            StringBuilder sb = new StringBuilder("public static Dictionary<string, string[]> refTable = new Dictionary<string, string[]>{\n");

            foreach (var result in refTbl)
            {
                if (refTbl.IndexOf(result) == refTbl.Count - 1)
                {
                    //last item
                    sb.AppendFormat(@"{{""{0}"",       new string[2]{{""{1}"",""{2}""}}}}", result.Key, result.Value[0], result.Value[1]);
                }
                else
                    sb.AppendFormat(@"{{""{0}"",       new string[2]{{""{1}"",""{2}""}}}},", result.Key, result.Value[0], result.Value[1]);

            }

            sb.Append("};");

            return sb.ToString(); ;
        }

        public CompilerResults Compile(string outFile)
        {
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
            var parameters = new CompilerParameters(new[] { "System.dll", "mscorlib.dll", "System.Core.dll", "System.Xml.dll", "System.Xml.Linq.dll" }, outFile, false);
            parameters.GenerateExecutable = true;

            StringBuilder Code = new StringBuilder();

            //Reference Table info
            string refTbl = ConvertRefTable();

            Code.AppendFormat(@"
            using System.Linq;
            using System.Xml.Linq;
            using System.Collections;
            using System.Collections.Generic;
            class Program 
            {{
                
                private static XElement {0} = null;
                private static XNamespace _ns = null;

                public static Dictionary<string, Dictionary<string, string>> _lookups = LoadAllLookups({3});

                public static Dictionary<string,string> _variables = new Dictionary<string,string>{{{5}}};

                {6}

                public static void Main(string[] args) 
                {{
                    {0} = XElement.Load(args[0]);
                    _ns = {0}.GetDefaultNamespace();
                    IEnumerable<XElement> {1} =  Enumerable.Empty<XElement>();
                    IEnumerable<XElement> {4} =  Enumerable.Empty<XElement>();

                   {2}

                    {0}.Save(args[1]);
                    
                }}", Extensions.xmlFile, Extensions.queryName, MapCode, LookupCode, Extensions.AttrChkQueryName,VariableCode, refTbl.ToString());
            

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
                        catch (System.IndexOutOfRangeException)
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

         private static string _GetRefVal(string attribute,string elemid)
         {
            string value = """";

            if (elemid == """") return """";

            elemid = elemid.Replace(""#"", """");

            string RefType = refTable[attribute][0];
            string RefAttr = refTable[attribute][1];

            value = (from el in _xml.Elements(_ns + RefType)
                       where el.Attribute(""elemId"").Value == elemid
                       select el.Attribute(RefAttr).Value).Single();



                return value;
        }


    private static string _GetRelSts(XAttribute attribute)
    {
         if (attribute == null || attribute.Value == """") return """";

        var els = _xml.Elements(_ns + ""ReleaseStatus"").Where(x => x.Attribute(""puid"").Value == attribute.Value).Select(x => x.Attribute(""name"").Value).ToArray();
        string statuses = string.Join("","", els);

            return statuses;

        }

    private static string _SetRelSts(XAttribute attribute, string Val)
    {
        string newPUID = """";

        if (attribute == null || attribute.Value == """")
        { }
        else
        {
            newPUID = attribute.Value;

            var els = _xml.Elements(_ns + ""ReleaseStatus"").Where(x => x.Attribute(""puid"").Value == attribute.Value);

            foreach (var el in els)
            {
                el.SetAttributeValue(""name"", Val);
            }
        }

        return newPUID;

    }

    private static string _GetRef(string attribute, string refVal){
            string elemId = """";

                if(refTable.ContainsKey(attribute))
                {
                    var dval = refTable[attribute];
                    var el = " + Extensions.xmlFile + @".Elements(_ns + dval[0]).Where(x => x.Attribute(dval[1]).Value == refVal).Select(x => x.Attribute(""elemId"").Value);

                    if (el.Count() == 1)
                        elemId = el.Single();
                    else
                        return """";
                }

             return (""#"" + elemId);
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
