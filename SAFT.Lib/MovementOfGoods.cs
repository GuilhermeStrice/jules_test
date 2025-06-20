/*
 * Original XSD Schema:
 * <xs:element name="MovementOfGoods" minOccurs="0">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="NumberOfMovementLines"/>
 * <xs:element ref="TotalQuantityIssued"/>
 * <xs:element name="StockMovement" minOccurs="0" maxOccurs="unbounded">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="DocumentNumber"/>
 * <xs:element ref="ATCUD"/>
 * <xs:element name="DocumentStatus">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="MovementStatus"/>
 * <xs:element ref="MovementStatusDate"/>
 * <xs:element ref="Reason" minOccurs="0"/>
 * <xs:element ref="SourceID"/>
 * <xs:element name="SourceBilling" type="SAFTPTSourceBilling"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * <xs:element ref="Hash"/>
 * <xs:element ref="HashControl"/>
 * <xs:element ref="Period" minOccurs="0"/>
 * <xs:element ref="MovementDate"/>
 * <xs:element ref="MovementType"/>
 * <xs:element ref="SystemEntryDate"/>
 * <xs:element ref="TransactionID" minOccurs="0"/>
 * <xs:choice>
 * <xs:element ref="CustomerID"/>
 * <xs:element ref="SupplierID"/>
 * </xs:choice>
 * <xs:element ref="SourceID"/>
 * <xs:element ref="EACCode" minOccurs="0"/>
 * <xs:element ref="MovementComments" minOccurs="0"/>
 * <xs:element ref="ShipTo" minOccurs="0" maxOccurs="1"/>
 * <xs:element ref="ShipFrom" minOccurs="0" maxOccurs="1"/>
 * <xs:element ref="MovementEndTime" minOccurs="0" maxOccurs="1"/>
 * <xs:element ref="MovementStartTime" maxOccurs="1"/>
 * <xs:element ref="ATDocCodeID" minOccurs="0" maxOccurs="1"/>
 * <xs:element name="Line" maxOccurs="unbounded">
 * <!-- Complex Line structure -->
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: Movement of goods containing stock movements with product lines and totals.
 */

using System.Collections.Generic;
using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents movement of goods with stock movements and summary information.
    /// </summary>
    public class MovementOfGoods
    {
        /// <summary>
        /// The number of movement lines.
        /// </summary>
        [XmlElement("NumberOfMovementLines")]
        public int NumberOfMovementLines { get; set; }

        /// <summary>
        /// The total quantity issued.
        /// </summary>
        [XmlElement("TotalQuantityIssued")]
        public decimal TotalQuantityIssued { get; set; }

        /// <summary>
        /// Collection of stock movements (optional, unbounded).
        /// </summary>
        [XmlElement("StockMovement")]
        public List<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }

    /// <summary>
    /// Represents a stock movement with document details and product lines.
    /// </summary>
    public class StockMovement
    {
        /// <summary>
        /// The document number.
        /// </summary>
        [XmlElement("DocumentNumber")]
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// The ATCUD (Unique Document Code).
        /// </summary>
        [XmlElement("ATCUD")]
        public string ATCUD { get; set; } = string.Empty;

        /// <summary>
        /// The current status of the document.
        /// </summary>
        [XmlElement("DocumentStatus")]
        public MovementDocumentStatus DocumentStatus { get; set; } = new MovementDocumentStatus();

        /// <summary>
        /// The document hash.
        /// </summary>
        [XmlElement("Hash")]
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// The hash control.
        /// </summary>
        [XmlElement("HashControl")]
        public string HashControl { get; set; } = string.Empty;

        /// <summary>
        /// The accounting period (optional).
        /// </summary>
        [XmlElement("Period")]
        public string? Period { get; set; }

        /// <summary>
        /// The movement date.
        /// </summary>
        [XmlElement("MovementDate")]
        public string MovementDate { get; set; } = string.Empty;

        /// <summary>
        /// The type of movement.
        /// </summary>
        [XmlElement("MovementType")]
        public MovementType MovementType { get; set; }

        /// <summary>
        /// The system entry date.
        /// </summary>
        [XmlElement("SystemEntryDate")]
        public string SystemEntryDate { get; set; } = string.Empty;

        /// <summary>
        /// The transaction identifier (optional).
        /// </summary>
        [XmlElement("TransactionID")]
        public string? TransactionID { get; set; }

        /// <summary>
        /// The customer identifier (used when SupplierID is not specified).
        /// </summary>
        [XmlElement("CustomerID")]
        public string? CustomerID { get; set; }

        /// <summary>
        /// The supplier identifier (used when CustomerID is not specified).
        /// </summary>
        [XmlElement("SupplierID")]
        public string? SupplierID { get; set; }

        /// <summary>
        /// The source identifier.
        /// </summary>
        [XmlElement("SourceID")]
        public string SourceID { get; set; } = string.Empty;

        /// <summary>
        /// The EAC code (optional).
        /// </summary>
        [XmlElement("EACCode")]
        public string? EACCode { get; set; }

        /// <summary>
        /// Movement comments (optional).
        /// </summary>
        [XmlElement("MovementComments")]
        public string? MovementComments { get; set; }

        /// <summary>
        /// Ship to information (optional).
        /// </summary>
        [XmlElement("ShipTo")]
        public string? ShipTo { get; set; }

        /// <summary>
        /// Ship from information (optional).
        /// </summary>
        [XmlElement("ShipFrom")]
        public string? ShipFrom { get; set; }

        /// <summary>
        /// Movement end time (optional).
        /// </summary>
        [XmlElement("MovementEndTime")]
        public string? MovementEndTime { get; set; }

        /// <summary>
        /// Movement start time.
        /// </summary>
        [XmlElement("MovementStartTime")]
        public string MovementStartTime { get; set; } = string.Empty;

        /// <summary>
        /// AT document code identifier (optional).
        /// </summary>
        [XmlElement("ATDocCodeID")]
        public string? ATDocCodeID { get; set; }

        /// <summary>
        /// Collection of movement lines (unbounded).
        /// </summary>
        [XmlElement("Line")]
        public List<MovementLine> Lines { get; set; } = new List<MovementLine>();

        /// <summary>
        /// Document totals.
        /// </summary>
        [XmlElement("DocumentTotals")]
        public MovementDocumentTotals DocumentTotals { get; set; } = new MovementDocumentTotals();

        /// <summary>
        /// Indicates whether CustomerID is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool CustomerIDSpecified => !string.IsNullOrEmpty(CustomerID);

        /// <summary>
        /// Indicates whether SupplierID is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool SupplierIDSpecified => !string.IsNullOrEmpty(SupplierID);
    }

    /// <summary>
    /// Represents the current status of a movement document.
    /// </summary>
    public class MovementDocumentStatus
    {
        /// <summary>
        /// The status of the movement.
        /// </summary>
        [XmlElement("MovementStatus")]
        public MovementStatus MovementStatus { get; set; }

        /// <summary>
        /// The status date.
        /// </summary>
        [XmlElement("MovementStatusDate")]
        public string MovementStatusDate { get; set; } = string.Empty;

        /// <summary>
        /// The reason for the status (optional).
        /// </summary>
        [XmlElement("Reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// The source identifier.
        /// </summary>
        [XmlElement("SourceID")]
        public string SourceID { get; set; } = string.Empty;

        /// <summary>
        /// The source billing information.
        /// </summary>
        [XmlElement("SourceBilling")]
        public SourceBilling SourceBilling { get; set; } = new SourceBilling();
    }

    /// <summary>
    /// Represents a movement line with product and tax information.
    /// </summary>
    public class MovementLine
    {
        /// <summary>
        /// The line number.
        /// </summary>
        [XmlElement("LineNumber")]
        public int LineNumber { get; set; }

        /// <summary>
        /// Collection of order references (optional, unbounded).
        /// </summary>
        [XmlElement("OrderReferences")]
        public List<OrderReferences> OrderReferences { get; set; } = new List<OrderReferences>();

        /// <summary>
        /// The product code.
        /// </summary>
        [XmlElement("ProductCode")]
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// The product description.
        /// </summary>
        [XmlElement("ProductDescription")]
        public string ProductDescription { get; set; } = string.Empty;

        /// <summary>
        /// The quantity.
        /// </summary>
        [XmlElement("Quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The unit of measure.
        /// </summary>
        [XmlElement("UnitOfMeasure")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        /// <summary>
        /// The unit price.
        /// </summary>
        [XmlElement("UnitPrice")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// The line description.
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Product serial number information (optional).
        /// </summary>
        [XmlElement("ProductSerialNumber")]
        public ProductSerialNumber? ProductSerialNumber { get; set; }

        /// <summary>
        /// The debit amount (used when CreditAmount is not specified).
        /// </summary>
        [XmlElement("DebitAmount")]
        public decimal? DebitAmount { get; set; }

        /// <summary>
        /// The credit amount (used when DebitAmount is not specified).
        /// </summary>
        [XmlElement("CreditAmount")]
        public decimal? CreditAmount { get; set; }

        /// <summary>
        /// Tax information (optional).
        /// </summary>
        [XmlElement("Tax")]
        public MovementTax? Tax { get; set; }

        /// <summary>
        /// Tax exemption reason (optional).
        /// </summary>
        [XmlElement("TaxExemptionReason")]
        public string? TaxExemptionReason { get; set; }

        /// <summary>
        /// Tax exemption code (optional).
        /// </summary>
        [XmlElement("TaxExemptionCode")]
        public string? TaxExemptionCode { get; set; }

        /// <summary>
        /// Settlement amount (optional).
        /// </summary>
        [XmlElement("SettlementAmount")]
        public decimal? SettlementAmount { get; set; }

        /// <summary>
        /// Customs information (optional).
        /// </summary>
        [XmlElement("CustomsInformation")]
        public CustomsInformation? CustomsInformation { get; set; }

        /// <summary>
        /// Indicates whether DebitAmount is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool DebitAmountSpecified => DebitAmount.HasValue;

        /// <summary>
        /// Indicates whether CreditAmount is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool CreditAmountSpecified => CreditAmount.HasValue;
    }

    /// <summary>
    /// Represents document totals for movement documents.
    /// </summary>
    public class MovementDocumentTotals
    {
        /// <summary>
        /// The tax payable amount.
        /// </summary>
        [XmlElement("TaxPayable")]
        public decimal TaxPayable { get; set; }

        /// <summary>
        /// The net total amount.
        /// </summary>
        [XmlElement("NetTotal")]
        public decimal NetTotal { get; set; }

        /// <summary>
        /// The gross total amount.
        /// </summary>
        [XmlElement("GrossTotal")]
        public decimal GrossTotal { get; set; }

        /// <summary>
        /// Currency information (optional).
        /// </summary>
        [XmlElement("Currency")]
        public Currency? Currency { get; set; }
    }

    // Placeholder classes for movement-specific types
    public class MovementTax { /* TODO: Implement */ }
} 