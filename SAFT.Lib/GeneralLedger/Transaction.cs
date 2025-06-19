using System;
using System.Xml.Serialization;

namespace SAFT.Lib.GeneralLedger
{
    public class Transaction
    {
        [XmlElement]
        public string TransactionID { get; set; }
        [XmlElement]
        public int Period { get; set; }
        [XmlElement]
        public DateTime TransactionDate { get; set; }
        [XmlElement]
        public string SourceID { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [XmlElement]
        public string DocArchivalNumber { get; set; }
        [XmlElement]
        public string TransactionType { get; set; }
        [XmlElement]
        public DateTime GLPostingDate { get; set; }
        [XmlElement]
        public string CustomerID { get; set; }
        [XmlElement]
        public string SupplierID { get; set; }
        [XmlElement]
        public Lines Lines { get; set; }
    }
} 