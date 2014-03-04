using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing.Printing;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
//using System.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using System.Diagnostics;
using MigraDoc.Rendering;
using System.IO;
using MigraDoc.Extensions.Html;
using MigraDoc.Extensions.Markdown;
using System.Globalization;
using MigraDoc.DocumentObjectModel.Tables;
using NKD.Module.BusinessObjects;
using System.Data.Entity;

namespace EXPEDIT.Share.Helpers
{
    public static class PdfHelper
    {
       
        public static void Html2Pdf(this string html, ref Stream stream, params string[] replaceTags)
        {
              DateTime now = DateTime.Now;
              string filename = "MixMigraDocAndPdfSharp.pdf";
              filename = Guid.NewGuid().ToString("D").ToUpper() + ".pdf";
              PdfDocument document = new PdfDocument();
              document.Info.Title = "PDFsharp XGraphic Sample";
              document.Info.Author = "Stefan Lange";
              document.Info.Subject = "Created with code snippets that show the use of graphical functions";
              document.Info.Keywords = "PDFsharp, XGraphics";
              SamplePage1(document);
              //SamplePage2(document);
              Debug.WriteLine("seconds=" + (DateTime.Now - now).TotalSeconds.ToString());
              // Save the document...
              document.Save(stream, false);


        }
        static void SamplePage1(PdfDocument document)
        {
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            // HACK²
            gfx.MUH = PdfFontEncoding.Unicode;
            gfx.MFEH = PdfFontEmbedding.Default;
            XFont font = new XFont("Verdana", 13, XFontStyle.Bold);
            gfx.DrawString("The following paragraph was rendered using MigraDoc:", font, XBrushes.Black,
              new XRect(100, 100, page.Width - 200, 300), XStringFormats.Center);
            // You always need a MigraDoc document for rendering.
            Document doc = new Document();
            Section sec = doc.AddSection();
            var markdown = @"
    # This is a heading

    This is some **bold** ass text with a [link](http://www.google.com).

    - List Item 1
    - List Item 2
    - List Item 3

    Pretty cool huh?
";

            sec.AddMarkdown(markdown);
            var html = @"
    <h1>This is a heading</h1>

    <p>This is some **bold** ass text with a <a href='http://www.google.com'>link</a>.<p>

    <ul>
        <li>List Item 1</li>
        <li>List Item 2</li>
        <li>List Item 3</li>
    </ul>

    <p>Pretty cool huh?</p>
";

            sec.AddHtml(html);
            // Add a single paragraph with some text and format information.
            Paragraph para = sec.AddParagraph();
            para.Format.Alignment = ParagraphAlignment.Justify;
            para.Format.Font.Name = "Times New Roman";
            para.Format.Font.Size = 12;
            para.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.DarkGray;
            para.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.DarkGray;
            para.AddText("Duisism odigna acipsum delesenisl ");
            para.AddFormattedText("ullum in velenit", TextFormat.Bold);
            para.AddText(" ipit iurero dolum zzriliquisis nit wis dolore vel et nonsequipit, velendigna " +
              "auguercilit lor se dipisl duismod tatem zzrit at laore magna feummod oloborting ea con vel " +
              "essit augiati onsequat luptat nos diatum vel ullum illummy nonsent nit ipis et nonsequis " +
              "niation utpat. Odolobor augait et non etueril landre min ut ulla feugiam commodo lortie ex " +
              "essent augait el ing eumsan hendre feugait prat augiatem amconul laoreet. ≤≥≈≠");
            para.Format.Borders.Distance = "5pt";
            para.Format.Borders.Color = Colors.Gold;
            // Create a renderer and prepare (=layout) the document
            MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(doc);
            docRenderer.PrepareDocument();
            // Render the paragraph. You can render tables or shapes the same way.
            docRenderer.RenderObject(gfx, XUnit.FromCentimeter(5), XUnit.FromCentimeter(10), "12cm", para);
        }
        static void SamplePage2(PdfDocument document)
        {
            //PdfPage page = document.AddPage();
            //XGraphics gfx = XGraphics.FromPdfPage(page);
            //// HACK²
            //gfx.MUH = PdfFontEncoding.Unicode;
            //gfx.MFEH = PdfFontEmbedding.Default;
            //// Create document from HalloMigraDoc sample
            //Document doc = HelloMigraDoc.Documents.CreateDocument();
            //// Create a renderer and prepare (=layout) the document
            //MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(doc);
            //docRenderer.PrepareDocument();
            //// For clarity we use point as unit of measure in this sample.
            //// A4 is the standard letter size in Germany (21cm x 29.7cm).
            //XRect A4Rect = new XRect(0, 0, A4Width, A4Height);
            //int pageCount = docRenderer.FormattedDocument.PageCount;
            //for (int idx = 0; idx < pageCount; idx++)
            //{
            //    XRect rect = GetRect(idx);
            //    // Use BeginContainer / EndContainer for simplicity only. You can naturaly use you own transformations.
            //    XGraphicsContainer container = gfx.BeginContainer(rect, A4Rect, XGraphicsUnit.Point);
            //    // Draw page border for better visual representation
            //    gfx.DrawRectangle(XPens.LightGray, A4Rect);
            //    // Render the page. Note that page numbers start with 1.
            //    docRenderer.RenderPage(gfx, idx + 1);
            //    // Note: The outline and the hyperlinks (table of content) does not work in the produced PDF document.
            //    // Pop the previous graphical state
            //    gfx.EndContainer(container);
            //}
        }
        static XRect GetRect(int index)
        {
            XRect rect = new XRect(0, 0, A4Width / 3 * 0.9, A4Height / 3 * 0.9);
            rect.X = (index % 3) * A4Width / 3 + A4Width * 0.05 / 3;
            rect.Y = (index / 3) * A4Height / 3 + A4Height * 0.05 / 3;
            return rect;
        }
        static double A4Width = XUnit.FromCentimeter(21).Point;
        static double A4Height = XUnit.FromCentimeter(29.7).Point;

        public static void GetPDF(this Invoice invoice, ref Stream stream, string logoFilename = null) {
           //PdfDocument document = new PdfDocument();
            var pdfRenderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.Always);
            pdfRenderer.Document = invoice.GetInvoiceDocument(logoFilename);
            pdfRenderer.RenderDocument();
            pdfRenderer.Save(stream, false);
        }

        public static Document GetInvoiceDocument(this Invoice invoice, string logoFileName = null)
        {
            Document document;
            //Address box
            TextFrame addressFrame;
            TextFrame companyFrame;
            //Invoice Items
            Table table;
            // Create a new MigraDoc document
            document = new Document();
            document.Info.Title = "Tax Invoice";
            document.Info.Subject = "Tax Invoice";
            document.Info.Author = ConstantsHelper.APP_OWNER;

            document.DefaultPageSetup.TopMargin = "4cm";

            ///Define Styles
            // RGB colors
            Color TableBorder = new Color(231, 231, 231);
            Color TableHeader = new Color(250, 176, 26);
            Color TableGray = new Color(242, 242, 242);

            // Get the predefined style Normal.
            Style style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Arial";
            style.Font.Color = new Color(65, 65, 66);

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);


            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called Table based on style Normal
            style = document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Arial";
            style.Font.Size = 9;

            // Create a new style called Reference based on style Normal
            style = document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);


            ///Create Page
            // Each MigraDoc document needs at least one section.
            Section section = document.AddSection();

            if (logoFileName != null)
            {
                // Put a logo in the header
                Image image = section.Headers.Primary.AddImage(logoFileName);
                image.Height = "2cm";
                image.LockAspectRatio = true;
                image.RelativeVertical = RelativeVertical.Line;
                image.RelativeHorizontal = RelativeHorizontal.Margin;
                image.Top = ShapePosition.Top;
                image.Left = ShapePosition.Right;
                image.WrapFormat.Style = WrapStyle.Through;
            }

            // Create footer
            Paragraph paragraph = section.Footers.Primary.AddParagraph();
            paragraph.AddText("*");
            paragraph.Format.Font.Size = 9;
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            // Create the text frame for the address
            addressFrame = section.AddTextFrame();
            addressFrame.Height = "3.0cm";
            addressFrame.Width = "7.0cm";
            addressFrame.Left = ShapePosition.Left;
            addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            addressFrame.Top = "5.0cm";
            addressFrame.RelativeVertical = RelativeVertical.Page;

            // Put sender in company frame
            companyFrame = section.Headers.Primary.AddTextFrame();
            companyFrame.Height = "3.0cm";
            companyFrame.Width = "7.0cm";
            companyFrame.Left = ShapePosition.Left;
            companyFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            companyFrame.Top = "1.0cm";
            companyFrame.RelativeVertical = RelativeVertical.Page;
            paragraph = companyFrame.AddParagraph(ConstantsHelper.ADDRESS_APP_OWNER);
            paragraph.Format.Font.Name = "Arial";
            paragraph.Format.Font.Size = 7;
            paragraph.Format.SpaceAfter = 3;

            // Add the print date field
            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "6cm";
            paragraph.Style = "Reference";
            paragraph.AddFormattedText("TAX INVOICE", TextFormat.Bold);
            paragraph.AddTab();
            paragraph.AddText(string.Format("{0:dd MMMM yyyy}", invoice.Dated));

            // Create the item table
            table = section.AddTable();
            table.Style = "Table";
            table.Borders.Color = TableBorder;
            table.Borders.Width = 0.1;
            table.Borders.Left.Width = 0.1;
            table.Borders.Right.Width = 0.1;
            table.Rows.LeftIndent = 0;

            // Before you can add a row, you must define the columns
            Column column = table.AddColumn("1cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            column = table.AddColumn("3.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            column = table.AddColumn("2cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn("4cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            // Create the header of the table
            Row row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Format.Font.Name = "Arial";
            row.Shading.Color = TableHeader;
            row.Cells[0].AddParagraph("Item");
            row.Cells[0].Format.Font.Bold = false;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Bottom;
            row.Cells[0].MergeDown = 1;
            row.Cells[1].AddParagraph("Title");
            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[1].MergeRight = 3;
            row.Cells[5].AddParagraph("Subtotal");
            row.Cells[5].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[5].VerticalAlignment = VerticalAlignment.Bottom;
            row.Cells[5].MergeDown = 1;

            row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Shading.Color = TableHeader;
            row.Cells[1].AddParagraph("Quantity");
            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[2].AddParagraph("Unit Price");
            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[3].AddParagraph("Discount Amount");
            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[4].AddParagraph("Tax");
            row.Cells[4].Format.Alignment = ParagraphAlignment.Left;

            table.SetEdge(0, 0, 6, 2, Edge.Box, BorderStyle.Single, 0.75, Color.Empty);

            // Fill address in address text frame
            paragraph = addressFrame.AddParagraph();
            paragraph.AddText(string.Format("{0} {1}", invoice.CustomerContact.Firstname, invoice.CustomerContact.Surname));
            paragraph.AddLineBreak();
            var cAddress = invoice.CustomerAddress;
            if (cAddress != null)
                paragraph.AddText(
                    string.Format("{0}\r\n{1} {2}\r\n{3} {4}\r\n{5} {6}\r\n{7}",
                    cAddress.AddressName,
                    cAddress.Street,
                    cAddress.Extended,
                    cAddress.City,
                    cAddress.State,
                    cAddress.Postcode,
                    cAddress.Country,
                    cAddress.Email
                    ));

            // Iterate the invoice items
            foreach (var line in invoice.InvoiceLine)
            {
                // Each item fills two rows
                Row row1 = table.AddRow();
                Row row2 = table.AddRow();
                row1.TopPadding = 1.5;
                row1.Cells[0].Shading.Color = TableGray;
                row1.Cells[0].VerticalAlignment = VerticalAlignment.Center;
                row1.Cells[0].MergeDown = 1;
                row1.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                row1.Cells[1].MergeRight = 3;
                row1.Cells[5].Shading.Color = TableGray;
                row1.Cells[5].MergeDown = 1;

                row1.Cells[0].AddParagraph(string.Format("{0}", line.Sequence + 1));
                paragraph = row1.Cells[1].AddParagraph();
                paragraph.AddFormattedText(line.Description, TextFormat.Bold);
                paragraph.AddFormattedText(string.Format(" by {1}", line.Description, line.SupplyItem.SupplierModel.Company.CompanyName), TextFormat.NotBold);
                var cPrefix = line.Currency.PrefixCharacters;
                var cPostfix = line.Currency.PostfixCharacters;
                row2.Cells[1].AddParagraph(string.Format("{0:0.00}", line.Quantity));
                row2.Cells[2].AddParagraph(string.Format("{0}{1:0.00}{2}", cPrefix, (line.Quantity.HasValue && line.Quantity > 0) ? line.OriginalSubtotal / line.Quantity : 0, cPostfix));
                row2.Cells[3].AddParagraph(string.Format("{0}{1:0.00}{2}", cPrefix, line.DiscountAmount.HasValue ? line.DiscountAmount : 0m, cPostfix));
                row2.Cells[4].AddParagraph(string.Format("{0}{1:0.00}{2}", cPrefix, line.Tax, cPostfix));
                row1.Cells[5].AddParagraph(string.Format("{0}{1:0.00}{2}", cPrefix, line.OriginalSubtotal, cPostfix));
                row1.Cells[5].VerticalAlignment = VerticalAlignment.Bottom;
                table.SetEdge(0, table.Rows.Count - 2, 6, 2, Edge.Box, BorderStyle.Single, 0.75);

            }
            var currencyPrefix = invoice.Currency.PrefixCharacters;
            var currencyPostfix = invoice.Currency.PostfixCharacters;

            // Add an invisible row as a space line to the table
            row = table.AddRow();
            row.Borders.Visible = false;

            // Add the total price row
            row = table.AddRow();
            row.Cells[0].Borders.Visible = false;
            row.Cells[0].AddParagraph("Subtotal");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].MergeRight = 4;
            row.Cells[5].AddParagraph(string.Format("{0}{1:0.00}{2}", currencyPrefix, invoice.OriginalTotal, currencyPostfix));

            // Add the discount row
            row = table.AddRow();
            row.Cells[0].Borders.Visible = false;
            row.Cells[0].AddParagraph("Discounts");
            row.Cells[5].AddParagraph(string.Format("{0}{1:0.00}{2}", currencyPrefix, invoice.DiscountAmount ?? 0m, currencyPostfix));
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].MergeRight = 4;

            // Add the additional fee row
            row = table.AddRow();
            row.Cells[0].Borders.Visible = false;
            row.Cells[0].AddParagraph("Shipping and Handling");
            row.Cells[5].AddParagraph(string.Format("{0}{1:0.00}{2}", currencyPrefix, (invoice.DiscountAllFreight.HasValue && invoice.DiscountAllFreight.Value) ? 0m : invoice.FreightAmount ?? 0m, currencyPostfix));
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].MergeRight = 4;

            // Add the VAT row
            row = table.AddRow();
            row.Cells[0].Borders.Visible = false;
            row.Cells[0].AddParagraph("Tax");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].MergeRight = 4;
            row.Cells[5].AddParagraph(string.Format("{0}{1:0.00}{2}", currencyPrefix, (invoice.TaxAmount ?? 0m) + (invoice.FreightTax ?? 0m), currencyPostfix));

            // Add the total due row
            row = table.AddRow();
            row.Cells[0].AddParagraph("Total");
            row.Cells[0].Borders.Visible = false;
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Format.Font.Color = new Color(121, 61, 31);
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].MergeRight = 4;
            row.Cells[5].AddParagraph(string.Format("{0}{1:0.00}{2}", currencyPrefix, invoice.Total, currencyPostfix));

            // Set the borders of the specified cell range
            table.SetEdge(5, table.Rows.Count - 4, 1, 4, Edge.Box, BorderStyle.Single, 0.75);

            // Add the notes paragraph
            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.Format.Borders.Width = 0.75;
            paragraph.Format.Borders.Distance = 3;
            paragraph.Format.Borders.Color = TableBorder;
            paragraph.Format.Shading.Color = TableGray;

            paragraph.AddText("notes:");
            paragraph.Section.AddHtml("&nbsp;");

            return document;
        }


    }
}