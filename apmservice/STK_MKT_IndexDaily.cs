//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace apmservice
{
    using System;
    using System.Collections.Generic;
    
    public partial class STK_MKT_IndexDaily
    {
        public string IndexID { get; set; }
        public string TradingDate { get; set; }
        public Nullable<double> OpenIndex { get; set; }
        public Nullable<double> HighIndex { get; set; }
        public Nullable<double> LowIndex { get; set; }
        public Nullable<double> CloseIndex { get; set; }
        public Nullable<double> ConstituentStockTradeVolume { get; set; }
        public Nullable<double> ConstituentStockTradeAmount { get; set; }
        public Nullable<double> IndexReturnRate { get; set; }
        public System.Guid UniqueID { get; set; }
        public Nullable<System.DateTime> TradingDate2 { get; set; }
    }
}
