public static class Global
{
    public static int _mapCounter = 0;

    public static bool _DisableWarnings = true;
    public static StreamWriter LogFile;
    public static ErrorList _errList = new ErrorList();


    public static string GetAttrValue(this XElement el, XName name)
    {
        var attr = el.Attribute(name);

        if (attr == null)
        {
            Global._errList.Add(new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.ATTRIBUTE_NULL, el.Attribute("puid").Value, el.Attribute("object_type").Value, TCTypes.WorkspaceObject, name.LocalName));
            return null;
        }


        return attr.Value;
    }
    public static void SetAttrValue(this XElement el, XName name, object value)
    {
        if (el.Attribute(name) == null)
        {
            if (value == null)
            {
                Global._errList.Add(new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.DELETE_NULL_ATTRIBUTE, el.Attribute("puid").Value, el.Name.LocalName, TCTypes.Attribute, name.LocalName));
                return;
            }
            ErrorList.ErrorInfo err = new ErrorList.ErrorInfo(Global._mapCounter, ErrorCodes.ATTRIBUTE_NULL, el.Attribute("puid").Value, el.Name.LocalName, TCTypes.Attribute, name.LocalName);
            Global._errList.Add(err);
        }
        else
        {
            el.SetAttributeValue(name, value);
        }
    }

    public static void Print(this string line)
    {
        System.Console.WriteLine(line);

        if (line == "")
            LogFile.WriteLine("");
        else
            LogFile.WriteLine("[" + System.DateTime.Now.ToString("dd-MMM-yy HH:mm") + "] " + line);
    }

    public static void Log(this string line)
    {
        if (line == "")
            LogFile.WriteLine("");
        else
            LogFile.WriteLine("[" + System.DateTime.Now.ToString("dd-MMM-yy HH:mm") + "] " + line);
    }

    public static void Log(this ErrorList.ErrorInfo e)
    {
        if (e.UID == "")
        {
            string logGFormat = "[{5}] <Map#{6}> {0} [{1}] - {2} -> {3}: |{4}|";
            LogFile.WriteLine(string.Format(logGFormat, e.ErrType, e.code, e.Message, e.TCType, e.TCID, System.DateTime.Now.ToString("dd-MMM-yy HH:mm"), e.MapNum));
        }
        else
        {
            string logFormat = "[{7}] <Map#{8}> {0} [{1}] - {2} -> PUID:{3} ObjectType:{4} {5}: |{6}|";
            LogFile.WriteLine(string.Format(logFormat, e.ErrType, e.code, e.Message, e.UID, e.ObjectType, e.TCType, e.TCID, System.DateTime.Now.ToString("dd-MMM-yy HH:mm"), e.MapNum));
        }

        if (e.ErrType == ErrorTypes.FatalError)
        {

            WriteLine("The utility has encountered a fatal error. Exiting...", System.ConsoleColor.Red);
            System.Environment.Exit(1);
        }
    }
    public static void Print(this ErrorList e)
    {
        if (e.Count() == 0) return;

        int errors = _errList.Where(x => x.ErrType == ErrorTypes.SevereError).Count();
        int warnings = _errList.Where(x => x.ErrType == ErrorTypes.Warning).Count();
        var fcolor = System.Console.ForegroundColor;
        var bcolor = System.Console.BackgroundColor;

        System.Console.ForegroundColor = System.ConsoleColor.Red;
        System.Console.WriteLine("Errors found   : " + errors);
        System.Console.ForegroundColor = fcolor;
        System.Console.ForegroundColor = System.ConsoleColor.Yellow;
        System.Console.WriteLine("Warnings found : " + warnings);
        System.Console.ForegroundColor = fcolor;
        System.Console.WriteLine("Check log for more details");
    }

    public static void WriteLine(string message, System.ConsoleColor color)
    {
        var fcolor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = color;
        System.Console.WriteLine(message);
        System.Console.ForegroundColor = fcolor;
        System.Console.WriteLine("Check log for more details");
    }
}