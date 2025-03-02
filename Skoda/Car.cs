using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Cars
{
    [XmlRoot("Brand")]
    public class Brand 
    {
        [XmlElement("brandName")]
        public string brandName { get; set; } = string.Empty;
        
        [XmlArray("carModels")]
        [XmlArrayItem("CarModel")]
        public List<CarModel> carModels { get; set; } = new List<CarModel>();
    }

    public class CarModel
    {
        public string modelName { get; set; } = string.Empty;
        
        [XmlArray("cars")]
        [XmlArrayItem("Car")]
        public List<Car> cars { get; set; } = new List<Car>();
    }

    public class Car 
    {
        [XmlElement("price")] 
        public double price { get; set; } = 0;
        [XmlElement("dateOfSale")]
        public DateTime dateOfSale { get; set; } = DateTime.Now;
        [XmlElement("taxRate")]
        public double taxRate { get; set; } = 0;
    }

    public struct WeekendSaleResult
    { 
        public string brand;
        public string model;
        public double priceTotalNoTax;
        public double priceTotalAddTax;
    }
}


