﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.33440.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "maps")]
public partial class Maps
{
    private Header headerField;


    /// <remarks/>
    public Header header
    {
        get
        {
            return this.headerField;
        }
        set
        {
            this.headerField = value;
        }
    }


    
  
    private object[] mapField;


    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("map", typeof(Map))]
    [System.Xml.Serialization.XmlElementAttribute("script", typeof(Script))]
    public object[] Items
    {
        get
        {
            return this.mapField;
        }
        set
        {
            this.mapField = value;
        }
    }

}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class Header
{

    private HeaderVariable[] varField;
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("variable")]
    public HeaderVariable[] variable
    {
        get
        {
            return this.varField;
        }
        set
        {
            this.varField = value;
        }
    }

    private HeaderLookup[] lookupField;
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("lookup")]
    public HeaderLookup[] lookup
    {
        get
        {
            return this.lookupField;
        }
        set
        {
            this.lookupField = value;
        }
    }

    private HeaderFunction[] functionField;
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("function")]
    public HeaderFunction[] function
    {
        get
        {
            return this.functionField;
        }
        set
        {
            this.functionField = value;
        }
    }



}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class HeaderLookup
{

    private string fileField;

    private string nameField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string file
    {
        get
        {
            return this.fileField;
        }
        set
        {
            this.fileField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class HeaderVariable
{
    private string nameField;
    private string valueField;

    [XmlAttribute("name")]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    [XmlAttribute("value")]
    public string value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class HeaderFunction
{
    [XmlText]
    public string data = "";


}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class Script
{
    [XmlText]
    public string data = "";

   
}


    /// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class Map
{

    private SrcCheck srccheckField;
    private SrcJoin[] srcJoinField;

    private object[] itemsField;

    private string aField;

    private string bField;

    private string mapclassField;

    public Map()
    {
        this.mapclassField = "no";
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("join")]
    public SrcJoin[] srcjoin
    {
        get
        {
            return this.srcJoinField;
        }
        set
        {
            this.srcJoinField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("src-check")]
    public SrcCheck srccheck
    {
        get
        {
            return this.srccheckField;
        }
        set
        {
            this.srccheckField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("attr", typeof(MapAttr))]
    [System.Xml.Serialization.XmlElementAttribute("attr-check", typeof(MapAttrCheck))]
    [System.Xml.Serialization.XmlElementAttribute("attribute", typeof(MapAttribute))]
    public object[] Items
    {
        get
        {
            return this.itemsField;
        }
        set
        {
            this.itemsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string a
    {
        get
        {
            return this.aField;
        }
        set
        {
            this.aField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string b
    {
        get
        {
            return this.bField;
        }
        set
        {
            this.bField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("map-class")]
    [System.ComponentModel.DefaultValueAttribute("no")]
    public string mapclass
    {
        get
        {
            return this.mapclassField;
        }
        set
        {
            this.mapclassField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class SrcJoin
{

    private string onField;
    private string objField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string on
    {
        get
        {
            return this.onField;
        }
        set
        {
            this.onField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string obj
    {
        get
        {
            return this.objField;
        }
        set
        {
            this.objField = value;
        }
    }

    
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class SrcCheck
{

    private string ifField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @if
    {
        get
        {
            return this.ifField;
        }
        set
        {
            this.ifField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MapAttr
{

    private object copyField;

    private string aField;

    private string bField;

    /// <remarks/>
    public object copy
    {
        get
        {
            return this.copyField;
        }
        set
        {
            this.copyField = value;
        }
    }

    /// <remarks/>
    public string a
    {
        get
        {
            return this.aField;
        }
        set
        {
            this.aField = value;
        }
    }

    /// <remarks/>
    public string b
    {
        get
        {
            return this.bField;
        }
        set
        {
            this.bField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MapAttrCheck
{

    private object[] itemsField;

    private string ifField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("attr", typeof(AttrCheckAttr))]
    [System.Xml.Serialization.XmlElementAttribute("attribute", typeof(AttrCheckAttribute))]
    public object[] Items
    {
        get
        {
            return this.itemsField;
        }
        set
        {
            this.itemsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @if
    {
        get
        {
            return this.ifField;
        }
        set
        {
            this.ifField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class AttrCheckAttr
{

    private object copyField;

    private string aField;

    private string bField;

    /// <remarks/>
    public object copy
    {
        get
        {
            return this.copyField;
        }
        set
        {
            this.copyField = value;
        }
    }

    /// <remarks/>
    public string a
    {
        get
        {
            return this.aField;
        }
        set
        {
            this.aField = value;
        }
    }

    /// <remarks/>
    public string b
    {
        get
        {
            return this.bField;
        }
        set
        {
            this.bField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class AttrCheckAttribute
{

    private string nameField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MapAttribute
{

    private string nameField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}
