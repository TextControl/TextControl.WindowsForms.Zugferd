using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace TXTextControl.DocumentServer.PDF.Zugferd {
	public class Invoice {
		public string Number { get; set; }
		public DateTime InvoiceDate { get; set; }
		public CurrencyCode Currency { get; set; }
		public InvoiceType Type { get; set; }
		public TradeParty Buyer { get; set; }
		public TradeParty Seller { get; set; }
		public Profile Profile { get; set; }
		public float TotalAmount { get; set; }
		public List<LineItem> LineItems = new List<LineItem>();
		public DateTime DueDate { get; set; } = DateTime.Now.AddDays(30);
		public float ApplicableTax { get; set; } = 0.19f;

		public Invoice(string invoiceNumber, DateTime invoiceDate, CurrencyCode currency) {
			Number = invoiceNumber;
			InvoiceDate = invoiceDate;
			Currency = currency;
		}

		public string CreateXml() {

			MemoryStream xml = new MemoryStream();
			XmlTextWriter xw = new XmlTextWriter(xml, Encoding.UTF8);
			xw.Formatting = Formatting.Indented;

			xw.WriteStartDocument();

			// write the required header information
			RenderDocumentHeader(xw);
			RenderExchangeDocument(xw);

			// supply chain started
			xw.WriteStartElement("rsm:SupplyChainTradeTransaction");

			// add all line items
			AddTradeLineItems(xw);

			// trade agreement
			xw.WriteStartElement("ram:ApplicableHeaderTradeAgreement");

			// render trade parties
			RenderUserDetails(xw, "ram:SellerTradeParty", Seller);
			RenderUserDetails(xw, "ram:BuyerTradeParty", Buyer);

			xw.WriteEndElement();

			// trade delivery
			RenderTradeDelivery(xw);

			// trade settlement
			RenderTradeSettlement(xw);

			// monetary summary
			RenderMonetarySummation(xw);

			// close document
			xw.WriteEndElement();
			xw.WriteEndDocument();

			xw.Flush();
			xml.Position = 0;

			return Encoding.UTF8.GetString(xml.ToArray());
		}

		private void AddTradeLineItems(XmlTextWriter writer) {

			foreach (LineItem product in this.LineItems) {
				writer.WriteStartElement("ram:IncludedSupplyChainTradeLineItem");
				writer.WriteStartElement("ram:AssociatedDocumentLineDocument");
				RenderOptionalElement(writer, "ram:LineID", "1");
				writer.WriteEndElement();

				writer.WriteStartElement("ram:SpecifiedTradeProduct");
				RenderOptionalElement(writer, "ram:Name", product.Name);
				writer.WriteEndElement();

				if (Profile != Profile.Basic) {
					writer.WriteStartElement("ram:SpecifiedLineTradeAgreement");
					writer.WriteStartElement("ram:NetPriceProductTradePrice");
					RenderOptionalElement(writer, "ram:ChargeAmount", product.Price.ToString());
					writer.WriteEndElement();
					writer.WriteEndElement();
				}

				writer.WriteStartElement("ram:SpecifiedLineTradeDelivery");
				RenderAttribute(writer, "ram:BilledQuantity", "unitCode", product.UnitCode.ToString(), product.Quantity.ToString());
				writer.WriteEndElement();

				writer.WriteStartElement("ram:SpecifiedLineTradeSettlement");

				writer.WriteStartElement("ram:ApplicableTradeTax");
				RenderOptionalElement(writer, "ram:TypeCode", "VAT");
				RenderOptionalElement(writer, "ram:CategoryCode", "S");
				RenderOptionalElement(writer, "ram:RateApplicablePercent", "19");
				writer.WriteEndElement();

				writer.WriteStartElement("ram:SpecifiedTradeSettlementLineMonetarySummation");
				RenderOptionalElement(writer, "ram:LineTotalAmount", FormatValue(product.Price * product.Quantity));
				writer.WriteEndElement();
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}

		private void RenderAttribute(XmlTextWriter writer, string tagName, string attributeName, string attributeValue, string nodeValue) {
			writer.WriteStartElement(tagName);
			writer.WriteAttributeString(attributeName, attributeValue);
			writer.WriteValue(nodeValue);
			writer.WriteEndElement();
		}

		private void RenderOptionalElement(XmlTextWriter writer, string tagName, string value) {
			if (!String.IsNullOrEmpty(value)) {
				writer.WriteElementString(tagName, value);
			}
		}

		private void RenderOptionalAmount(XmlTextWriter writer, string tagName, float value, int decimals = 2, bool renderCurrency = false) {
			if (value != float.MinValue) {
				writer.WriteStartElement(tagName);
				
				if (renderCurrency == true)
					writer.WriteAttributeString("currencyID", Currency.ToString("g"));

				writer.WriteValue(FormatValue(value, decimals));
				writer.WriteEndElement();
			}
		}

		private void RenderUserDetails(XmlTextWriter writer, string customerTag, TradeParty user) {
			if (user != null) {
				writer.WriteStartElement(customerTag);

				if (!String.IsNullOrEmpty(user.Name)) {
					writer.WriteElementString("ram:Name", user.Name);
				}

				writer.WriteStartElement("ram:PostalTradeAddress");
				writer.WriteElementString("ram:PostcodeCode", user.Postcode);
				writer.WriteElementString("ram:LineOne", string.IsNullOrEmpty(user.ContactName) ? user.Street : user.ContactName);

				if (!string.IsNullOrEmpty(user.ContactName))
					writer.WriteElementString("ram:LineTwo", user.Street);

				writer.WriteElementString("ram:CityName", user.City);
				writer.WriteElementString("ram:CountryID", user.Country.ToString("g"));
				writer.WriteEndElement();

				foreach (TaxID taxID in user.SpecifiedTaxRegistrations) {
					writer.WriteStartElement("ram:SpecifiedTaxRegistration");
					RenderAttribute(writer, "ram:ID", "schemeID", taxID.Scheme.ToString(), taxID.ID);
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
			}
		}

		private void RenderTradeDelivery(XmlTextWriter writer) {
			writer.WriteStartElement("ram:ApplicableHeaderTradeDelivery");
			writer.WriteStartElement("ram:ActualDeliverySupplyChainEvent");
			writer.WriteStartElement("ram:OccurrenceDateTime");

			RenderAttribute(writer, "udt:DateTimeString", "format", "102", ConvertDateFormat(DateTime.Now));

			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private void RenderTradeSettlement(XmlTextWriter writer) {
			writer.WriteStartElement("ram:ApplicableHeaderTradeSettlement");

			writer.WriteElementString("ram:InvoiceCurrencyCode", Currency.ToString("g"));

			writer.WriteStartElement("ram:ApplicableTradeTax");
			RenderOptionalAmount(writer, "ram:CalculatedAmount", (float)(TotalAmount * ApplicableTax));
			writer.WriteElementString("ram:TypeCode", "VAT");
			RenderOptionalAmount(writer, "ram:BasisAmount", TotalAmount);
			writer.WriteElementString("ram:CategoryCode", "S");
			writer.WriteElementString("ram:RateApplicablePercent", (ApplicableTax * 100).ToString());
			writer.WriteEndElement();

			writer.WriteStartElement("ram:SpecifiedTradePaymentTerms");
			writer.WriteStartElement("ram:DueDateDateTime");
			RenderAttribute(writer, "udt:DateTimeString", "format", "102", ConvertDateFormat(DueDate));
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private void RenderMonetarySummation(XmlTextWriter writer) {
			writer.WriteStartElement("ram:SpecifiedTradeSettlementHeaderMonetarySummation");

			RenderOptionalAmount(writer, "ram:LineTotalAmount", TotalAmount);
			RenderOptionalAmount(writer, "ram:ChargeTotalAmount", 0);
			RenderOptionalAmount(writer, "ram:AllowanceTotalAmount", 0);
			RenderOptionalAmount(writer, "ram:TaxBasisTotalAmount", TotalAmount);
			RenderOptionalAmount(writer, "ram:TaxTotalAmount", (float)(TotalAmount * ApplicableTax), 2, true);
			RenderOptionalAmount(writer, "ram:GrandTotalAmount", TotalAmount + (float)(TotalAmount * ApplicableTax));
			RenderOptionalAmount(writer, "ram:DuePayableAmount", TotalAmount + (float)(TotalAmount * ApplicableTax));

			writer.WriteEndElement();
		}

		private void RenderDocumentHeader(XmlTextWriter writer) {
			writer.WriteStartElement("rsm:CrossIndustryInvoice");
			writer.WriteAttributeString("xmlns", "a", null, "urn:un:unece:uncefact:data:standard:QualifiedDataType:100");
			writer.WriteAttributeString("xmlns", "rsm", null, "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100");
			writer.WriteAttributeString("xmlns", "qdt", null, "urn:un:unece:uncefact:data:standard:QualifiedDataType:10");
			writer.WriteAttributeString("xmlns", "ram", null, "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100");
			writer.WriteAttributeString("xmlns", "xs", null, "http://www.w3.org/2001/XMLSchema");
			writer.WriteAttributeString("xmlns", "udt", null, "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100");

			writer.WriteStartElement("rsm:ExchangedDocumentContext");
			writer.WriteStartElement("ram:GuidelineSpecifiedDocumentContextParameter");
			writer.WriteElementString("ram:ID", "urn:cen.eu:en16931:2017#compliant#urn:zugferd.de:2p0:" + Profile.ToString().ToLower());
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private void RenderExchangeDocument(XmlTextWriter writer) {
			writer.WriteStartElement("rsm:ExchangedDocument");
			writer.WriteElementString("ram:ID", Number);
			writer.WriteElementString("ram:TypeCode", String.Format("{0}", (int)Type));

			writer.WriteStartElement("ram:IssueDateTime");
			writer.WriteStartElement("udt:DateTimeString");
			writer.WriteAttributeString("format", "102");
			writer.WriteValue(ConvertDateFormat(InvoiceDate, "102"));
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private string ConvertDateFormat(DateTime date, String format = "102") {
			if (format.Equals("102")) {
				return date.ToString("yyyyMMdd");
			}
			else {
				return date.ToString("yyyy-MM-ddTHH:mm:ss");
			}
		}

		private string FormatValue(float value, int numDecimals = 2) {
			string formatString = "0.";

			for (int i = 0; i < numDecimals; i++) {
				formatString += "0";
			}

			return value.ToString(formatString).Replace(",", ".");
		}

	}
}