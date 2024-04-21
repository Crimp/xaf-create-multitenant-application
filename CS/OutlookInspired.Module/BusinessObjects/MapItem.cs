using System.ComponentModel.DataAnnotations;
using DevExpress.ExpressApp.DC;
using OutlookInspired.Module.Services.Internal;

namespace OutlookInspired.Module.BusinessObjects{
    public interface IMapItem{
        string City{ get; set; }
        double Latitude{ get; set; }
        double Longitude{ get; set; }
        decimal Total{ get; init; }
    }

    [DomainComponent]
    public class MapItem : IMapItem{
        [DevExpress.ExpressApp.Data.Key]
        public int ID{ get; set; }
        [MaxLength(100)]
        public string City{ get; set; }
        public double Latitude{ get; set; }
        public double Longitude { get; set; }
        public decimal Total { get; init; }
        [MaxLength(100)]
        public string CustomerName{ get; init; }
        [MaxLength(100)]
        public string ProductName{ get; init; }

        public ProductCategory ProductCategory{ get; set; }
        [MaxLength(20)]
        public string Color{ get; set; }

        public string PropertyValue(Type type) 
            => type.MapItemProperty() == nameof(ProductName) ? ProductName : CustomerName;
    }
}