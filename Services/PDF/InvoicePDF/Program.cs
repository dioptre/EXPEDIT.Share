using System;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using XODB.Module.BusinessObjects;
using System.Linq;

namespace InvoicePDF
{

    class Programm
    {
        static void Main()
        {
            try
            {
                var d = new XODBC(null, null, true);
                var i = (from o in d.Invoices where o.InvoiceID==new Guid("F716BCD8-0581-4BB6-8257-A299002ED613") select o).Single();
                

                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
                pdfRenderer.Document = i.CreatePDF();
                pdfRenderer.RenderDocument();

                string filename = "Invoice-" + Guid.NewGuid().ToString("N").ToUpper() + ".pdf";
                pdfRenderer.Save(filename);
                Process.Start(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
