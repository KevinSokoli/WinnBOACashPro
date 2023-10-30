using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Winn_BOA_Cash_Pro.Data;
using Winn_BOA_Cash_Pro.Models;

namespace Winn_BOA_Cash_Pro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        //public async Task<IActionResult> Index()
        //{
        //    return _context.FedWires != null ?
        //                View(await _context.FedWires.ToListAsync()) :
        //                Problem("Entity set 'ApplicationDbContext.FedWires'  is null.");
        //}
        public async Task<IActionResult> Index()
        {
            var newFedWires = await _context.FedWires
                .Where(fw => fw.TransactionStatus == "New")
                .ToListAsync();

            return View(newFedWires);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}