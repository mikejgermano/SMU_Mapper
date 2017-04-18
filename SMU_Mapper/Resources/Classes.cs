static class Classes
{
    public static XElement _xml = null;
    public static XNamespace _ns;
    public static string _IMAN_master_form;

    static private XElement[] _getMasterForm(XElement obj, string StorageClass)
    {
        var islandElements = Islands[obj.Attribute("island_id").Value];

        var MasterForm = (from im in islandElements.Where(x => x.Name.LocalName == "ImanRelation")
                          join mForm in islandElements.Where(x => x.Name.LocalName == "Form") on im.Attribute("secondary_object").Value equals mForm.Attribute("puid").Value
                          join mFormS in islandElements.Where(x => x.Name.LocalName == StorageClass) on mForm.Attribute("data_file").Value equals mFormS.Attribute("puid").Value into mf1
                          from mFormS in mf1.DefaultIfEmpty()
                          where obj.Attribute("puid").Value == im.Attribute("primary_object").Value && im.Attribute("relation_type").Value == _IMAN_master_form
                          select new XElement[2] { mForm, mFormS }).SingleOrDefault();

        return MasterForm;
    }

    public class ItemClass
    {
        public XElement element;
        public ItemMasterFormClass masterForm;
        public IEnumerable<RevisionClass> revisions;

        public ItemClass(XElement xitem, string srev, string smasterFormS, string smasterRevFormS)
        {
            element = xitem;
            masterForm = new ItemMasterFormClass(element, smasterFormS);
            revisions = _getItemRevisions(element, srev, smasterRevFormS);
        }

        private static IEnumerable<Classes.RevisionClass> _getItemRevisions(XElement item, string sRev, string StorageClass)
        {
            var islandElements = Islands[item.Attribute("island_id").Value];

            var Revisions = (from rev in islandElements.Where(x => x.Name.LocalName == sRev)
                             where rev.Attribute("items_tag").Value == item.Attribute("puid").Value
                             select new Classes.RevisionClass { element = rev, masterRevForm = new RevisionClass.ItemRevMasterFormClass() }).ToList();

            if (Revisions.Count() == 0)
                Revisions = (from rev in _xml.Elements(_ns + sRev)
                             where rev.Attribute("items_tag").Value == item.Attribute("puid").Value
                             select new Classes.RevisionClass { element = rev, masterRevForm = new RevisionClass.ItemRevMasterFormClass() }).ToList();

            if (Revisions.Count() == 0)
            {
                Global._errList.Add(new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.REVISIONS_NOT_FOUND, item.Attribute("puid").Value, sRev, TCTypes.Item, item.Attribute("item_id").Value));
            }

            foreach (var r in Revisions)
            {
                r.masterRevForm = new RevisionClass.ItemRevMasterFormClass(r.element, StorageClass);
            }

            return Revisions;
        }

    }

    public class ItemMasterFormClass
    {
        public XElement element;
        public XElement storage;

        public ItemMasterFormClass(XElement item, string smasterFormS)
        {
            var forms = _getMasterForm(item, smasterFormS);
            if (forms == null) return;
            element = forms[0];
            storage = forms[1];
        }
    }

    public class RevisionClass
    {
        public ItemClass item;
        public XElement element;
        public ItemRevMasterFormClass masterRevForm;

        public RevisionClass() { }

        public RevisionClass(XElement xrevision, string sitem, string srev, string sMasterFormS, string sMasterRevFormS)
        {
            element = xrevision;

            masterRevForm = new ItemRevMasterFormClass(element, sMasterRevFormS);

            item = _getItem(element, sitem, srev, sMasterFormS, sMasterRevFormS);
        }


        public class ItemRevMasterFormClass
        {
            public XElement element;
            public XElement storage;

            public ItemRevMasterFormClass() { }

            public ItemRevMasterFormClass(XElement rev, string sMasterFormS)
            {
                var forms = _getMasterForm(rev, sMasterFormS);
                if (forms == null) return;
                element = forms[0];
                storage = forms[1];
            }
        }

        private static ItemClass _getItem(XElement rev, string sitem, string srev, string sMasterFormS, string smasterRevFormS)
        {
            var islandElements = Islands[rev.Attribute("island_id").Value];

            try
            {
                ItemClass qItem = (from item in islandElements.Where(x => x.Name.LocalName == sitem)
                                   where item.Attribute("puid").Value == rev.Attribute("items_tag").Value
                                   select new ItemClass(item, srev, sMasterFormS, smasterRevFormS)).SingleOrDefault();


                //check if in another island
                if (qItem == null)
                {
                    qItem = (from item in _xml.Elements(_ns + sitem)
                             where item.Attribute("puid").Value == rev.Attribute("items_tag").Value
                             select new ItemClass(item, srev, sMasterFormS, smasterRevFormS)).Single();
                }

                return qItem;
            }
            catch (System.Exception ex)
            {
                Global._errList.Add(new ErrorList.ErrorInfo(0, ErrorCodes.ITEM_NOT_FOUND, rev.Attribute("puid").Value, srev, TCTypes.ItemRevision, "<" + rev.Attribute("items_tag").Value + ">", rev.Attribute("item_revision_id").Value));

                return null;
            }
        }

    }

    public static Dictionary<string, IEnumerable<XElement>> Islands = new Dictionary<string, IEnumerable<XElement>>();

    public static void _ReadXML()
    {
        int bound0 = classes.GetUpperBound(0);
        List<string> ItemList = new List<string>(bound0);
        List<string> RevList = new List<string>(bound0);
        List<string> ItemMasterList = new List<string>(bound0);
        List<string> ItemRevList = new List<string>(bound0);
        List<string> ItemMasterSList = new List<string>(bound0);
        List<string> ItemRevSList = new List<string>(bound0);

        for (int i = 0; i <= bound0; i++)
        {
            ItemList.Add(classes[i, 0]);
            RevList.Add(classes[i, 1]);
            ItemMasterList.Add(classes[i, 2]);
            ItemMasterSList.Add(classes[i, 3]);
            ItemRevList.Add(classes[i, 4]);
            ItemRevSList.Add(classes[i, 5]);
        }


        var masterList = ItemList.Union(RevList).Union(ItemMasterSList).Union(ItemRevSList).ToList();
        masterList.Add("ImanRelation");

        var formList = ItemMasterList.Union(ItemRevList);

        var islandData = from el in _xml.Elements()
                         where masterList.Contains(el.Name.LocalName) || (el.Name.LocalName != "POM_stub" && el.Attribute("object_type") != null && formList.Contains(el.Attribute("object_type").Value))
                         group el by el.Attribute("island_id").Value into g
                         orderby g.Key
                         select g;

        foreach (var el in islandData)
        {
            string islandID = el.Key;
            Islands.Add(islandID, el);

        }
    }
}