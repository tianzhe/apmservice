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
    
    public partial class STK_MKT_RiskFactorDaily
    {
        public Nullable<double> SecurityID { get; set; }
        public string Symbol { get; set; }
        public string TradingDate { get; set; }
        public Nullable<double> Yield { get; set; }
        public Nullable<double> Volatility { get; set; }
        public Nullable<double> Beta1 { get; set; }
        public Nullable<double> Beta2 { get; set; }
        public Nullable<double> Cor1 { get; set; }
        public Nullable<double> Cor2 { get; set; }
        public Nullable<double> NonSysRisk1 { get; set; }
        public Nullable<double> NonSysRisk2 { get; set; }
        public Nullable<double> Rsq1 { get; set; }
        public Nullable<double> Rsq2 { get; set; }
        public Nullable<double> ARsq1 { get; set; }
        public Nullable<double> ARsq2 { get; set; }
        public System.Guid UniqueID { get; set; }
        public Nullable<System.DateTime> TradingDate2 { get; set; }
    
        public virtual TRD_Co TRD_Co { get; set; }
    }
}
