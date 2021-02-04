using System.Collections.Generic;

namespace TXTextControl.DocumentServer.PDF.Zugferd {

    public class TradeParty
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
        public CountryCode Country { get; set; }
        public string Street { get; set; }
        public List<TaxID> SpecifiedTaxRegistrations { get; set; } = new List<TaxID>();
   
   }

   public enum TaxScheme {
      FC,
      VA
	}

   public class TaxID {
      public TaxScheme Scheme { get; set; }
      public string ID { get; set; }
   }

}
