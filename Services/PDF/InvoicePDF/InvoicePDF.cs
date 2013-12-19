using System;
using System.Globalization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using System.Diagnostics;
using XODB.Module.BusinessObjects;
using System.Data.Entity;
using MigraDoc.Extensions.Html;

namespace EXPEDIT.Share.Services.PDF
{
    public static class InvoicePDF
    {

        public static Document CreatePDF(this Invoice invoice)
        {
            Document document;
            //Address box
            TextFrame addressFrame;
            TextFrame companyFrame;
            //Invoice Items
            Table table;
            // Create a new MigraDoc document
            document = new Document();
            document.Info.Title = "Invoice";
            document.Info.Subject = "Mining Appstore Invoice";
            document.Info.Author = "MINING APPSTORE";

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

            // Put a logo in the header
            Image image = section.Headers.Primary.AddImage("../../store.jpg");
            image.Height = "2cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Right;
            image.WrapFormat.Style = WrapStyle.Through;

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
            paragraph = companyFrame.AddParagraph(
                "EXPEDIT SOLUTIONS PTY LTD - MiningAppstore\r\n"+
                "ABN 93152456374\r\n"+
                "3 Fincastle Street, Moorooka, Brisbane\r\n" +
                "QLD, 4105 Australia\r\n\r\n" +
                "P: +61733460727\r\n" +
                "E: accounts@miningappstore.com\r\n" +
                "U: http://miningappstore.com"
                );            
            paragraph.Format.Font.Name = "Arial";
            paragraph.Format.Font.Size = 7;
            paragraph.Format.SpaceAfter = 3;

            // Add the print date field
            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "6cm";
            paragraph.Style = "Reference";
            paragraph.AddFormattedText("INVOICE", TextFormat.Bold);
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
