using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SMU_Mapper
{
    public static class CreateModel
    {
        public static void Create(string ModelXML,string outFile)
        {
            XElement doc = XElement.Load(ModelXML);
            XNamespace ns = doc.GetDefaultNamespace();

            var ItemModel = (from item in doc.Descendants(ns + "TcTypeConstantAttach")
                             join IM in doc.Descendants(ns + "TcTypeConstantAttach") on item.Attribute("typeName").Value equals IM.Attribute("typeName").Value
                             join IRM in doc.Descendants(ns + "TcTypeConstantAttach") on item.Attribute("value").Value equals IRM.Attribute("typeName").Value
                             where item.Attribute("constantName").Value == "ItemRevision" && IM.Attribute("constantName").Value == "MasterForm" && IRM.Attribute("constantName").Value == "MasterForm"
                             select new { Item = item.Attribute("typeName").Value, ItemRevision = item.Attribute("value").Value, IM = IM.Attribute("value").Value, IRM = IRM.Attribute("value").Value }).OrderBy(x => x.Item);


            string[] PrintModel = ItemModel.Select(x => String.Join(",", new Object[] { x.Item, x.ItemRevision, x.IM, x.IRM })).ToArray();

            File.WriteAllLines(outFile, PrintModel);

        }



        public static void Schema(string xsdFile, string outFile)
        {

            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("http://www.tcxml.org/Schemas/TCXMLSchema", xsdFile);

            schema.Compile();
            XmlSchemaObjectTable table = schema.GlobalElements;
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.Indent = true;

            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(outFile, settings))
            {

                writer.WriteStartDocument(true);
                writer.WriteStartElement("BMO");
                //writer.WriteLine("USE Test");

                XmlSchemaElement[] array = new XmlSchemaElement[table.Values.Count];
                table.Values.CopyTo(array, 0);

                foreach (XmlSchemaElement el in array)
                {
                    if (el.ElementSchemaType.BaseXmlSchemaType.GetType().Name.ToString() == "XmlSchemaComplexType")
                    {
                        string elementName = el.Name;

                        XmlSchemaComplexType mType = (XmlSchemaComplexType)el.ElementSchemaType;
                        XmlSchemaAttribute[] Attributes = mType.AttributeUses.Values.Cast<XmlSchemaAttribute>().Select(e => e).ToArray();

                        if (Attributes.Count() == 0)
                            continue;

                        writer.WriteStartElement(elementName);

                       // writer.WriteLine("CREATE TABLE [" + elementName + "] (");

                        XmlSchemaAttribute last = Attributes.Last();
                        foreach (XmlSchemaAttribute attr in Attributes)
                        {
                            
                            string name = attr.Name;
                           // string type = attr.AttributeSchemaType.TypeCode.ToString();
                            string requirement = attr.Use.ToString();
                            requirement = ((requirement == "Required") ? "Y" : "N");
                           

                            writer.WriteStartElement(name);
                            //writer.WriteStartAttribute("req");
                            //writer.WriteValue(requirement);
                            //writer.WriteEndAttribute();

                            //if (attr.Equals(last))
                            //    writer.WriteLine("\t[" + name + "] " + typeReplace + ((requirement == "Required") ? " NOT NULL" : ""));
                            //else
                            //    writer.WriteLine("\t[" + name + "] " + typeReplace + ((requirement == "Required") ? " NOT NULL" : "") + ",");

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();

                    }
                }

              
                writer.WriteEndDocument();
            }
        }

    }
}
