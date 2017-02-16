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
        public static HeaderModel model;

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


        public static StringBuilder Convert(this Map map, int i)
        {
            StringBuilder MapCode = new StringBuilder();

            string[] _DontChangeName = new string[] { "Dataset", "Form" };

            //Change other TC Class types
            string tcTypeS = model.getTcType(map.a);
            ModelClass mClassS = null;

            if (tcTypeS != "")
            {
                mClassS = model.getTcModel(map.a);
            }

            string tcTypeT = model.getTcType(map.b);
            ModelClass mClassT = null;
           
            if(tcTypeT != "")
            {
                mClassT = model.getTcModel(map.b);
            }

            string query = "";
            //Query
            query = "{0} = from {3} in _xml.Elements(_ns + \"{1}\") {4} {2} select {3};";
            //Joins
            string joins = "";
            if (map.srcjoin != null)
                joins = ConvertJoin(map.a, map.srcjoin);

            //Where
            string where = "";
            if (map.srccheck != null)
            {
                string ifs = map.srccheck.@if;

                //build Where
                where = ConvertIf(map.a, map.srccheck);

            }


            string queryBuild = String.Format(query, queryName, map.a, where, map.a, joins);

            MapCode.AppendLine(queryBuild);

            //Start the itteration
            MapCode.AppendFormat("foreach(var {0} in {1}){{", map.b, Extensions.queryName);

            //Map-Class Name change

            if (map.a != map.b)
            {
                if (_DontChangeName.Contains(map.a))
                    MapCode.AppendFormat(" {1}.SetAttributeValue(\"object_type\",\"{2}\");", Extensions.queryName, map.b, map.b);
                else
                {
                    //MapCode.AppendFormat(" {1}.Name = _ns + \"{1}\";{1}.SetAttributeValue(\"object_type\",\"{2}\");", Extensions.queryName, map.b, map.b);
                    switch (tcTypeT + tcTypeS)
                    {
                        case "itemitem":
                            MapCode.AppendFormat(" _TCPropagateItem({0},\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\");", map.b, mClassS.item, mClassS.itemrevision, mClassS.masterform, mClassS.masterformS, mClassS.masterformRev, mClassS.masterformRevS, mClassT.item,mClassT.itemrevision,mClassT.masterform,mClassT.masterformS,mClassT.masterformRev,mClassT.masterformRevS);
                            break;
                        case "itemrevitemrev":
                            MapCode.AppendFormat(" _TCPropagateItemRevision({0},\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\");", map.b, mClassS.item, mClassS.itemrevision, mClassS.masterform, mClassS.masterformS, mClassS.masterformRev, mClassS.masterformRevS, mClassT.item, mClassT.itemrevision, mClassT.masterform, mClassT.masterformS, mClassT.masterformRev, mClassT.masterformRevS);
                            break;
                        default:
                            MapCode.AppendFormat(" {1}.Name = _ns + \"{1}\";{1}.SetAttributeValue(\"object_type\",\"{2}\");", Extensions.queryName, map.b, map.b);
                            break;
                    }

                    MapCode.AppendFormat(" _RecordTypeChange({0}.Attribute(\"puid\").Value,\"{1}\",\"{2}\");", map.b, map.a, map.b);
                }

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
                                string q = ConvertMapAttr((MapAttr)task, map.b);
                                Tasks.AppendLine(q);
                                break;
                            }
                        case "MapAttribute":
                            {
                                string q = ConvertMapAttribute((MapAttribute)task, map.b);
                                Tasks.AppendLine(q);

                                break;
                            }
                        case "MapAttrCheck":
                            {
                                string q = ConvertMapAttrCheck((MapAttrCheck)task, map.b);
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

        public static StringBuilder Convert(this Script map, int i)
        {
            StringBuilder MapCode = new StringBuilder();

            MapCode.AppendFormat(@"try
             {{
                 {0}
             }}
             catch(System.Exception ex)
             {{
                 throw new System.Exception(""Error in Script :"" + {1} + ""\nError: "" + ex.Message);}}", map.data, i.ToString());

            return MapCode;
        }

        public static ModelClass getTcModel(this HeaderModel model, string obj)
        {
            ModelClass m;

            foreach (var c in model.classes)
            {
                if (c.item == obj || c.itemrevision == obj)
                    return c;
 
            }

            return null;
        }

        public static string getTcType(this HeaderModel model, string obj)
        {
            foreach (var c in model.classes)
            {
                if (c.item == obj)
                    return "item";
                else if (c.itemrevision == obj)
                    return "itemrev";
            }

            return "";
        }

        public static string[] getRecordedMaps(Maps m)
        {
            var maps = m.Items.Where(x => x.GetType() == typeof(Map)).Cast<Map>().Where(x => x.srccheck != null).Select(x => x.b).ToArray();

            return maps;
        }

        private static string ConvertMapAttrCheck(MapAttrCheck mCheck, string alias)
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
                            string q = ConvertMapAttr((AttrCheckAttr)task, alias);
                            Tasks.AppendLine(q);
                            break;
                        }
                    case "AttrCheckAttribute":
                        {
                            string q = ConvertMapAttribute((AttrCheckAttribute)task, alias);
                            Tasks.AppendLine(q);

                            break;
                        }
                }
            }

            Tasks.Append("}");


            return Tasks.ToString();
        }

        private static string ConvertMapAttribute(MapAttribute mAttribute, string alias)
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

        private static string ConvertMapAttribute(AttrCheckAttribute mAttribute, string alias)
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

        private static string ConvertMapAttr(MapAttr mAttr, string alias)
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

        private static string ConvertMapAttr(AttrCheckAttr mAttr, string alias)
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

            foreach (SrcJoin srcjoin in mval)
            {

                string obj = srcjoin.obj;
                string on = srcjoin.on;

                string join = "";

                //patter from XSD
                //join dataset in _xmlfile.Elements() on 

                var groups = Regex.Match(on, @"^([a-zA-z0-9_]+)@([a-zA-z0-9_]+)(?:\s)*?=(?:\s)*?([a-zA-z0-9_]+)@([a-zA-z0-9_]+)$").Groups;

                string leftObj = groups[1].Value;
                string leftAttr = groups[2].Value;
                string rightObj = groups[3].Value;
                string rightAttr = groups[4].Value;

                join = string.Format("join {0} in {1}.Elements(_ns + \"{0}\") on {2}.Attribute(\"{3}\").Value equals {4}.Attribute(\"{5}\").Value",
                    rightObj, xmlFile, leftObj, leftAttr, rightObj, rightAttr);

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

                string patternRefJoin = String.Format(@"([a-zA-z0-9_]+)@({0})", string.Join("|", refTable.Keys.ToArray()));
                string patternRef = String.Format(@"@({0})", string.Join("|", refTable.Keys.ToArray()));

                //check Release Status first
                result = _RelStsReplace(result, @"([a-zA-z0-9_]+)@(release_status_list)");
                result = _RelStsReplace(result, @"@(release_status_list)");

                string patternJoins = @"([a-zA-z0-9_]+)@([a-zA-z0-9_]+)";
                result = _RefReplace(result, patternRefJoin);
                result = Regex.Replace(result, patternJoins, "$1.Attribute(\"" + "$2" + "\").Value");

                string pattern = @"@([\w]*)";
                result = _RefReplace(result, patternRef);
                result = Regex.Replace(result, pattern, alias + ".Attribute(\"" + "$1" + "\").Value");

                result = result.Replace(".Attribute(\"release_status_list\").Value", ".Attribute(\"release_status_list\")");

                return result;
            }
            else
                return val;
        }

        private static string _RefReplace(string input, string pattern)
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
                if (right.ToUpper() == "NULL")
                {
                    left = left.Replace(".Value", "");
                }

                conditions[i] = left + opt + right;
            }

            val = String.Join(" ", AltMerge(conditions.ToList(), OperandsC));

            return val;
        }


    }


    /// <summary>
    /// Convert the mapping code into C# code for compilation
    /// </summary>
    class MapConverter
    {

        private StringBuilder MapCode = new StringBuilder();
        private string FunctionCode;
        private string LookupCode;
        private string VariableCode;
        private string[] MapChanges;
        private HeaderModel model;

        public void ConvertMaps(Maps maps)
        {
            model = maps.header.model;
            Extensions.model = model;
            MapChanges = Extensions.getRecordedMaps(maps);
            FunctionCode = ConvertFunctions(maps.header);
            LookupCode = ConvertLookups(maps);
            VariableCode = ConvertVariables(maps.header);


            //Target Mappings that don't match the model list
            List<string> l = new List<string>();

            foreach (var m in maps.Items.Where(x => x.GetType() == typeof(Map)).Cast<Map>())
            {
                if (model.getTcType(m.b) == "")
                    l.Add(m.b);
            }
            if (l.Count() > 0) { Console.WriteLine("--Target Mappings that don't match model--"); l.ToList().ForEach(Console.WriteLine); }
            //End

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
            if (mMap.header.lookup == null)
                return "";

            var arLookup = mMap.header.lookup.Select(x => "new string[2] {\"" + x.name + "\",@\"" + x.file + "\"}");
            string list = string.Join(",", arLookup);

            return list;

        }

        private string ConvertFunctions(Header h)
        {
            if (h.function == null)
                return "";

            var arVar = String.Join(Environment.NewLine, h.function.Select(x => x.data));


            return arVar;

        }

        private string ConvertVariables(Header h)
        {

            if (h.variable == null)
                return "";

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
            string CreateAccesDatabase = Properties.Resources.CreateAccesDatabase;
            string ClassCode = Properties.Resources.Classes;
            //string functions; 

            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" }, { "Platform", "x64" } });
            var parameters = new CompilerParameters(new[] { "System.dll", "mscorlib.dll", "System.Core.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Data.dll" }, outFile, false);
            parameters.ReferencedAssemblies.Add(@"System.Data.SqlServerCe.dll");


            parameters.GenerateExecutable = true;
            parameters.IncludeDebugInformation = true;
            parameters.CompilerOptions = "/platform:x64";

            StringBuilder Code = new StringBuilder();

            //Reference Table info
            string refTbl = ConvertRefTable();

            Code.AppendFormat(@"
            using System.Linq;
            using System.Xml.Linq;
            using System.Collections;
            using System.Collections.Generic;
            using System.Data.SqlServerCe;
           
            
            {7}

            class Program 
            {{
                                

                private static XElement {0} = null;
                private static XNamespace _ns = null;

                public static Dictionary<string, Dictionary<string, string>> _lookups = LoadAllLookups({3});
                public static Dictionary<string,string> _variables = new Dictionary<string,string>{{{5}}};
                public static Dictionary<string,string[]> _TypeChangeLog = new Dictionary<string,string[]>();

                public static string _IMAN_master_form = null;

                {6}

                public static void Main(string[] args) 
                {{
                    
                    //bool db_result = CreateNewAccessDatabase();
                    
                    {0} = XElement.Load(args[0]);
                    _ns = {0}.GetDefaultNamespace();
                    IEnumerable<XElement> {1} =  Enumerable.Empty<XElement>();
                    IEnumerable<XElement> {4} =  Enumerable.Empty<XElement>();

                    string _IMAN_master_form = _GetRef(""relation_type"", ""IMAN_master_form"");

                    Classes._xml = _xml;
                    Classes._ns = _ns;
                    Classes._IMAN_master_form = _IMAN_master_form;

                   {2}

                    {0}.Save(args[1]);
                    
                }}", Extensions.xmlFile, Extensions.queryName, MapCode, LookupCode, Extensions.AttrChkQueryName, VariableCode, refTbl.ToString(), ClassCode);


            string CompileCode = Properties.Resources.CompileCode;
            Code.Append(CompileCode);


            Code.AppendLine(FunctionCode);

            Code.AppendLine(CreateAccesDatabase);

            Code.Append("}");

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
