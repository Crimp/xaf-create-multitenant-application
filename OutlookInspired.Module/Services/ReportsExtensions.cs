﻿using System.Drawing;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.Pdf;
using DevExpress.XtraReports.UI;
using OutlookInspired.Module.BusinessObjects;
using OutlookInspired.Module.Resources.Reports;

namespace OutlookInspired.Module.Services;
static class ReportsExtensions{
    public const string RevenueReport = "Revenue Report";
    public const string RevenueAnalysis = "Revenue Analysis";
    public const string Contacts = "Contacts";
    public const string LocationsReport = "Locations";
    public const string SalesSummaryReport = "Sales Summary Report";
    public const string CustomerProfile = "Profile";
    public const string OrdersReport = "Orders";
    public const string ProductProfile = "Profile";
    public const string Sales = "Sales";
    public const string TopSalesPerson = "Top Sales Person";
    public static PredefinedReportsUpdater AddOrderReports(this PredefinedReportsUpdater predefinedReportsUpdater){
        predefinedReportsUpdater.AddPredefinedReport<FedExGroundLabel>(nameof(FedExGroundLabel), typeof(Order));
        predefinedReportsUpdater.AddPredefinedReport<SalesRevenueReport>(RevenueReport, typeof(Order));
        predefinedReportsUpdater.AddPredefinedReport<SalesRevenueAnalysisReport>(RevenueAnalysis, typeof(Order));
        return predefinedReportsUpdater;
    }

    public static PredefinedReportsUpdater AddCustomerReports(this PredefinedReportsUpdater predefinedReportsUpdater){
        predefinedReportsUpdater.AddPredefinedReport<CustomerContactsDirectory>(Contacts, typeof(Customer));
        predefinedReportsUpdater.AddPredefinedReport<CustomerLocationsDirectory>(LocationsReport, typeof(Customer));
        predefinedReportsUpdater.AddPredefinedReport<CustomerSalesSummaryReport>(SalesSummaryReport, typeof(Customer));
        predefinedReportsUpdater.AddPredefinedReport<CustomerProfile>(CustomerProfile, typeof(Customer));
        return predefinedReportsUpdater;
    }
    public static PredefinedReportsUpdater AddProductReports(this PredefinedReportsUpdater predefinedReportsUpdater){
        predefinedReportsUpdater.AddPredefinedReport<ProductOrders>(OrdersReport, typeof(Product));
        predefinedReportsUpdater.AddPredefinedReport<ProductProfile>(ProductProfile, typeof(Product));
        predefinedReportsUpdater.AddPredefinedReport<ProductSalesSummary>(Sales, typeof(Product));
        predefinedReportsUpdater.AddPredefinedReport<ProductTopSalesperson>(TopSalesPerson, typeof(Product));
        return predefinedReportsUpdater;
    }
    
    public static byte[] ToPdf(this XtraReport report,string waterMarkText=null){
        using var memoryStream = new MemoryStream();
        report.ExportToPdf(memoryStream);
        var bytes = memoryStream.ToArray();
        return waterMarkText != null ? bytes.AddWaterMark(waterMarkText) : bytes;
    }

    public static string WatermarkText(this Order order) 
        => order.ShipmentStatus switch{
            ShipmentStatus.Received => "Shipment Received",
            ShipmentStatus.Transit => "Shipment in Transit",
            _ => "Awaiting shipment"
        };

    public static void AddWatermark(this PdfDocumentProcessor processor, string watermark){
        var pages = processor.Document.Pages;
        using var font = new Font("Segoe UI", 48, FontStyle.Regular);
        foreach (var t in pages){
            using var graphics = processor.CreateGraphics();
            var pageLayout = new RectangleF(
                -(float)t.CropBox.Width * 0.35f,
                (float)t.CropBox.Height * 0.1f,
                (float)t.CropBox.Width * 1.25f,
                (float)t.CropBox.Height);
                        
            var angle = Math.Asin(pageLayout.Width / (double)pageLayout.Height) * 180.0 / Math.PI;
            graphics.TranslateTransform(-pageLayout.X, -pageLayout.Y);
            graphics.RotateTransform((float)angle);

            using(var textBrush = new SolidBrush(Color.FromArgb(100, Color.Red)))
                graphics.DrawString(watermark, font, textBrush, new PointF(50, 50));
            graphics.AddToPageForeground(t);
        }
    }

    static byte[] AddWaterMark(this byte[] bytes,string text){
        using var processor = new PdfDocumentProcessor();
        using var memoryStream = new MemoryStream(bytes);
        processor.LoadDocument(memoryStream);
        processor.AddWatermark(text);
        using var stream = new MemoryStream();
        processor.SaveDocument(stream);
        return stream.ToArray();
    }
}