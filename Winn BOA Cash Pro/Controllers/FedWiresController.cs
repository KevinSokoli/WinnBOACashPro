using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using Winn_BOA_Cash_Pro.Data;
using Winn_BOA_Cash_Pro.Models;

namespace Winn_BOA_Cash_Pro.Controllers
{
    public class FedWiresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _usermanager;
        public FedWiresController(ApplicationDbContext context, UserManager<AppUser> usermanager)
        {
            _context = context;
            _usermanager = usermanager;
        }

        // GET: FedWires
        public async Task<IActionResult> Index()
        {
            var newFedWires = await _context.FedWires
                .Where(fw => fw.TransactionStatus == "New")
                .ToListAsync();

            return View(newFedWires);
        }

        // GET: FedWires/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.FedWires == null)
            {
                return NotFound();
            }

            var fedWire = await _context.FedWires
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fedWire == null)
            {
                return NotFound();
            }

            return View(fedWire);
        }

        // GET: FedWires/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FedWires/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FromAccountName,FromBankName,FromAccountNumber,FromAbanumber,ToAccountName,ToBankName,ToAccountNumber,ToAbanumber,TransferAmount,ToBankCity,Description,TransactionStatus,CreatedDate,CreatedBy")] FedWire fedWire)
        {
            if (ModelState.IsValid)
            {
                fedWire.CreatedDate = DateTime.Now;
                fedWire.CreatedBy = User.Identity.Name;
                _context.Add(fedWire);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fedWire);
        }

        // GET: FedWires/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.FedWires == null)
            {
                return NotFound();
            }

            var fedWire = await _context.FedWires.FindAsync(id);
            if (fedWire == null)
            {
                return NotFound();
            }
            return View(fedWire);
        }

        // POST: FedWires/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FromAccountName,FromBankName,FromAccountNumber,FromAbanumber,ToAccountName,ToBankName,ToAccountNumber,ToAbanumber,TransferAmount,Description,ToBankCity,TransactionStatus,CreatedDate,CreatedBy")] FedWire fedWire)
        {
            if (id != fedWire.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fedWire);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FedWireExists(fedWire.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fedWire);
        }

        // GET: FedWires/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.FedWires == null)
            {
                return NotFound();
            }

            var fedWire = await _context.FedWires
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fedWire == null)
            {
                return NotFound();
            }

            return View(fedWire);
        }

        // POST: FedWires/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.FedWires == null)
            {
                return Problem("Entity set 'ApplicationDbContext.FedWires'  is null.");
            }
            var fedWire = await _context.FedWires.FindAsync(id);
            if (fedWire != null)
            {
                _context.FedWires.Remove(fedWire);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FedWireExists(int id)
        {
          return (_context.FedWires?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        public async Task<IActionResult> DownloadDetails()
        {
            var newFedWires = await _context.FedWires.Where(f => f.TransactionStatus == "New").ToListAsync();

            if (newFedWires.Any())
            {
                decimal totalTransferAmount = 0;
                int totalSumOfLast10Digits = 0;
                var firstFedWire = newFedWires.First();
                var combinedTextContent = new StringBuilder();
                string filler = new string(' ', 39);
                string Blank = new string(' ', 2);
                int numberOfIds = newFedWires.Count;
                combinedTextContent.Append($"SIGNONRECWINNBAFF{Blank}{firstFedWire.CreatedDate.ToString("yyyyMMddhhmmss")}WINNTRSY{filler}\n");
                foreach (var fedWire in newFedWires)
                {
                    string formattedToAccountNumber = fedWire.ToAccountNumber.PadRight(35);
                    string formattedToBankName = fedWire.ToBankName.PadRight(33);
                    string formattedToBankCity = fedWire.ToBankCity.PadRight(30);
                    string formattedFromAccountNumber = fedWire.FromAccountNumber.PadRight(35);
                    string formattedFromBankName = fedWire.FromAccountName.PadRight(35);
                    string formattedToAccountName = fedWire.ToAccountName.PadRight(35);
                    string formattedFromAbanumber = fedWire.FromAbanumber.PadRight(11);
                    string formattedToAbanumber = fedWire.ToAbanumber.PadRight(11);
                    string formattedDescription = fedWire.Description.PadLeft(35);
                    string formattedCreatedDate = fedWire.CreatedDate.ToString("yyMMddhhmm").PadRight(15);
                    string Blank2 = new string(' ', 3);
                    string Blank1 = new string(' ', 2);
                    string Blank3 = new string(' ', 18);
                    string Blank4 = new string(' ', 4);
                    string Blank5 = new string(' ', 12);
                    string Blank6 = new string(' ', 15);
                    string Blank7 = new string(' ', 35);
                    string Blank8 = new string(' ', 5);
                    string Blank11 = new string(' ', 6);
                    string fillerP40 = new string(' ', 26);
                    string fillerP50 = new string(' ', 25);
                    string fillerP32 = new string(' ', 33);
                    string textContent =
                    $"CRFBAF820WINNBAFF{Blank1}{formattedFromAccountNumber}FWT{fedWire.CreatedDate.ToString("yyyyMMdd")}USDP0CRF02.1{Blank} \n" +
                    $"P20WINNBAFF{Blank}{fedWire.CreatedDate.ToString("yyyyMMdd")}FWT{formattedCreatedDate}{fedWire.CreatedDate.ToString("yyyyMMdd")}{fedWire.TransferAmount.ToString("000000000000000.00")}USDD{Blank}C{Blank}CCD{Blank2}\n" +
                    $"P32{Blank11}ADD{formattedDescription}{fillerP32}\n" +
                    $"P40{formattedFromAccountNumber}{formattedFromAbanumber}USDUS{fillerP40}\n" +
                    $"P41FWT{formattedToAbanumber}DA{Blank1}{formattedToAccountNumber}{Blank3}US{Blank4}\n" +
                    $"P42RB{formattedToBankName}{Blank5}{formattedToBankCity}\n" +
                    $"P50{formattedFromBankName}{Blank6}US{fillerP50}\n" +
                    $"P53{formattedToAccountName}{Blank7}US{Blank8}\n";

                    // Calculate the sum of the last 10 digits for each record
                    string last10Digits = fedWire.ToAccountNumber.Length >= 10
                    ? fedWire.ToAccountNumber.Substring(fedWire.ToAccountNumber.Length - 10)
                    : fedWire.ToAccountNumber;
                    int sumOfLast10Digits = int.Parse(last10Digits);
                    totalSumOfLast10Digits += sumOfLast10Digits;
                    totalTransferAmount += fedWire.TransferAmount;
                    combinedTextContent.Append(textContent);

                    fedWire.TransactionStatus = "Processed";
                }
                string Blank9 = new string('0', 15);
                string Blank10 = new string('0', 4);
                int totalNumberOfLines = combinedTextContent.ToString().Split('\n').Length;
                string Numberofphysicalrecordsinfile = totalNumberOfLines.ToString().PadLeft(15, '0');
                string NumberofP20credittransactions = numberOfIds.ToString().PadLeft(15, '0');;
                string FileTrailerRecordCount = totalSumOfLast10Digits.ToString().PadLeft(10, '0');
                long TotalTransferAmount = (long)(totalTransferAmount * 100);
                string PaymentAmountControlTotal = TotalTransferAmount.ToString("D14").PadLeft(18, '0');


                combinedTextContent.Append("P80"+Numberofphysicalrecordsinfile+NumberofP20credittransactions+Blank9+FileTrailerRecordCount + PaymentAmountControlTotal+Blank10);

                // Save the changes to the database.
                await _context.SaveChangesAsync();

                // Provide the combined details as a downloadable text file.
                return File(Encoding.UTF8.GetBytes(combinedTextContent.ToString()), "text/plain", "newFedWireDetails.txt");
            }

            else
            {
                // Display a message when there are no files to download.
                TempData["NoFilesMessage"] = "There are no files to download.";

                return RedirectToAction("Index"); // Redirect to the Index action or any other appropriate action.
            }
        }

    }
}
