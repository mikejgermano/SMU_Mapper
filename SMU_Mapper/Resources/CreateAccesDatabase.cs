public static bool CreateNewAccessDatabase()
    {

		System.Console.WriteLine("CreateNewAccessDatabase()");
        bool result = false;

        string connectionString = "DataSource=\"test.sdf\"; Password=\"mypassword\"";
        SqlCeEngine en = new SqlCeEngine(connectionString);
        en.CreateDatabase();

        SqlCeConnection conn = null;

		result = true;

        try
        {
            conn = new SqlCeConnection(connectionString);
            conn.Open();

			//CREATE TABLE FooTable(UID int PRIMARY KEY, TO_TYPE tinyint)

            SqlCeCommand cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE FooTable(PUID nvarchar(14) PRIMARY KEY, FROM_TYPE nvarchar(35), TO_TYPE nvarchar(35))";
            cmd.ExecuteNonQuery();
        }
        catch(System.Exception ex)
        {
			System.Console.WriteLine(ex.Message);
			result = false;
        }
        finally
        {
            conn.Close();
        }

        return result;
    }