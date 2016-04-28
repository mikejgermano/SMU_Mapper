using SMU_Mapper.Classes;
using SMU_Mapper.Properties;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SMU_Mapper
{
    class MMLReader
    {
        private string _file ="";
        private XDocument _xmlDoc;
        private Maps _maps = null;

        public Maps Maps { get { return _maps; } }

        /// <summary>
        /// Gets the MML XML Document
        /// </summary>
        public XDocument MMLDoc { get { return _xmlDoc; } }


        /// <summary>
        /// Gets or Sets the path of the MML file
        /// </summary>
        public string MapFile
        {
            get { return _file; }
            set
            {
                if (!Validate_LoadMXML(value)) { throw new Exception("Your XML file has some issues"); } else _file = value; ;
            }
        }

        public bool Validate_LoadMXML(string mFile)
        {
            bool errors = false;

            if (!File.Exists(mFile)) throw new FileNotFoundException("File does not Exist");

            if (Path.GetExtension(mFile).ToUpper() != ".XML") throw new FileNotFoundException("File is not an XML document");

            //Validate against XSD

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add(null, XmlReader.Create(new StringReader(Resources.MMLSchema)));

            XmlReaderSettings xrs = new XmlReaderSettings();
            xrs.ValidationType = ValidationType.Schema;
            xrs.ValidationFlags =
                XmlSchemaValidationFlags.ReportValidationWarnings |
                XmlSchemaValidationFlags.ProcessIdentityConstraints |
                XmlSchemaValidationFlags.ProcessInlineSchema |
                XmlSchemaValidationFlags.ProcessSchemaLocation;
            xrs.Schemas = schemaSet;
            xrs.ValidationEventHandler += (sender,vargs) => 
            {
                //write to log
                IXmlLineInfo info = sender as IXmlLineInfo;
                if (vargs.Severity == XmlSeverityType.Error) errors = true;
                Console.WriteLine("{0}: {1}; Line: {2}", vargs.Severity, vargs.Message, info != null ? info.LineNumber.ToString() : "not known");
            };

            using (XmlReader xr = XmlReader.Create(mFile, xrs))
            {
                _xmlDoc = XDocument.Load(xr, LoadOptions.SetLineInfo);
            }


            return !errors;
        }

        public MMLReader(string mFile)
        {
            MapFile = mFile;
            LoadMaps();

        }

        private void LoadMaps()
        {

            var map = from el in MMLDoc.Descendants("map")
                       select el;

            using (FileStream xmlStream = new FileStream(MapFile, FileMode.Open))
            {
                using (XmlReader xmlReader = XmlReader.Create(xmlStream))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Maps));
                    Maps deserializedMaps = serializer.Deserialize(xmlReader) as Maps;
                    this._maps = deserializedMaps;
                }
            }
        }


    }
}
