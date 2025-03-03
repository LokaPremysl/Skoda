using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics.Eventing.Reader;
using System.Collections;
using System.Threading.Channels;

namespace Cars
{
    internal class Presenter
    {
        Brand cars = new Brand();

        private readonly IView view;
        public Presenter(IView view)
        {
            this.view = view;
            this.view.LoadXML_View += DeserializationFromXML;
            this.view.SaveXML_View += SerializationToXML;
            this.view.CarInfo_View += CarInfo;
            this.view.WeekendSale_View += CountWeekendSale;
            this.view.CheckTableData_View += CheckTableData;
        }

        Brand CarInfo()
        {
            Brand carsToView = cars;
            return carsToView;
        }

        public void SerializationToXML()
        {
            Brand carSorted = DoSortedList(cars);
            string fileName = string.Empty;

            if(string.IsNullOrEmpty(carSorted.brandName)) 
            {
                MessageBox.Show($"Není načten soubor s daty k uložení", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Uložit soubor jako"
            };

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            fileName = saveFileDialog.FileName;

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var serializer = new XmlSerializer(typeof(Brand));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            if(carSorted != null) 
            { 
            try
            {
                using (FileStream fileStream = File.Create(fileName))
                using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
                {
                    serializer.Serialize(writer, carSorted);
                }
                MessageBox.Show("File saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error saving XML: {exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            }
            
        }

        public void DeserializationFromXML()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Otevřít soubor"
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string fileName = openFileDialog.FileName;

            using (FileStream projectLoadStream = File.OpenRead(fileName))
            {
                try
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    XmlSerializer serializer = new XmlSerializer(typeof(Brand));

                    using (XmlReader reader = XmlReader.Create(projectLoadStream, settings))
                    {
                        Brand? deserializedData = serializer.Deserialize(reader) as Brand;
                        if (deserializedData != null)
                        {
                            cars.carModels.Clear();
                            cars = deserializedData;
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"Soubor XML neobsahuje data v požadovaném formátu\n\n {exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public Brand DoSortedList(Brand unsortedData)
        {
            Brand sortedData = new Brand();
            sortedData.brandName = unsortedData.brandName;
            Dictionary<string, CarModel> dic = new Dictionary<string, CarModel>();

            foreach (var carModel in unsortedData.carModels)
            {
                if (!dic.ContainsKey(carModel.modelName))
                {
                    dic[carModel.modelName] = new CarModel
                    {
                        modelName = carModel.modelName,
                        cars = new List<Car>(carModel.cars),
                    };
                }
                else
                {
                    dic[carModel.modelName].cars.AddRange(carModel.cars);
                }
            }

            sortedData.carModels = dic.Values.ToList();
            return sortedData;
        }

        public (int, List<WeekendSaleResult>) CountWeekendSale()
        {
            int checkState = 0;
            List<WeekendSaleResult> weekendSaleAll = new List<WeekendSaleResult>();

            if (string.IsNullOrEmpty(cars.brandName))
            {
                checkState = 1;
            }
            else
            {
                checkState = 2;

                var sortedCarList = DoSortedList(cars);

                foreach (var carModel in sortedCarList.carModels)
                {
                    double totalPriceNoTax = 0;
                    double totalPriceWithTax = 0;

                    foreach (var car in carModel.cars)
                    {
                        if (car.dateOfSale.DayOfWeek == DayOfWeek.Saturday || car.dateOfSale.DayOfWeek == DayOfWeek.Sunday)
                        {
                            totalPriceNoTax += car.price;
                            totalPriceWithTax += car.taxRate * 0.01 * car.price + car.price;
                        }
                    }

                    var weekendSale = new WeekendSaleResult
                    {
                        brand = sortedCarList.brandName,
                        model = carModel.modelName,
                        priceTotalNoTax = totalPriceNoTax,
                        priceTotalAddTax = totalPriceWithTax
                    };
                    weekendSaleAll.Add(weekendSale);
                }
            }
            return (checkState, weekendSaleAll);

        }
        public int CheckTableData()
        {
            int checkState = 0;

            if (string.IsNullOrEmpty(cars.brandName))
            {
                checkState = 1;
            }
            return checkState;
        }
    }
}

