using Microsoft.AspNetCore.Mvc;
using HeThongBanHang_Comestic_UI.Models;

namespace HeThongBanHang_Comestic_UI.ViewComponents
{
    public class MenuPriceViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var data = new List<MenuPrice>
            {
                
                new MenuPrice { label = "100k - 300k", minPrice = 100000, maxPrice = 300000 },
                new MenuPrice { label = "300k - 500k", minPrice = 300000, maxPrice = 500000 },
                new MenuPrice { label = "Trên 500k", minPrice = 500000, maxPrice = null }
            };

            return View(data);
        }
    }
}
