using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TXTextControl.DocumentServer.PDF.Zugferd;

namespace tx_zugferd {
	public partial class Form1 : Form {
		public Form1() {
			InitializeComponent();
		}

      private Invoice CreateSampleInvoice() {
         // new zugferd invoice
         Invoice invoice = new Invoice("A12345", DateTime.Now, CurrencyCode.USD);

         invoice.Type = InvoiceType.Invoice;
         invoice.Profile = Profile.Comfort;

         // buyer
         invoice.Buyer = new TradeParty {
            ID = "TX_1",
            Name = "Text Control GmbH",
            ContactName = "Peter Paulsen",
            City = "Bremen",
            Postcode = "28217",
            Country = CountryCode.DE,
            Street = "Überseetor 18"
         };

         // seller
         invoice.Seller = new TradeParty {
            ID = "TX_2",
            Name = "Text Control, LLC",
            ContactName = "Jack Jackson",
            City = "Charlotte, NC",
            Postcode = "28210",
            Country = CountryCode.US,
            Street = "6926 Shannon Willow Rd, Suite 400",
         };

         // add tax id's
         invoice.Seller.SpecifiedTaxRegistrations.Add(
            new TaxID() { ID = "US12367623", Scheme = TaxScheme.VA });

         // add products
         List<LineItem> lineItems = new List<LineItem>();

         lineItems.Add(new LineItem() {
            Price = 200,
            ProductID = "A123",
            Name = "Product A",
            Quantity = 5,
            Total = 1000,
            UnitCode = QuantityCodes.C62
         });

         // add line items to invoice
         foreach (LineItem item in lineItems)
            invoice.LineItems.Add(item);

         // set the total amount
         invoice.TotalAmount = 1000;

         return invoice;
      }

		private void button1_Click(object sender, EventArgs e) {

         TXTextControl.DocumentServer.MailMerge mm = new TXTextControl.DocumentServer.MailMerge();
         mm.TextComponent = textControl1;

         textControl1.Load("invoice.tx", TXTextControl.StreamType.InternalUnicodeFormat);

         Invoice invoice = CreateSampleInvoice();

         // merge data into template
         mm.MergeJsonData(JsonConvert.SerializeObject(invoice));

         // create the XML
         string xmlZugferd = invoice.CreateXml();
         
         // get the required meta data
         string metaData = MetaData.GetMetaData();

			TXTextControl.SaveSettings saveSettings = new TXTextControl.SaveSettings();

         // create a new embedded file
         var zugferdInvoice = new TXTextControl.EmbeddedFile(
            "ZUGFeRD-invoice.xml",
            Encoding.UTF8.GetBytes(xmlZugferd),
            metaData);

         zugferdInvoice.Description = "ZUGFeRD-invoice";
         zugferdInvoice.Relationship = "Alternative";
         zugferdInvoice.MIMEType = "application/xml";
         zugferdInvoice.LastModificationDate = DateTime.Now;

         // set the embedded files
         saveSettings.EmbeddedFiles = new TXTextControl.EmbeddedFile[] {
            new TXTextControl.EmbeddedFile(
               "ZUGFeRD-invoice.xml",
               Encoding.UTF8.GetBytes(xmlZugferd),
               metaData) };

         // export the PDF
			textControl1.Save("test.pdf", TXTextControl.StreamType.AdobePDFA, saveSettings);
		}
	}
}
