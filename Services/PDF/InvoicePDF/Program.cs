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
            //try
            //{
                var d = new XODBC(null, null, true);
                var i = (from o in d.Invoices where o.InvoiceID == new Guid("42452AD5-207C-4D14-9A17-A299004A5AEE") select o).Single();
                

                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.None);
                pdfRenderer.Document = i.CreatePDF();
                pdfRenderer.RenderDocument();

                string filename = "Invoice-" + Guid.NewGuid().ToString("N").ToUpper() + ".pdf";
                pdfRenderer.Save(filename);
                Process.Start(filename);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    Console.ReadLine();
            //}
        }
    }
}
