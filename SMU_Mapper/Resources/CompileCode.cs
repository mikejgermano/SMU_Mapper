private static void _UpdateGroupMembers()
{
    var GroupMemebers = _xml.Elements(_ns + "GroupMember").ToArray();
    foreach (var gm in GroupMemebers)
    {
        string group = GroupRef[gm.Attribute("group").Value.Remove(0, 1)];
        string user = UserRef[gm.Attribute("user").Value.Remove(0, 1)];
        string role = RoleRef[gm.Attribute("role").Value.Remove(0, 1)];

        string newMember = string.Join(",", group, user, role);
        gm.SetAttrValue("member", newMember);
    }
}

private static void _RemoveGroupRoleList()
{
    var Groups = _xml.Elements(_ns + "Group").Where(x => x.Attribute("list_of_role") != null).ToArray();
    foreach (var g in Groups)
    {
        g.Attribute("list_of_role").Remove();
    }
}

private static Dictionary<string, string> LoadLookup(string path)
{
    Dictionary<string, string> LoadLookup_d = new Dictionary<string, string>();
    try
    {
        var lookups = System.IO.File.ReadAllLines(path).Select(x => x.Split('|'));

        foreach (var l in lookups)
        {
            if (!LoadLookup_d.ContainsKey(l[0]))
                LoadLookup_d.Add(l[0], l[1]);
            else
            {
                Global._errList.Add(new ErrorList.ErrorInfo(0, ErrorCodes.DUPLICATE_LOOKUP_KEY, "", "", TCTypes.General, path, l[0], l[1]));
            }
        }

        if (LoadLookup_d.Count() == 0)
            System.Console.WriteLine("Warning - " + System.IO.Path.GetFileName(path) + " is empty");
    }
    catch (System.IO.FileNotFoundException nf)
    {
        System.Console.WriteLine("Error - loading lookups, couldn't find the lookup file.\nCheck that your path is correct:\n" + nf.FileName);
        System.Environment.Exit(1);
    }
    catch (System.IndexOutOfRangeException)
    {
        System.Console.WriteLine("Error - loading lookup in " + System.IO.Path.GetFileName(path) + "\n...most likely key/value pair is malformed");
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

    Dictionary<string, string> d1;
    string value = null;

    if (_lookups.TryGetValue(name, out d1))
    {
        d1.TryGetValue(key, out value);
    }

    return value;

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

private static void _LoadRefTables()
{
    var elements = _xml.Elements(_ns + "User");
    foreach (var el in elements)
    {
        string elemId = el.Attribute("elemId").Value; string refVal = el.Attribute("user_id").Value;
        UserRef[elemId] = refVal; UserRefVal[refVal] = elemId;
    }

    elements = _xml.Elements(_ns + "Group");
    foreach (var el in elements)
    {
        string elemId = el.Attribute("elemId").Value; string refVal = el.Attribute("full_name").Value;
        GroupRef[elemId] = refVal; GroupRefVal[refVal] = elemId;
    }

    elements = _xml.Elements(_ns + "UnitOfMeasure");
    foreach (var el in elements)
    {
        string elemId = el.Attribute("elemId").Value; string refVal = el.Attribute("symbol").Value;
        UnitOfMeasureRef[elemId] = refVal; UnitOfMeasureRefVal[refVal] = elemId;
    }

    elements = _xml.Elements(_ns + "ImanVolume");
    foreach (var el in elements)
    {
        string elemId = el.Attribute("elemId").Value; string refVal = el.Attribute("volume_name").Value;
        ImanVolumeRef[elemId] = refVal; ImanVolumeRefVal[refVal] = elemId;
    }

    elements = _xml.Elements(_ns + "DatasetType");
    foreach (var el in elements)
    {
        string elemId = el.Attribute("elemId").Value; string refVal = el.Attribute("datasettype_name").Value;
        DatasetTypeRef[elemId] = refVal; DatasetTypeRefVal[refVal] = elemId;
    }

    elements = _xml.Elements(_ns + "ImanType");
    foreach (var el in elements)
    {
        string elemId = el.Attribute("elemId").Value; string refVal = el.Attribute("type_name").Value;
        ImanTypeRef[elemId] = refVal; ImanTypeRefVal[refVal] = elemId;
    }

    elements = _xml.Elements(_ns + "Tool");
    foreach (var el in elements)
    {
        string elemId = el.Attribute("elemId").Value; string refVal = el.Attribute("object_name").Value;
        ToolRef[elemId] = refVal; ToolRefVal[refVal] = elemId;
    }

    elements = _xml.Elements(_ns + "Role");
    foreach (var el in elements)
    {
        string elemId = el.Attribute("elemId").Value; string refVal = el.Attribute("role_name").Value;
        RoleRef[elemId] = refVal; RoleRefVal[refVal] = elemId;
    }

    ReleaseStatusRef = _xml.Elements(_ns + "ReleaseStatus").ToDictionary(x => x.Attribute("puid").Value, x => x);
}

private static string _GetRef(string attribute, string refVal)
{
    string elemId = "";

    if (Global.refTable.ContainsKey(attribute))
    {
        var dval = Global.refTable[attribute];

        switch (dval[0])
        {
            case "User":
                UserRefVal.TryGetValue(refVal, out elemId);
                break;
            case "Group":
                GroupRefVal.TryGetValue(refVal, out elemId);
                break;
            case "UnitOfMeasure":
                UnitOfMeasureRefVal.TryGetValue(refVal, out elemId);
                break;
            case "ImanVolume":
                ImanVolumeRefVal.TryGetValue(refVal, out elemId);
                break;
            case "DatasetType":
                DatasetTypeRefVal.TryGetValue(refVal, out elemId);
                break;
            case "ImanType":
                ImanTypeRefVal.TryGetValue(refVal, out elemId);
                break;
            case "Tool":
                ToolRefVal.TryGetValue(refVal, out elemId);
                break;
        }

        if (elemId == null)
        {
            Global._errList.Add(new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.MISSING_REF_OBJECT, "", "", TCTypes.SystemObject, dval[0], refVal));
            return "";
        }

        if (elemId == "")
            return "";
    }

    return ("#" + elemId);
}


private static string _GetRefVal(string attribute, string elemid)
{
    string value = null;

    if (elemid == "") return "";

    string id = elemid.Replace("#", "");

    string RefType = Global.refTable[attribute][0];
    string RefAttr = Global.refTable[attribute][1];


    switch (RefType)
    {
        case "User":
            UserRef.TryGetValue(id, out value);
            break;
        case "Group":
            GroupRef.TryGetValue(id, out value);
            break;
        case "UnitOfMeasure":
            UnitOfMeasureRef.TryGetValue(id, out value);
            break;
        case "ImanVolume":
            ImanVolumeRef.TryGetValue(id, out value);
            break;
        case "DatasetType":
            DatasetTypeRef.TryGetValue(id, out value);
            break;
        case "ImanType":
            ImanTypeRef.TryGetValue(id, out value);
            break;
        case "Tool":
            ToolRef.TryGetValue(id, out value);
            break;
    }


    return value;
}


private static string _GetRelSts(XAttribute attribute)
{
    try
    {
        if (attribute == null || attribute.Value == "") return null;
        XElement el = null;
        ReleaseStatusRef.TryGetValue(attribute.Value, out el);
        string statuses = string.Join(",", el.Attribute("name").Value);

        return statuses;
    }
    catch { return ""; }
}

private static string _SetRelSts(XAttribute attribute, string Val)
{
    string newPUID = "";

    if (attribute == null || attribute.Value == "")
    { }
    else
    {

        if (Val == "")
        {
            return "";
        }


        newPUID = attribute.Value;

        XElement el = null;
        ReleaseStatusRef.TryGetValue(attribute.Value, out el);

        if (Val == null)
        {
            newPUID = "";
            el.Remove();
        }
        else
            el.SetAttributeValue("name", Val);

    }

    return newPUID;
}


private static void _RemoveRefObject(XElement el, params string[] attributes)
{
    string uid = "#" + el.Attribute("elemId").Value;


    foreach (var attr in attributes)
    {
        var query = _xml.Elements().Where(x => x.Attribute(attr) != null && x.Attribute(attr).Value == uid);

        foreach (var element in query)
        {
            element.SetAttributeValue(attr, null);
        }
    }

    el.Remove();

}

private static void _RemoveRelStatus(XElement el)
{
    string uid = el.Attribute("puid").Value;


    var query = _xml.Elements().Where(x => x.Attribute("release_status_list") != null && x.Attribute("release_status_list").Value == uid);

    foreach (var element in query)
    {
        element.SetAttributeValue("release_status_list", null);
    }

    el.Remove();
}

private static void _UpdateStubs()
{
    var Stubs = _xml.Elements(_ns + "POM_stub");
    List<XElement> RecordStubs = new List<XElement>();
    int one2one = 0, PartialMap = 0;
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

            if (Classes.isSecondary(toClass))
            {
                toClass = Classes.getSecondaryClass(toClass);
            }

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
        if (Classes.isSecondary(e.ChangeRecord[1]))
            e.Stub.SetAttributeValue("object_class", Classes.getSecondaryClass(e.ChangeRecord[1]));
        else
            e.Stub.SetAttributeValue("object_class", e.ChangeRecord[1]);
        e.Stub.SetAttributeValue("object_type", e.ChangeRecord[1]);
    }
}

private static void _RecordTypeChange(string puid, string from, string to)
{
    _TypeChangeLog[puid] = new string[2] { from, to };
}

private static void _TCPropagateItem(XElement _item, string sItem, string sRev, string sMasterForm, string sMasterFormS, string sMasterRevForm, string sMasterRevFormS, string tItem, string tRev, string tMasterForm, string tMasterFormS, string tMasterRevForm, string tMasterRevFormS, int secondaryRule)
{
    string sItemClass = sItem;
    string sRevClass = sRev;
    string tItemClass = sItem;
    string tRevClass = sRev;

    if (secondaryRule == 1)
    {
        sItemClass = "Item";
        sRevClass = "ItemRevision";
    }
    else if (secondaryRule == 2)
    {
        tItemClass = "Item";
        tRevClass = "ItemRevision";
    }
    else if (secondaryRule == 3)
    {

        sItemClass = "Item";
        sRevClass = "ItemRevision";

        tItemClass = "Item";
        tRevClass = "ItemRevision";
    }

    var Item = new Classes.ItemClass(_item, sRev, sMasterFormS, sMasterRevFormS);


    //change Item
    _RecordTypeChange(Item.element.Attribute("puid").Value, Item.element.Attribute("object_type").Value, tItem);
    Item.element.SetAttributeValue("object_type", tItem);
    Item.element.Name = _ns + tItemClass;

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
        _RecordTypeChange(tc.element.Attribute("puid").Value, tc.element.Attribute("object_type").Value, tRev);
        tc.element.SetAttributeValue("object_type", tRev);
        tc.element.Name = _ns + tRevClass;

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

private static void _TCPropagateItemRevision(XElement _rev, string sItem, string sRev, string sMasterForm, string sMasterFormS, string sMasterRevForm, string sMasterRevFormS, string tItem, string tRev, string tMasterForm, string tMasterFormS, string tMasterRevForm, string tMasterRevFormS, int secondaryRule)
{
    string sItemClass = sItem;
    string sRevClass = sRev;
    string tItemClass = sItem;
    string tRevClass = sRev;

    if (secondaryRule == 1)
    {
        sItemClass = "Item";
        sRevClass = "ItemRevision";
    }
    else if (secondaryRule == 2)
    {
        tItemClass = "Item";
        tRevClass = "ItemRevision";
    }
    else if (secondaryRule == 3)
    {

        sItemClass = "Item";
        sRevClass = "ItemRevision";

        tItemClass = "Item";
        tRevClass = "ItemRevision";
    }




    var Revision = new Classes.RevisionClass(_rev, sItem, sRev, sMasterFormS, sMasterRevFormS);

    //change Item
    _RecordTypeChange(Revision.item.element.Attribute("puid").Value, Revision.item.element.Attribute("object_type").Value, tItem);
    Revision.item.element.SetAttributeValue("object_type", tItem);
    Revision.item.element.Name = _ns + tItemClass;

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
        _RecordTypeChange(tc.element.Attribute("puid").Value, tc.element.Attribute("object_type").Value, tRev);
        tc.element.SetAttributeValue("object_type", tRev);
        tc.element.Name = _ns + tRevClass;

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
    foreach (var tc in items)
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