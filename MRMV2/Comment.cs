//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MRMV2
{
    using System;
    using System.Collections.Generic;
    
    public partial class Comment
    {
        public string Comment_ID { get; set; }
        public string Recipe_ID { get; set; }
        public string Comment1 { get; set; }
        public string Comment_Date { get; set; }
        public string User_ID { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Recipe Recipe { get; set; }
    }
}
