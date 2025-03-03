using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cars
{
    public interface IView
    {
        public event Action LoadXML_View;
        public event Action SaveXML_View;
        public event Func<Brand> CarInfo_View;
        public event Func<(int state, List<WeekendSaleResult> result)> WeekendSale_View;
        public event Func<int> CheckTableData_View;
    }
}
