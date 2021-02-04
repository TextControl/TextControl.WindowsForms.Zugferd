namespace TXTextControl.DocumentServer.PDF.Zugferd {

    public class LineItem
    {
        public string ProductID { get; set; }

        public string Name { get; set; }

        public float Price { get; set; }

        public float Quantity { get; set; }

        public float Total { get; set; }
        public QuantityCodes UnitCode { get; set; }
   }
}
