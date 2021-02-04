using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TXTextControl.DocumentServer.PDF.Zugferd {
	public class MetaData {
		public static string GetMetaData() {
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "TXTextControl.DocumentServer.PDF.Zugferd.zugferd-metadata.xml";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))

			using (StreamReader reader = new StreamReader(stream)) {
				return reader.ReadToEnd();
			}
		}
	}
}
