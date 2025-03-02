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
        bool done = false;

        private readonly IView view;
        public Presenter(IView view)
        {
            this.view = view;
            this.view.LoadXML_View += DeserializationFromXML;
            this.view.SaveXML_View += SerializationToXML;
            this.view.CarInfo_View += CarInfo;
            this.view.WeekendSale_View += CountWeekendSale;
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

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Save file as"
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

        public void DeserializationFromXML()
        {

            if (cars == null)
            {
                cars = new Brand { carModels = new List<CarModel>() };
            }
            else
            {
                cars.carModels.Clear();
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Open file"
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
                        Brand deserializedData = serializer.Deserialize(reader) as Brand;
                        if (deserializedData != null)
                        {
                            cars = deserializedData;
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"Error loading XML: {exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public Brand DoSortedList(Brand unsortedData)
        {
            Brand sortedData = new Brand();
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

        public List<WeekendSaleResult> CountWeekendSale(int a)
        {
            List<WeekendSaleResult> weekendSaleAll = new List<WeekendSaleResult>();
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
            return weekendSaleAll;

        }
    }
}

