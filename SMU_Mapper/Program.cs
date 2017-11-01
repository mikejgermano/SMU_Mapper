using SMU_Mapper.Classes;
using System;
using System.IO;

namespace SMU_Mapper
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                DateTime endDT = new DateTime(2018, 1, 15,0, 0, 0, 0, DateTimeKind.Utc);

                if(DateTime.UtcNow > endDT)
                {
                    Console.WriteLine("License has expired. Please obtain a new license");
                    return;
                }

                //CreateModel.Create(@"D:\Work\ATO\mapper\Model\Amway-model.xml", "src-ItemModel.txt");
                //CreateModel.Schema(@"C:\Users\germano\Desktop\mapper\TCXML_Schema\Denso\source_TCXML.xsd", "src-attributes.xml");

                //CreateModel.Create(@"D:\Work\ATO\mapper\Model\tgtD-model.xml", "target-ItemModel.txt");
                //CreateModel.Schema(@"C:\Users\germano\Desktop\mapper\TCXML_Schema\Denso\target_TCXML.xsd", "target-attributes.xml");
                //return;

                //Read MML File
                string file = args[0];
                MMLReader mReader = new MMLReader(file);
                Console.WriteLine("{0} loaded successfully",Path.GetFileName(file));
                
                //Compile Maps 2 code
                MapConverter converter = new MapConverter();
                converter.ConvertMaps(mReader.Maps);
                var results = converter.Compile(args[1]);


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + Environment.NewLine);
            }
        }
    }
}
