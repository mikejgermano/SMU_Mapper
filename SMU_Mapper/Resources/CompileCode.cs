private static Dictionary<string, string> LoadLookup(string path)
                {
                        Dictionary<string, string> LoadLookup_d = new Dictionary<string, string>();
                        try
                        {
                            LoadLookup_d = System.IO.File.ReadAllLines(path).Select(x => x.Split('|')).ToDictionary(x => x[0], x => x[1]);
      
                            if(LoadLookup_d.Count() == 0)
                            System.Console.WriteLine("Warning - " + System.IO.Path.GetFileName(path) + " is empty");
                        }
                        catch(System.IO.FileNotFoundException nf)
                        {
                            System.Console.WriteLine("Error - loading lookups, couldn't find the lookup file.\nCheck that your path is correct:\n" + nf.FileName);
                            System.Environment.Exit(1);
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            System.Console.WriteLine("Error - loading lookup in " + System.IO.Path.GetFileName(path) +  "\n...most likely key/value pair is malformed");
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
            string value = "";

            if (elemid == "") return "";

            elemid = elemid.Replace("#", "");

            string RefType = refTable[attribute][0];
            string RefAttr = refTable[attribute][1];

            value = (from el in _xml.Elements(_ns + RefType)
                       where el.Attribute("elemId").Value == elemid
                       select el.Attribute(RefAttr).Value).Single();



                return value;
        }


    private static string _GetRelSts(XAttribute attribute)
    {
        try{ 
        if (attribute == null || attribute.Value == "") return "";

        var els = _xml.Elements(_ns + "ReleaseStatus").Where(x => x.Attribute("puid").Value == attribute.Value).Select(x => x.Attribute("name").Value).ToArray();
        string statuses = string.Join(",", els);

            return statuses;
        }
        catch { return "";}
     }

    private static string _SetRelSts(XAttribute attribute, string Val)
    {
        string newPUID = "";

        if (attribute == null || attribute.Value == "")
        { }
        else
        {
        
            if(Val == "")
            {
                return "";
            }
         

            newPUID = attribute.Value;

            var els = _xml.Elements(_ns + "ReleaseStatus").Where(x => x.Attribute("puid").Value == attribute.Value);

            foreach (var el in els)
            {

                if (Val == null)
                {
                    newPUID = "";
                    el.Remove();
                }
                else
                el.SetAttributeValue("name", Val);
            }
        }

        return newPUID;

    }

    private static string _GetRef(string attribute, string refVal){
            string elemId = "";

                if(refTable.ContainsKey(attribute))
                {
                    var dval = refTable[attribute];
                     var el = _xml.Elements(_ns + dval[0]).Where(x => x.Attribute(dval[1]).Value == refVal).Select(x => x.Attribute("elemId").Value);

                    if (el.Count() == 1)
                        elemId = el.Single();
                    else
                        return "";
                }

             return ("#" + elemId);
            }

private static void _UpdateStubs()
{
    var Stubs = _xml.Elements(_ns + "POM_stub");
    List<XElement> RecordStubs = new List<XElement>();
    int one2one = 0, PartialMap =0;
    foreach (var stub in Stubs)
    {
        string oType = stub.Attribute("object_type").Value;

       

        //One2One
        if (Classes.OneToOneMaps.ContainsKey(oType))
        {
            one2one++;
            stub.Attribute("object_type").Value = Classes.OneToOneMaps[oType];

            if (stub.Attribute("object_class").Value != "Form")
            {
                stub.Attribute("object_class").Value = Classes.OneToOneMaps[oType];
            }
        }
        else if (Classes.recordedClasses.Contains(oType)) { RecordStubs.Add(stub); }
    }

    var groups = RecordStubs.GroupBy(x => x.Attribute("object_type").Value);

    foreach (var group in groups)
    {
        SqlCeConnection connection = new SqlCeConnection("DataSource =\".\\Data\\" + group.Key + ".sdf\"");
        SqlCeCommand cmd = new SqlCeCommand("SELECT TO_TYPE FROM POM_CHANGES WHERE PUID = @id", connection);
        cmd.Parameters.Add("@id", System.Data.SqlDbType.NVarChar, 14);


        connection.Open();
        foreach (var g in group)
        {
            cmd.Parameters[0].Value = g.Attribute("object_uid").Value;
            object value = cmd.ExecuteScalar();

            if (value == null) continue;
            PartialMap++;
            string toClass = Classes.recordedToClasses.Single(x => x.Value == (byte)value).Key;

            g.Attribute("object_type").Value = toClass;

            if (g.Attribute("object_class").Value != "Form")
            {
                g.Attribute("object_class").Value = toClass;
            }
        }

        connection.Close();
    }

    ("One2One Stubs: " + one2one.ToString()).Log();
    ("PartialMap Stubs: " + PartialMap.ToString()).Log();

}

private static void _ReconcileLocalStubs()
    {
        var stubs = from stub in _xml.Elements(_ns + "POM_stub")
                    join chg in _TypeChangeLog on stub.Attribute("object_uid").Value equals chg.Key
                    select new { Stub = stub, ChangeRecord = chg.Value };

        foreach (var e in stubs)
        {
            e.Stub.SetAttributeValue("object_class", e.ChangeRecord[1]);
            e.Stub.SetAttributeValue("object_type", e.ChangeRecord[1]);
        }
    }

private static void _RecordTypeChange(string puid, string from, string to)
    {
            _TypeChangeLog[puid] = new string[2]{from,to};
    }

private static void _TCPropagateItem(XElement _item, string sItem, string sRev, string sMasterForm, string sMasterFormS, string sMasterRevForm, string sMasterRevFormS, string tItem, string tRevision, string tMasterForm, string tMasterFormS, string tMasterRevForm, string tMasterRevFormS)
{
    var Item = new Classes.ItemClass(_item, sRev, sMasterFormS, sMasterRevFormS);

    //change Item
    _RecordTypeChange(Item.element.Attribute("puid").Value, Item.element.Attribute("object_type").Value, tItem);
    Item.element.SetAttributeValue("object_type", tItem);
    Item.element.Name = _ns + tItem;

    //change masterForm
    if (Item.masterForm.element != null)
    {
        _RecordTypeChange(Item.masterForm.element.Attribute("puid").Value, Item.masterForm.element.Attribute("object_type").Value, tMasterForm);
        Item.masterForm.element.Attribute("object_type").Value = tMasterForm;
    }
    else
    {
        ErrorList.ErrorInfo err = new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.MISSING_MASTERFORM, _item.Attribute("puid").Value, _item.Name.LocalName, TCTypes.Item, _item.Attribute("item_id").Value);
        Global._errList.Add(err);
    }

    //change masterForm Storage
    if (Item.masterForm.storage != null)
    {
        _RecordTypeChange(Item.masterForm.storage.Attribute("puid").Value, Item.masterForm.storage.Name.LocalName, tMasterFormS);
        Item.masterForm.storage.Name = _ns + tMasterFormS;
    }

    foreach (var tc in Item.revisions)
    {
        //change revision type
        _RecordTypeChange(tc.element.Attribute("puid").Value, tc.element.Attribute("object_type").Value, tRevision);
        tc.element.SetAttributeValue("object_type", tRevision);
        tc.element.Name = _ns + tRevision;

        //change masterRevForm
        if (tc.masterRevForm.element != null)
        {
            _RecordTypeChange(tc.masterRevForm.element.Attribute("puid").Value, tc.masterRevForm.element.Attribute("object_type").Value, tMasterRevForm);
            tc.masterRevForm.element.Attribute("object_type").Value = tMasterRevForm;
        }
        else
        {
            ErrorList.ErrorInfo err = new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.MISSING_MASTERFORM, tc.element.Attribute("puid").Value, tc.element.Name.LocalName, TCTypes.ItemRevision, Item.element.Attribute("item_id").Value, tc.element.Attribute("item_revision_id").Value);
            Global._errList.Add(err);
        }

        if (tc.masterRevForm.storage != null)
        {
            //change masterRevFormStorage
            _RecordTypeChange(tc.masterRevForm.storage.Attribute("puid").Value, tc.masterRevForm.storage.Name.LocalName, tMasterRevFormS);
            tc.masterRevForm.storage.Name = _ns + tMasterRevFormS;
        }
    }
}

private static void _TCPropagateItemRevision(XElement _rev, string sItem, string sRevision, string sMasterForm, string sMasterFormS, string sMasterRevForm, string sMasterRevFormS, string tItem, string tRevision, string tMasterForm, string tMasterFormS, string tMasterRevForm, string tMasterRevFormS)
{
    var Revision = new Classes.RevisionClass(_rev, sItem, sRevision, sMasterFormS, sMasterRevFormS);

    //change Item
    _RecordTypeChange(Revision.item.element.Attribute("puid").Value, Revision.item.element.Attribute("object_type").Value, tItem);
    Revision.item.element.SetAttributeValue("object_type", tItem);
    Revision.item.element.Name = _ns + tItem;

    //change masterForm
    if (Revision.item.masterForm.element != null)
    {
        _RecordTypeChange(Revision.item.masterForm.element.Attribute("puid").Value, Revision.item.masterForm.element.Attribute("object_type").Value, tMasterForm);
        Revision.item.masterForm.element.Attribute("object_type").Value = tMasterForm;
    }
    else
    {
        ErrorList.ErrorInfo err = new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.MISSING_MASTERFORM, Revision.item.element.Attribute("puid").Value, Revision.item.element.Name.LocalName, TCTypes.Item, Revision.item.element.Attribute("item_id").Value);
        Global._errList.Add(err);
    }

    //change masterForm Storage
    if (Revision.item.masterForm.storage != null)
    {
        _RecordTypeChange(Revision.masterRevForm.storage.Attribute("puid").Value, Revision.masterRevForm.storage.Name.LocalName, tMasterFormS);
        Revision.item.masterForm.storage.Name = _ns + tMasterFormS;
    }

    foreach (var tc in Revision.item.revisions)
    {
        //change revision type
        _RecordTypeChange(tc.element.Attribute("puid").Value, tc.element.Attribute("object_type").Value, tRevision);
        tc.element.SetAttributeValue("object_type", tRevision);
        tc.element.Name = _ns + tRevision;

        //change masterRevForm
        if (tc.masterRevForm.element != null)
        {
            _RecordTypeChange(tc.masterRevForm.element.Attribute("puid").Value, tc.masterRevForm.element.Attribute("object_type").Value, tMasterRevForm);
            tc.masterRevForm.element.Attribute("object_type").Value = tMasterRevForm;
        }
        else
        {
            ErrorList.ErrorInfo err = new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.MISSING_MASTERFORM, tc.element.Attribute("puid").Value, tc.element.Name.LocalName, TCTypes.ItemRevision, Revision.item.element.Attribute("item_id").Value, tc.element.Attribute("item_revision_id").Value);
            Global._errList.Add(err);
        }

        //change masterRevFormStorage
        if (tc.masterRevForm.storage != null)
        {
            _RecordTypeChange(tc.masterRevForm.storage.Attribute("puid").Value, tc.masterRevForm.storage.Name.LocalName, tMasterRevFormS);
            tc.masterRevForm.storage.Name = _ns + tMasterRevFormS;
        }
    }
}

private static void _FastTCPropagateItem(string sItem, string sRev, string sMasterForm, string sMasterFormS, string sMasterRevForm, string sMasterRevFormS, string tItem, string tRevision, string tMasterForm, string tMasterFormS, string tMasterRevForm, string tMasterRevFormS)
    {
        var items = _xml.Elements(_ns + sItem);
        foreach(var tc in items)
        {
            _RecordTypeChange(tc.Attribute("puid").Value, sItem, tItem);
            tc.SetAttributeValue("object_type", tItem);
            tc.Name = _ns + tItem;
        }

        var masterforms = from mf in _xml.Elements(_ns + "Form")
                          where mf.Attribute("object_type").Value == sMasterForm
                          select mf;
        foreach (var tc in masterforms)
        {
            _RecordTypeChange(tc.Attribute("puid").Value, sMasterForm, tMasterForm);
            tc.SetAttributeValue("object_type", tMasterForm);
        }

        var masterformsS = _xml.Elements(_ns + sMasterFormS);
        foreach (var tc in masterformsS)
        {
            _RecordTypeChange(tc.Attribute("puid").Value, sMasterFormS, tMasterFormS);
            tc.SetAttributeValue("object_type", tMasterFormS);
            tc.Name = _ns + tMasterFormS;
        }

        var revisions = _xml.Elements(_ns + sRev);
        foreach (var tc in revisions)
        {
            _RecordTypeChange(tc.Attribute("puid").Value, sRev, tRevision);
            tc.SetAttributeValue("object_type", tRevision);
            tc.Name = _ns + tRevision;
        }

        var masterrevforms = from mf in _xml.Elements(_ns + "Form")
                          where mf.Attribute("object_type").Value == sMasterRevForm
                          select mf;
        foreach (var tc in masterrevforms)
        {
            _RecordTypeChange(tc.Attribute("puid").Value, sMasterRevForm, tMasterRevForm);
            tc.SetAttributeValue("object_type", tMasterRevForm);
        }

        var masterrevformsS = _xml.Elements(_ns + sMasterRevFormS);
        foreach (var tc in masterrevformsS)
        {
            _RecordTypeChange(tc.Attribute("puid").Value, sMasterRevFormS, tMasterRevFormS);
            tc.SetAttributeValue("object_type", tMasterRevFormS);
            tc.Name = _ns + tMasterRevFormS;
        }
    }