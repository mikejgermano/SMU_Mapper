public static bool CreateDatabase(string[] classes)
{
    if (Directory.Exists("Data") && Directory.GetFiles("Data").Count() > 0)
        return false;


    Global.Print("Create Database");
    bool result = true;


    foreach (var c in classes)
    {
        string connectionString = "DataSource=\".\\Data\\" + c + ".sdf" + "\"";
        SqlCeEngine en = new SqlCeEngine(connectionString);
        en.CreateDatabase();

        SqlCeConnection conn = null;

        try
        {
            conn = new SqlCeConnection(connectionString);
            conn.Open();

            SqlCeCommand cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE POM_CHANGES(PUID nvarchar(14) CONSTRAINT pkid PRIMARY KEY, TO_TYPE tinyint)";
            cmd.ExecuteNonQuery();
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine(ex.Message);
            result = false;
        }
        finally
        {
            conn.Close();
        }
    }

    Global.Log(classes.Count().ToString() + " tables created");

    result = true;

    return result;
}

public static string[][] _GetChangeData(string c)
{
    var data = _TypeChangeLog.Where(x => x.Value[0] == c).Select(x => new string[2] { x.Key, x.Value[1] }).ToArray();

    return data;
}

public static void _AddChangeToDB()
{
    var data = _TypeChangeLog.Where(x => Classes.recordedClasses.Contains(x.Value[0])).Select(x => x.Value[0]).Distinct();

    foreach (var c in data)
    {
        _UpdateTable(c);
    }
}

public static void _UpdateTable(string ChangeStubs)
{
    var data = _GetChangeData(ChangeStubs);
    int count = 0;
    SqlCeConnection connection = new SqlCeConnection("DataSource =\".\\Data\\" + ChangeStubs + ".sdf\"");
    using (var cmd = new SqlCeCommand())
    {
        connection.Open();
        cmd.CommandType = System.Data.CommandType.TableDirect;
        cmd.CommandText = "POM_CHANGES";
        cmd.Connection = connection;
        cmd.IndexName = "pkid";
        try
        {
            using (var result = cmd.ExecuteResultSet(ResultSetOptions.Scrollable | ResultSetOptions.Updatable))
            {
                for (int i = 0; i < data.Count(); i++)
                {
                    string pkValue = data[i][0]; // set this, obviously
                    byte TO = 0;
                    bool test = Classes.recordedToClasses.TryGetValue(data[i][1], out TO);

                    if (!test) Global._errList.Add(new ErrorList.ErrorInfo(-1, ErrorCodes.CLASS_INDEX_NOT_FOUND, "", data[i][1], TCTypes.General, i.ToString()));

                    if (!result.Seek(DbSeekOptions.FirstEqual, pkValue))
                    {

                        // row doesn't exist, insert
                        var record = result.CreateRecord();

                        // set values
                        record.SetValue(0, pkValue);
                        record.SetByte(1, TO);


                        result.Insert(record);
                        count++;
                    }
                }
            }
            if(count != 0)
            string.Format("Stubs Created [{0}] : {1}", count.ToString("D4"), ChangeStubs).Log();
        }
        catch (System.Exception ex) { System.Console.WriteLine(ex.Message); }
    }
}