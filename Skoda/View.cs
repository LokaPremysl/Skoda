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
        DataGridView? carsTable;
        DataGridView? saleWeekendTable;
        ViewResult? viewResult;
        ViewModels? viewModels;

        List<string> modelNames = new List<string>() { "Favorit", "Forman", "Felicia", "Fabia", "Octavia" };


        public View()
        {
            InitializeComponents();
            this.Size = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
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
            if (viewResult != null)
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
            carsTable?.Rows.Add(modelName, date, price, tax);
            if (carsTable != null)
                carsTable.Height = 2 + carsTable.ColumnHeadersHeight + (carsTable.Rows.Count * carsTable.RowTemplate.Height);
        }

        public event Func<Brand>? CarInfo_View;
        void FillMainTable()
        {
            if (CarInfo_View != null)
            {
                var cars = CarInfo_View();
                carsTable?.Rows.Clear();
                CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                customCulture.NumberFormat.NumberGroupSeparator = ".";
                customCulture.NumberFormat.NumberDecimalSeparator = ",";

                foreach (var table in cars.carModels)
                {
                    foreach (var car in table.cars)
                    {
                        string price = car.price.ToString("N0", customCulture) + ",-";
                        CreateRow(cars.brandName + " " + table.modelName, car.dateOfSale, price, car.taxRate);
                    };
                }
            }
        }

        void SetButtons()
        {
            AddButton(10, "Víkendový prodej");
            AddButton(40, "Zkontrolovat data");
            AddButton(70, "Zobrazit modely Škoda");
            AddButton(130, "Nahrát XML");
            AddButton(160, "Uložit setøídìné XML");
        }

        void AddButton(int Y, string text)
        {
            System.Windows.Forms.Button button = new System.Windows.Forms.Button();
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            button.Size = new Size(130, 25);
            button.Location = new Point(450, Y);
            button.Text = text;
            if (text == "Víkendový prodej")
            {
                toolTip.SetToolTip(button, "Provede souèet cen vozù prodaných o víkendu");
                button.Click += new EventHandler(WeekendSale);
            }
            if (text == "Zkontrolovat data")
            {
                toolTip.SetToolTip(button, "Zkontroluje naètená data");
                button.Click += new EventHandler(CheckTableData);
            }
            if (text == "Zobrazit modely Škoda")
            {
                toolTip.SetToolTip(button, "Zobrazí dostupné modely Škoda");
                button.Click += new EventHandler(ShowModels);
            }
            if (text == "Nahrát XML")
            {
                toolTip.SetToolTip(button, "Nahraje soubor XML v požadované struktuøe");
                button.Click += new EventHandler(LoadXML);
            }
            if (text == "Uložit setøídìné XML")
            {
                toolTip.SetToolTip(button, "Uloží soubor XML s uspoøádáním dle modelu");
                button.Click += new EventHandler(SaveXML);
            }
            Controls.Add(button);
        }

        public event Action? LoadXML_View;
        void LoadXML(object? sender, EventArgs e)
        {
            if (LoadXML_View != null)
            {
                LoadXML_View();
                FillMainTable();
            }
        }

        public event Action? SaveXML_View;
        void SaveXML(object? sender, EventArgs e)
        {
            if (SaveXML_View != null)
            {
                SaveXML_View();
            }
        }

        public event Func<(int state, List<WeekendSaleResult> result)>? WeekendSale_View;
        void WeekendSale(object? sender, EventArgs e)
        {
            if (WeekendSale_View != null)
            {
                (int checkState, List<WeekendSaleResult> result) = WeekendSale_View();
                if (checkState == 1)
                {
                    MessageBox.Show($"Nejsou naètena data, zkuste nahrát XML soubor", "Upozonìní", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    RunResultView(result);
                }
            }
        }

        public event Func<int>? CheckTableData_View;
        void CheckTableData(object? sender, EventArgs e)
        {
            if (CheckTableData_View != null)
            {
                int checkState = CheckTableData_View();

                if (checkState == 1)
                {
                    MessageBox.Show($"Nejsou naètena data, zkuste nahrát XML soubor", "Upozonìní", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    RunCheckTableData();
                }
            }
        }

        void RunCheckTableData()
        {
            int rowIndex = 0;
            int numberOfErrors = 0;
            if (carsTable?.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in carsTable.Rows)
                {
                    if (!string.IsNullOrEmpty(row.Cells[0].Value.ToString()))
                    {
                        int spaceIndex = row.Cells[0].Value?.ToString()?.IndexOf(' ') ?? -1;
                        string cellValue = row.Cells[0].Value?.ToString() ?? "";

                        if (cellValue != null && spaceIndex >= 0 && spaceIndex + 1 < cellValue.Length)
                        {
                            string modelName = cellValue.Substring(spaceIndex + 1);
                            if (modelName != null)
                                if (!modelNames.Contains(modelName))
                                {
                                    carsTable.Rows[rowIndex].Cells[0].Style.BackColor = Color.LightPink;
                                    numberOfErrors++;
                                    MessageBox.Show($"Model vozu nekoresponduje s modely typu Škoda", "Upozonìní", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                        }
                    }
                    if (((DateTime)row.Cells[1].Value <= new DateTime(2009, 12, 31) && (double)row.Cells[3].Value != 19) ||
                       ((DateTime)row.Cells[1].Value > new DateTime(2009, 12, 31) && (double)row.Cells[3].Value != 20))
                    {
                        carsTable.Rows[rowIndex].Cells[3].Style.BackColor = Color.LightPink;
                        numberOfErrors++;
                        MessageBox.Show($"Nesrovnalost v dani", "Upozonìní", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    };
                    rowIndex++;
                }
            }
            MessageBox.Show($"Kontrola dat probìhla s poètem podezøelých záznamù: {numberOfErrors}", "Upozonìní", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public partial class ViewResult : Form
        {
        }

        void RunResultView(List<WeekendSaleResult> result)
        {
            viewResult = new ViewResult();
            viewResult.Text = "Auta prodaná o víkendu";
            viewResult.Size = new Size(370, 400);

            SetSaleWeekendTable();
            if (saleWeekendTable != null)
            {
                saleWeekendTable.Columns[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                saleWeekendTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                saleWeekendTable.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
                saleWeekendTable.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
                saleWeekendTable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                saleWeekendTable.Columns["NoTax"].DefaultCellStyle.Format = "F2";
            }
            CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberGroupSeparator = ".";
            customCulture.NumberFormat.NumberDecimalSeparator = ",";

            foreach (var table in result)
            {
                string priceNoTax = table.priceTotalNoTax.ToString("F2", customCulture);
                string priceTax = table.priceTotalAddTax.ToString("F2", customCulture);
                saleWeekendTable?.Rows.Add(table.brand + " " + table.model + "\r\n" + priceNoTax, priceTax);
            }

            if (saleWeekendTable != null)
                saleWeekendTable.Height = 2 * (2 + saleWeekendTable.ColumnHeadersHeight + saleWeekendTable.Rows.Count * saleWeekendTable.RowTemplate.Height) - 12;

            if (viewResult != null)
                viewResult.ShowDialog();
        }
        public partial class ViewModels : Form
        {
        }

        void ShowModels(object? sender, EventArgs e)
        {
            viewModels = new ViewModels();
            viewModels.Text = "Seznam modelù Škoda";
            viewModels.Size = new Size(70, 200);
            viewModels.StartPosition = FormStartPosition.CenterScreen;

            int textPositionY = 10;
            foreach (var model in modelNames)
            {
                Label text = new Label();
                text.Text = model;
                text.Location = new Point(20, textPositionY);
                viewModels.Controls.Add(text);
                textPositionY += 30;
            }
            viewModels.ShowDialog();
        }
    }
}

