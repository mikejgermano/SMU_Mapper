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
        public static string wildcard = "*";
        public static string wildcard_replace = "_wildcard";

        public static HeaderModel model;


        public enum refObjects
        { DatasetType, Group, ImanType, ImanVolume, ReleaseStatus, Tool, UnitOfMeasure, User };

        public static Dictionary<refObjects, string> refObjectTable = new Dictionary<refObjects, string>
        {
            { refObjects.DatasetType,"datasettype_name" },
            { refObjects.Group,"full_name" },
            { refObjects.ImanType,"type_name" },
            { refObjects.ImanVolume,"volume_name" },
            { refObjects.ReleaseStatus,"name" },
            { refObjects.Tool,"object_name" },
            { refObjects.UnitOfMeasure,"symbol" },
            { refObjects.User,"user_id" }
        };



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
            bool wildcard_used = false;

            string[] _DontChangeName = new string[] { "Dataset", "Form" };

            //check for wildcard
            if (map.a == wildcard || map.b == wildcard)
            {
                wildcard_used = true;
                map.a = wildcard_replace;
                map.b = wildcard_replace;
            }

            //Change other TC Class types
            string tcTypeS = model.getTcType(map.a);
            ModelClass mClassS = null;

            if (tcTypeS != "")
            {
                mClassS = model.getTcModel(map.a);
            }

            string tcTypeT = model.getTcType(map.b);
            ModelClass mClassT = null;

            if (tcTypeT != "")
            {
                mClassT = model.getTcModel(map.b);
            }



            //if ((map.mapclass == "yes" && map.srccheck != null) || (mClassS == null || mClassT == null || map.mapclass == "no"))
            //{
            string query = "";
            //Query
            if (wildcard_used)
                query = "{0} = from {3} in _xml.Elements() {4} {2} select {3};";
            else
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

            //Errorchecking Empty Query
            MapCode.AppendFormat("if (!_query.Any()) {{ Global._errList.Add(new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.MAP_QUERY_EMPTY, \"\", \"\", TCTypes.Mapping, \"{0}\")); }}", map.a);

            //Start the itteration
            MapCode.AppendFormat("foreach(var {0} in {1}){{", map.b, Extensions.queryName);

            //Map-Class Name change

            if (map.a != map.b && wildcard_used == false)
            {
                if (_DontChangeName.Contains(map.a))
                    MapCode.AppendFormat(" {1}.SetAttrValue(\"object_type\",\"{2}\");", Extensions.queryName, map.b, map.b);
                else
                {
                    //MapCode.AppendFormat(" {1}.Name = _ns + \"{1}\";{1}.SetAttrValue(\"object_type\",\"{2}\");", Extensions.queryName, map.b, map.b);
                    switch (tcTypeT + tcTypeS + "|" + map.mapclass)
                    {
                        case "itemitem|yes":
                            MapCode.AppendFormat(" _TCPropagateItem({0},\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\");", map.b, mClassS.item, mClassS.itemrevision, mClassS.masterform, mClassS.masterformS, mClassS.masterformRev, mClassS.masterformRevS, mClassT.item, mClassT.itemrevision, mClassT.masterform, mClassT.masterformS, mClassT.masterformRev, mClassT.masterformRevS);
                            break;
                        case "itemrevitemrev|yes":
                            MapCode.AppendFormat(" if({13}.Name.LocalName != \"{13}\")_TCPropagateItemRevision({0},\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\");", map.b, mClassS.item, mClassS.itemrevision, mClassS.masterform, mClassS.masterformS, mClassS.masterformRev, mClassS.masterformRevS, mClassT.item, mClassT.itemrevision, mClassT.masterform, mClassT.masterformS, mClassT.masterformRev, mClassT.masterformRevS, map.b);
                            break;
                        default:
                            MapCode.AppendFormat(" {1}.Name = _ns + \"{1}\";{1}.SetAttrValue(\"object_type\",\"{2}\");", Extensions.queryName, map.b, map.b);
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
            //}

            /*Fast Propagate
            if (map.a != map.b && map.srccheck == null && map.mapclass=="yes")
            {
                if (mClassS != null && mClassT != null)
                {
                    MapCode.AppendFormat(" _FastTCPropagateItem(\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\");", mClassS.item, mClassS.itemrevision, mClassS.masterform, mClassS.masterformS, mClassS.masterformRev, mClassS.masterformRevS, mClassT.item, mClassT.itemrevision, mClassT.masterform, mClassT.masterformS, mClassT.masterformRev, mClassT.masterformRevS);
                }
            }*/

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

        public static StringBuilder Convert(this HeaderMapRefObject mapRef)
        {
            StringBuilder MapCode = new StringBuilder();

            refObjects type = mapRef.type;
            string attributeRef = refObjectTable[type];
            var attrRefVal = String.Join(",", refTable.Where(x => x.Value[0] == type.ToString()).Select(x => "\"" + x.Key + "\"").ToArray());


            string lookup = mapRef.lookup;
            MapRefObjectValue[] values = mapRef.mapRefObjectValue;

            string valueCode = "";

            if (values != null)
            {
                valueCode = "Dictionary<string, string> valLookup = new Dictionary<string, string>(){";
                var arLookup = values.Select(x => "{\"" + x.a + "\",\"" + x.b + "\"}");
                string list = string.Join(",", arLookup);
                valueCode += list;
                valueCode += "};";
            }

            string removeCode = "";
            if (type == refObjects.ReleaseStatus)
                removeCode = "_RemoveRelStatus(m);";
            else
                removeCode = "_RemoveRefObject(m," + attrRefVal + ");";

            if (lookup != null)
            {
                MapCode.AppendFormat(@"try
             {{
                 _query = _xml.Elements(_ns + ""{0}""); 

                 foreach(var m in _query)
                   {{
                        string refVal = Lookup(""{2}"",m.GetAttrValue(""{1}""));
                        
                        if(refVal == """")
                        {3}
                        else if (refVal != null)
                        m.SetAttrValue(""{1}"",refVal);
                   }}
             }}
             catch(System.Exception ex)
             {{
                 throw new System.Exception(ex.Message);}}", type.ToString(), attributeRef, lookup, removeCode);
            }
            else if (lookup == null && values != null)
            {
                MapCode.AppendFormat(@"try
             {{
                 _query = _xml.Elements(_ns + ""{0}""); 

                 {2}

                 foreach(var m in _query)
                   {{
                        string refVal = null;
                        valLookup.TryGetValue(m.GetAttrValue(""{1}""), out refVal);

                        if(refVal == """")
                        {3}
                        else if (refVal != null)
                        m.SetAttrValue(""{1}"",refVal);
                   }}
             }}
             catch(System.Exception ex)
             {{
                 throw new System.Exception(ex.Message);}}", type.ToString(), attributeRef, valueCode, removeCode);
            }



            return MapCode;
        }

        public static ModelClass getTcModel(this HeaderModel model, string obj)
        {
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
            List<string> RecordedMapsList = new List<string>();

            var maps = m.Items.Where(x => x.GetType() == typeof(Map)).Cast<Map>().Where(x => x.srccheck != null && x.a != x.b).Select(x => x.a).Distinct().ToArray();

            foreach (var map in maps)
            {
                RecordedMapsList.Add(map);
                var classes = getFullModel(map, m);
                if (classes != null)
                    RecordedMapsList.AddRange(classes);
            }

            return RecordedMapsList.Distinct().ToArray();
        }

        public static string[] getToRecordedMaps(Maps m)
        {
            List<string> RecordedMapsList = new List<string>();

            var maps = m.Items.Where(x => x.GetType() == typeof(Map)).Cast<Map>().Where(x => x.srccheck != null && x.a != x.b).Select(x => x.b).Distinct().ToArray();

            foreach (var map in maps)
            {
                RecordedMapsList.Add(map);
                var classes = getFullModel(map, m);
                if (classes != null)
                    RecordedMapsList.AddRange(classes);
            }

            return RecordedMapsList.Distinct().ToArray();
        }

        public static Dictionary<string, string> getOneToOneMaps(Maps m, string[] MapChanges)
        {
            Dictionary<string, string> MapList = new Dictionary<string, string>();

            var maps = m.Items.Where(x => x.GetType() == typeof(Map)).Cast<Map>().Where(x => x.srccheck == null && x.a != x.b && !MapChanges.Contains(x.a)).Select(x => new { From = getFullModel(x.a, m), To = getFullModel(x.b, m) });
            List<string[]> mapSrc = new List<string[]>();
            List<string[]> mapTgt = new List<string[]>();


            List<string> invalid_maps = new List<string>();
            string invalidMapRegex = "Cannot map {0} to {1} - {2} is not defined in model";
            foreach (var map in m.Items.Where(x => x.GetType() == typeof(Map)).Cast<Map>().Where(x => x.srccheck == null && x.a != x.b && !MapChanges.Contains(x.a)))
            {
                var src = getFullModel(map.a, m);
                var trgt = getFullModel(map.b, m);

                if (src == null && trgt != null)
                    invalid_maps.Add(String.Format(invalidMapRegex, map.a, map.b, map.a));
                else if (trgt == null && src != null)
                    invalid_maps.Add(String.Format(invalidMapRegex, map.a, map.b, map.b));
                else if (src != null && trgt != null)
                {
                    mapSrc.Add(src);
                    mapTgt.Add(trgt);
                }
            }

            if (invalid_maps.Count() > 0)
            {
                invalid_maps.ForEach(x => Console.WriteLine(x));
                return null;
            }

            var mapSrcArr = mapSrc.SelectMany(x => x).ToArray();
            var mapTgtArr = mapTgt.SelectMany(x => x).ToArray();

            for (int i = 0; i < mapSrcArr.Count(); i++)
            {
                MapList[mapSrcArr[i]] = mapTgtArr[i];
            }

            return MapList;
        }

        private static string[] getFullModel(string tclass, Maps m)
        {
            var classes = m.header.model.classes;
            if (classes == null) return null;

            var model = classes.Where(x => x.item == tclass || x.itemrevision == tclass).Select(x => new string[] { x.item, x.itemrevision, x.masterform, x.masterformS, x.masterformRev, x.masterformRevS }).FirstOrDefault();

            return model;
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

            string sb = "{0}.SetAttrValue(\"{1}\",{2});";

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

            string sb = "{0}.SetAttrValue(\"{1}\",{2});";

            string queryBuild = String.Format(sb, alias, mAttribute.name, value.Replace("'", "\""));

            return queryBuild;
        }

        private static string ConvertMapAttr(MapAttr mAttr, string alias)
        {

            if (mAttr.copy == null)
            {
                string sb = "{0}.SetAttrValue(\"{1}\",{0}.GetAttrValue(\"{2}\"));if({0}.Attribute(\"{2}\") != null){0}.Attribute(\"{2}\").Remove();";

                string queryBuild = String.Format(sb, alias, mAttr.b, mAttr.a);

                return queryBuild;
            }
            else
            {
                string sb = "{0}.SetAttrValue(\"{1}\",{0}.GetAttrValue(\"{2}\"));";

                string queryBuild = String.Format(sb, alias, mAttr.b, mAttr.a);

                return queryBuild;
            }


        }

        private static string ConvertMapAttr(AttrCheckAttr mAttr, string alias)
        {
            if (mAttr.copy == null)
            {
                string sb = "{0}.SetAttrValue(\"{1}\",{0}.GetAttrValue(\"{2}\"));if({0}.Attribute(\"{2}\") != null){0}.Attribute(\"{2}\").Remove();";

                string queryBuild = String.Format(sb, alias, mAttr.b, mAttr.a);

                return queryBuild;
            }
            else
            {
                string sb = "{0}.SetAttrValue(\"{1}\",{0}.GetAttrValue(\"{2}\").Value);";

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
                result = _RefReplace(result, patternRefJoin, alias);
                result = Regex.Replace(result, patternJoins, "$1.GetAttrValue(\"" + "$2" + "\")");

                string pattern = @"@([\w]*)";
                result = _RefReplace(result, patternRef, "");
                result = Regex.Replace(result, pattern, alias + ".GetAttrValue(\"" + "$1" + "\")");

                result = result.Replace(".GetAttrValue(\"release_status_list\")", ".Attribute(\"release_status_list\")");

                return result;
            }
            else
                return val;
        }

        private static string _RefReplace(string input, string pattern, string alias)
        {
            string gRef = "";

            if (alias != "")
            {
                gRef = Regex.Replace(input, pattern, "_GetRefVal(\"" + "$2" + "\", " + alias + "@" + "$2" + ")");
            }
            else
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
        public string[] MapChanges, ToMapChanges;
        public Dictionary<string, string> One2OneMaps = new Dictionary<string, string>();
        private HeaderModel model;
        private int TotalMapCount = 0;


        public void ConvertMaps(Maps maps)
        {
            model = maps.header.model;
            Extensions.model = model;
            MapChanges = Extensions.getRecordedMaps(maps);
            ToMapChanges = Extensions.getToRecordedMaps(maps);
            One2OneMaps = Extensions.getOneToOneMaps(maps, MapChanges);
            FunctionCode = ConvertFunctions(maps.header);
            LookupCode = ConvertLookups(maps);
            VariableCode = ConvertVariables(maps.header);


            //Mappings that don't match the model list
            var srcMdlMiss = (from m in maps.Items.Where(x => x.GetType() == typeof(Map)).Cast<Map>()
                              where model.getTcType(m.b) == ""
                              select m.b).Distinct().ToList();

            if (srcMdlMiss.Count() > 0) { Console.WriteLine("--Source Mappings that don't match model--"); srcMdlMiss.ForEach(Console.WriteLine); }

            var trgtMdlMiss = (from m in maps.Items.Where(x => x.GetType() == typeof(Map)).Cast<Map>()
                               where model.getTcType(m.a) == ""
                               select m.a).Distinct().ToList();

            if (trgtMdlMiss.Count() > 0) { Console.WriteLine("--Target Mappings that don't match model--"); trgtMdlMiss.ForEach(Console.WriteLine); }
            //End

            //Map Reference Objects
            foreach (HeaderMapRefObject m in maps.header.mapRefObject)
            {
                MapCode.AppendLine(m.Convert().ToString());
            }

            MapCode.AppendLine("_LoadRefTables();");

            //convert Map
            TotalMapCount = maps.Items.Count();
            int i = 1;
            foreach (object m in maps.Items)
            {
                if (m.GetType().Name == "Map")
                {
                    MapCode.AppendLine("\n\n// Map :" + i.ToString());
                    MapCode.AppendLine("Global._mapCounter = " + i.ToString() + ";");
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

        private string ConvertModelTable()
        {
            var last = model.classes.Last();
            StringBuilder sb = new StringBuilder("public static string[,] classes = new string[,] {");
            string format = "{{\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"}}";

            foreach (var m in model.classes)
            {
                string s = String.Format(format, m.item, m.itemrevision, m.masterform, m.masterformS, m.masterformRev, m.masterformRevS);

                if (m.Equals(last))
                {
                    sb.Append(s);
                }
                else
                {
                    s += ",";

                    sb.AppendLine(s);
                }
            }

            sb.Append("};");

            var last2 = MapChanges.LastOrDefault();
            StringBuilder sb2 = new StringBuilder("\npublic static string[] recordedClasses = new string[] {");

            foreach (var m in MapChanges)
            {
                string s = String.Format("\"{0}\"", m);

                if (m.Equals(last2))
                {
                    sb2.Append(s);
                }
                else
                {
                    s += ",";

                    sb2.AppendLine(s);
                }
            }

            sb2.AppendLine("};");

            sb2.Append("public static Dictionary<string,byte> recordedToClasses = new Dictionary<string,byte> {");

            for (int i = 0; i < ToMapChanges.Count(); i++)
            {
                string s = String.Format("{{\"{0}\",{1}}}", ToMapChanges[i], i);

                if (i + 1 == ToMapChanges.Count())
                {
                    sb2.Append(s);
                }
                else
                {
                    s += ",";

                    sb2.AppendLine(s);
                }
            }

            sb2.AppendLine("};");

            sb2.Append("public static Dictionary<string,string> OneToOneMaps = new Dictionary<string,string> {");
            var last3 = One2OneMaps.LastOrDefault();
            foreach (var kv in One2OneMaps)
            {
                string s = String.Format("{{\"{0}\",\"{1}\"}}", kv.Key, kv.Value);

                if (kv.Equals(last3))
                {
                    sb2.Append(s);
                }
                else
                {
                    s += ",";

                    sb2.AppendLine(s);
                }
            }

            sb2.AppendLine("};");


            return sb.AppendLine(sb2.ToString()).ToString();
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
            string GlobalCode = Properties.Resources.Global;
            string CreateAccesDatabase = Properties.Resources.CreateAccesDatabase;
            string ClassCode = Properties.Resources.Classes;
            string ErrorInfoCode = Properties.Resources.ErrorInfo;
            string StringExtensions = Properties.Resources.StringExtensions;

            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" }, { "Platform", "x64" } });
            var parameters = new CompilerParameters(new[] { "System.dll", "mscorlib.dll", "System.Core.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Data.dll" }, outFile, false);
            parameters.ReferencedAssemblies.Add(@"System.Data.SqlServerCe.dll");


            parameters.GenerateExecutable = true;
            parameters.IncludeDebugInformation = true;
            parameters.CompilerOptions = "/platform:x64";

            StringBuilder Code = new StringBuilder();

            //Reference Table info
            string refTbl = "";//ConvertRefTable();

            string modelTbl = ConvertModelTable();

            ClassCode = ClassCode.Insert(23, modelTbl);

            string logRegex = @"-log=(?:\"")?(.+)(?:\"")?";

            Code.AppendFormat(@"
            using System.Linq;
            using System.Xml.Linq;
            using System.Collections;
            using System.Collections.Generic;
            using System.Data.SqlServerCe;
            using System.ComponentModel;
            using System.IO;
            using System.Diagnostics;

            {11}

            {10}

            {9}
            
            {7}

            class Program 
            {{
                                

                private static XElement {0} = null;
                private static XNamespace _ns = null;

                public static Dictionary<string, Dictionary<string, string>> _lookups = new Dictionary<string, Dictionary<string, string>>();
                public static Dictionary<string,string> _variables = new Dictionary<string,string>{{{5}}};
                public static Dictionary<string,string[]> _TypeChangeLog = new Dictionary<string,string[]>();

                public static string _IMAN_master_form = null;

                {6}

                public static Dictionary<string, string> UserRef = new Dictionary<string, string>();
                public static Dictionary<string, string> GroupRef = new Dictionary<string, string>();
                public static Dictionary<string, string> UnitOfMeasureRef = new Dictionary<string, string>();
                public static Dictionary<string, string> ImanVolumeRef = new Dictionary<string, string>();
                public static Dictionary<string, string> DatasetTypeRef = new Dictionary<string, string>();
                public static Dictionary<string, string> ImanTypeRef = new Dictionary<string, string>();
                public static Dictionary<string, string> ToolRef = new Dictionary<string, string>();

                public static Dictionary<string, string> UserRefVal = new Dictionary<string, string>();
                public static Dictionary<string, string> GroupRefVal = new Dictionary<string, string>();
                public static Dictionary<string, string> UnitOfMeasureRefVal = new Dictionary<string, string>();
                public static Dictionary<string, string> ImanVolumeRefVal = new Dictionary<string, string>();
                public static Dictionary<string, string> DatasetTypeRefVal = new Dictionary<string, string>();
                public static Dictionary<string, string> ImanTypeRefVal = new Dictionary<string, string>();
                public static Dictionary<string, string> ToolRefVal = new Dictionary<string, string>();

               public static Dictionary<string, XElement> ReleaseStatusRef = new Dictionary<string, XElement>();

                public static int Main(string[] args) 
                {{
                   
                    string log = args.ToList().Where(x => System.Text.RegularExpressions.Regex.IsMatch(x, ""{12}"",System.Text.RegularExpressions.RegexOptions.IgnoreCase)).SingleOrDefault();

                    if (log != null)
                    {{
                        string logfile = System.Text.RegularExpressions.Regex.Match(log,""{12}"").Groups[1].Value;
                        Global.LogFile = new StreamWriter(logfile, false);
                        Global.LogFile.AutoFlush = true;
                    }}

                    if (args.Contains(""-warn"")) Global._DisableWarnings = false;
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    
                    _lookups = LoadAllLookups({3});               

                    int _maxCount = {8};
                    bool db_result = CreateDatabase(Classes.recordedClasses);
                   
                    (""Loading XML - "" + args[0]).Print();
                    try{{{0} = XElement.Load(args[0]);}}catch(System.Exception ex){{Global._errList.Add(new ErrorList.ErrorInfo(0, ErrorCodes.XML_MALFORMED, """", """",TCTypes.General, """"));}}

                    _ns = {0}.GetDefaultNamespace();
                    IEnumerable<XElement> {1} =  Enumerable.Empty<XElement>();
                    IEnumerable<XElement> {4} =  Enumerable.Empty<XElement>();

                    

                    string _IMAN_master_form = _GetRef(""relation_type"", ""IMAN_master_form"");

                    Classes._xml = _xml;
                    Classes._ns = _ns;
                    Classes._IMAN_master_form = _IMAN_master_form;

                    (""Updating Stubs"").Print();
                     _UpdateStubs();

                     Classes._ReadXML();
                    (""Caching Complete"").Print();

                    Global.initElemID(_xml);
                    RemoveRoles();

                    {2}

                    _AddChangeToDB();
                    _ReconcileLocalStubs();

                    (""Saving mapped XML - "" + args[1]).Print();
                    {0}.Save(args[1]);

                    stopWatch.Stop();
                    System.TimeSpan ts = stopWatch.Elapsed;
                    string elapsedTime = System.String.Format(""{{0:00}}:{{1:00}}.{{2:00}}"",ts.Minutes, ts.Seconds,ts.Milliseconds / 10);
                    (""Total Duration - "" + elapsedTime).Print();

                    System.Console.WriteLine(""Writing log file"");
                    ("""").Print();   

                    Global._errList.Print();
                    Global.LogFile.Close();

                    return 0;
                    
                }}", Extensions.xmlFile,        //0
                Extensions.queryName,           //1
                MapCode,                        //2
                LookupCode,                     //3
                Extensions.AttrChkQueryName,    //4
                VariableCode,                   //5
                refTbl.ToString(),              //6
                ClassCode,                      //7
                TotalMapCount.ToString(),       //8
                StringExtensions,               //9
                ErrorInfoCode,                  //10   
                GlobalCode,                     //11
                logRegex                        //12    
                );



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
