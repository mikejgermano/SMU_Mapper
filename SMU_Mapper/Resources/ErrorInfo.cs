public enum ErrorCodes
{
    ///<summary>Mapping has been skipped because it found zero source objects</summary>
    [Description("Mapping has been skipped because it found zero source objects")]
    [ErrorType(ErrorTypes.Warning)]
    MAP_QUERY_EMPTY,

    ///<summary> WorkspaceObject is missing MasterForm</summary>
    [Description("WorkspaceObject is missing MasterForm")]
    [ErrorType(ErrorTypes.SevereError)]
    MISSING_MASTERFORM,

    ///<summary>Attribute was added to current element because it did not exist</summary>
    [Description("Attribute was added to current element because it did not exist")]
    [ErrorType(ErrorTypes.Warning)]
    ATTRIBUTE_NULL,

    ///<summary>Deletion skipped for attribute because it does not exist on object</summary>
    [Description("Deletion skipped for attribute because it does not exist on object")]
    [ErrorType(ErrorTypes.Warning)]
    DELETE_NULL_ATTRIBUTE,

    ///<summary>Class index could not be found for stubbing Database</summary>
    [Description("Class index could not be found for stubbing Database")]
    [ErrorType(ErrorTypes.FatalError)]
    CLASS_INDEX_NOT_FOUND
}
public enum TCTypes {Attribute, Mapping, General, WorkspaceObject, Item, ItemMaster, ItemMasterS, ItemRevision, ItemRevisionMaster, ItemRevMasterS, Dataset, BOM, Relation, SystemObject };
public enum ErrorTypes { SevereError = 0, FatalError = 1, Warning = 2 };
public class ErrorType : System.Attribute
{

    public virtual ErrorTypes type { get; set; }

    public ErrorType(ErrorTypes mtype) { this.type = mtype; }
}
public class ErrorList : List<ErrorList.ErrorInfo>
{
    public new void Add(ErrorInfo item)
    {
        if (Global._DisableWarnings == true && item.ErrType == ErrorTypes.Warning) return;

        item.Log();
        base.Add(item);
    }
    public class ErrorInfo
    {



        public string GetEnumDescription(ErrorCodes value)
        {
            System.Reflection.FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
        public ErrorTypes GetErrorType(ErrorCodes value)
        {
            System.Reflection.FieldInfo fi = value.GetType().GetField(value.ToString());

            ErrorType[] attributes =
                (ErrorType[])fi.GetCustomAttributes(typeof(ErrorType), false);

            return attributes[0].type;

        }
        public ErrorCodes code;
        public TCTypes TCType;
        public ErrorTypes ErrType;
        /// <summary>
        /// Item        - item_id<para/>
        /// Master Form - object_name<para/>
        /// Revision    - item_revision_id<para/>
        /// Dataset     - object_name<para/>
        /// </summary>
        public string TCID;
        public string UID, ObjectType, Message ,MapNum;

        public ErrorInfo(int MapNum, ErrorCodes code, string UID, string ObjectType, TCTypes TCType, params string[] GSID)
        {
            this.MapNum = MapNum.ToString("D2");
            this.code = code;
            this.Message = GetEnumDescription(code);
            ErrType = GetErrorType(code);

            this.UID = UID;
            this.ObjectType = ObjectType;
            this.TCType = TCType;
            TCID = string.Join("¬", GSID);
        }
    }
}