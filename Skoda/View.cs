using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Cars
{
    public partial class View : Form, IView
    {
        DataGridView carsTable;
        DataGridView saleWeekendTable;
        ViewResult viewResult;

        public View()
        {
            InitializeComponents();
            this.Size = new Size(600, 650);
            this.Text = "Prodaná auta";
            SetMainTable();
            SetButtons();
        }

        private void InitializeComponents()
        {
        }

        DataGridView SetTableHead()
        {
            DataGridView tableHead = new DataGridView();
            tableHead.RowHeadersVisible = false;
            tableHead.Location = new Point(10, 10);
            tableHead.BackgroundColor = SystemColors.Window;
            tableHead.Font = new Font("Calibri", 10, FontStyle.Bold);
            tableHead.EnableHeadersVisualStyles = false;
            tableHead.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            tableHead.DefaultCellStyle.Padding = new Padding(5, 2, 5, 2);
            tableHead.Height = tableHead.ColumnHeadersHeight + (tableHead.Rows.Count * tableHead.RowTemplate.Height);

            tableHead.AllowUserToResizeRows = false;
            tableHead.AllowUserToResizeColumns = false;
            tableHead.AllowUserToAddRows = false;
            tableHead.ReadOnly = true;
            return tableHead;
        }

        void SetMainTable()
        {
            carsTable = SetTableHead();

            carsTable.Columns.Add(CreateColumn("ModelName", "Název modelu", 120));
            carsTable.Columns.Add(CreateColumn("DateOfSell", "Datum prodeje", 100));
            carsTable.Columns.Add(CreateColumn("Price", "Cena", 75));
            carsTable.Columns.Add(CreateColumn("Tax", "DPH", 75));

            int totalWidth = 0;
            foreach (DataGridViewColumn column in carsTable.Columns)
            {
                totalWidth = totalWidth + column.Width;
            }
            carsTable.Width = totalWidth + 3;

            carsTable.Columns["DateOfSell"].DefaultCellStyle.Format = "d.M.yyyy";

            Controls.Add(carsTable);
        }

        void SetSaleWeekendTable()
        {
            saleWeekendTable = SetTableHead();

            saleWeekendTable.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            saleWeekendTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            saleWeekendTable.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            saleWeekendTable.Columns.Add(CreateColumn("NoTax", "Název Modelu" + "\r\n" + "Cena bez DPH", 160));
            saleWeekendTable.Columns.Add(CreateColumn("AddTax", "\r\n" + "Cena s DPH", 160));

            int totalWidth = 0;
            foreach (DataGridViewColumn column in saleWeekendTable.Columns)
            {
                totalWidth = totalWidth + column.Width;
            }
            saleWeekendTable.Width = totalWidth + 3;

            viewResult.Controls.Add(saleWeekendTable);
        }

        DataGridViewTextBoxColumn CreateColumn(string modelName, string name, int width)
        {
            DataGridViewTextBoxColumn carTableColumn = new DataGridViewTextBoxColumn();
            carTableColumn.Name = modelName;
            carTableColumn.HeaderText = name;
            carTableColumn.Width = width;
            return carTableColumn;
        }

        void CreateRow(string modelName, DateTime date, string price, double tax)
        {
            carsTable.Rows.Add(modelName, date, price, tax);
            carsTable.Height = 2 + carsTable.ColumnHeadersHeight + (carsTable.Rows.Count * carsTable.RowTemplate.Height);
        }

        public event Func<Brand> CarInfo_View;
        void FillMainTable()
        {
            var cars = CarInfo_View();
            carsTable.Rows.Clear();

            CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberGroupSeparator = ".";
            customCulture.NumberFormat.NumberDecimalSeparator = ",";

            foreach (var table in cars.carModels)
            {
                foreach (var car in table.cars)
                {
                    string price = car.price.ToString("N0", customCulture) + ",-";
                    CreateRow(cars.brandName + " " + table.modelName, car.dateOfSale, price, car.taxRate);
                }
            }
        }

        void SetButtons()
        {
            AddButton(10, "Load XML");
            AddButton(40, "Save XML");
            AddButton(70, "Weekend Sales");
        }

        void AddButton(int Y, string text)
        {
            System.Windows.Forms.Button button = new System.Windows.Forms.Button();
            button.Size = new Size(100, 25);
            button.Location = new Point(450, Y);
            button.Text = text;
            if (text == "Load XML")
                button.Click += new EventHandler(LoadXML);
            if (text == "Save XML")
                button.Click += new EventHandler(SaveXML);
            if (text == "Weekend Sales")
                button.Click += new EventHandler(WeekendSale);
            Controls.Add(button);
        }

        public event Action LoadXML_View;
        void LoadXML(object sender, EventArgs e)
        {
            LoadXML_View();
            FillMainTable();
        }


        public event Action SaveXML_View;
        void SaveXML(object sender, EventArgs e)
        {
            SaveXML_View();
        }

        public event Func<int, List<WeekendSaleResult>> WeekendSale_View;
        void WeekendSale(object sender, EventArgs e)
        {
            var result = WeekendSale_View(1);
            RunResultView(result);
        }

        public partial class ViewResult : Form
        {

        }

        void RunResultView(List<WeekendSaleResult> result)
        {
            viewResult = new ViewResult();
            viewResult.Text = "Auta prodaná o víkendu";
            int unitposition = 0;
            viewResult.Size = new Size(600, 600);

            SetSaleWeekendTable();

            saleWeekendTable.Columns[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            saleWeekendTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            saleWeekendTable.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            saleWeekendTable.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
            saleWeekendTable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            saleWeekendTable.Columns["NoTax"].DefaultCellStyle.Format = "F2";

            CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberGroupSeparator = ".";
            customCulture.NumberFormat.NumberDecimalSeparator = ",";

            foreach (var table in result)
            {
                string priceNoTax = table.priceTotalNoTax.ToString("F2", customCulture);
                string priceTax = table.priceTotalAddTax.ToString("F2", customCulture);
                saleWeekendTable.Rows.Add(table.brand + " " + table.model + "\r\n" + priceNoTax, priceTax);
            }

            saleWeekendTable.Height = 2 * (2 + saleWeekendTable.ColumnHeadersHeight + saleWeekendTable.Rows.Count * saleWeekendTable.RowTemplate.Height) - 12;

            viewResult.ShowDialog();
        }
    }
}

