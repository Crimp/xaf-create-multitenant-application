﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using OutlookInspired.Module.Attributes;
using OutlookInspired.Module.Features.CloneView;
using OutlookInspired.Module.Features.Maps;
using OutlookInspired.Module.Features.ViewFilter;
using OutlookInspired.Module.Services;
using OutlookInspired.Module.Services.Internal;


namespace OutlookInspired.Module.BusinessObjects{
    [ImageName("BO_Product")]
    [CloneView(CloneViewType.DetailView, BrochureDetailView)]
    [CloneView(CloneViewType.DetailView, CardViewDetailView)]
    [CloneView(CloneViewType.DetailView, MapsDetailView)]
    [Appearance("UnAvailable",AppearanceItemType.ViewItem, "!"+nameof(Available),TargetItems = "*",FontStyle = DevExpress.Drawing.DXFontStyle.Strikeout)]
    [XafDefaultProperty(nameof(Name))]
    public class Product :OutlookInspiredBaseObject, IViewFilter,ISalesMapsMarker{
        
        public const string CardViewDetailView = "ProductCardView_DetailView";
        public const string BrochureDetailView = "Product_Brochure_DetailView";
        public const string MapsDetailView = "Product_DetailView_Maps";
        
        [FontSizeDelta(8)]
        public  virtual string Name { get; set; }
        [EditorAlias(DevExpress.ExpressApp.Editors.EditorAliases.RichTextPropertyEditor)]
        public  virtual byte[] Description { get; set; }

        
        [VisibleInDetailView(false)][VisibleInListView(false)][VisibleInLookupListView(false)]
        public virtual string DescriptionString => Description.ToDocument(server => server.Text);
        public  virtual DateTime ProductionStart { get; set; }
        public  virtual bool Available { get; set; }
        [ImageEditor(ListViewImageEditorMode = ImageEditorMode.PictureEdit,
            DetailViewImageEditorMode = ImageEditorMode.PictureEdit,ImageSizeMode = ImageSizeMode.Zoom)]
        public  virtual byte[] Image { get; set; }
        public virtual Employee Support { get; set; }
        public virtual Employee Engineer { get; set; }
        [Browsable(false)]
        public virtual Guid? EngineerId { get; set; }
        [XafDisplayName("Inventory")]
        public  virtual int? CurrentInventory { get; set; }
        public  virtual int Backorder { get; set; }
        public  virtual int Manufacturing { get; set; }
        [NotMapped][VisibleInDetailView(false)]
        public virtual ObservableCollection<MapItem> CitySales{ get; set; } = new();
        public virtual Picture PrimaryImage { get; set; }
        [Column(TypeName = CurrencyType)]
        public  virtual decimal Cost { get; set; }
        [Column(TypeName = CurrencyType)]
        public  virtual decimal SalePrice { get; set; }
        [Column(TypeName = CurrencyType)]
        public  virtual decimal RetailPrice { get; set; }
        public  virtual double Weight { get; set; }
        public  virtual double ConsumerRating { get; set; }
        public  virtual ProductCategory Category { get; set; }

        [InverseProperty(nameof(ProductCatalog.Product))][Aggregated]
        public virtual ObservableCollection<ProductCatalog> Catalogs{ get; set; } = new();
        Expression<Func<OrderItem, bool>> ISalesMapsMarker.SalesExpression => item => item.Product.ID == ID;

        [Aggregated]
        public virtual ObservableCollection<ProductImage> Images{ get; set; } = new();

        [Aggregated]
        public virtual ObservableCollection<QuoteItem> QuoteItems{ get; set; } = new();
        [EditorAlias(EditorAliases.PdfViewerEditor)]
        public byte[] Brochure => Catalogs.Select(catalog => catalog.PDF).FirstOrDefault();
        string IBaseMapsMarker.Title => Name;
        double IBaseMapsMarker.Latitude => throw new NotImplementedException();
        double IBaseMapsMarker.Longitude => throw new NotImplementedException();
        [InverseProperty(nameof(OrderItem.Product))][Aggregated]
        
        public virtual ObservableCollection<OrderItem> OrderItems{ get; set; } = new();


        IEnumerable<Order> ISalesMapsMarker.Orders => OrderItems.Select(item => item.Order).Distinct();
    }

    public enum ProductCategory {
        [ImageName(nameof(Automation))]
        Automation,
        [ImageName(nameof(Monitors))]
        Monitors,
        [ImageName(nameof(Projectors))]
        Projectors,
        [ImageName(nameof(Televisions))]
        Televisions,
        [ImageName(nameof(VideoPlayers))]
        VideoPlayers
    }

}