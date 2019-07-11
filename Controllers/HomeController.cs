using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bankAccount_cSharp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bankAccount_cSharp.Controllers
{
    
    public class HomeController : Controller
    {
        private HomeContext dbContext;
     
        // here we can "inject" our context service into the constructorcopy
        public HomeController(HomeContext context)
        {
            dbContext = context;
        }

        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("create")]
        public IActionResult Create(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(user => user.Email == newUser.Email))
                {
                    ModelState.AddModelError("NewUser.Email", "Email already in use!");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                    dbContext.Add(newUser);
                    HttpContext.Session.SetInt32("Id", newUser.UserId);
                    dbContext.SaveChanges();
                    Console.WriteLine(newUser.UserId);
                    return Redirect($"account/{newUser.UserId}");
                }
            }
            return View("Index");
        }

        [HttpPost("login")]
        public IActionResult Login(LogUser LoggedUser)
        {

            if(ModelState.IsValid)
            {
                var confirmUser = dbContext.Users.FirstOrDefault(user => user.Email == LoggedUser.Email);
                Console.WriteLine(confirmUser.Email);
                if(confirmUser == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                
                var hasher = new PasswordHasher<LogUser>();
                
                var result = hasher.VerifyHashedPassword(LoggedUser, confirmUser.Password, LoggedUser.Password);
                
                if(result == 0)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                else
                {
                    HttpContext.Session.SetInt32("Id", confirmUser.UserId);
                    return Redirect($"account/{confirmUser.UserId}");
                }
            }
            return View("Index");

        }

        [HttpGet("account/{id}")]
        public IActionResult Show(int id)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                ModelState.AddModelError("Email", "Please log in or register!");
                return View("Index");
            }
            List<Transaction> AllTransactions = dbContext.Transactions
            .Include(a => a.AccountOwner)
            .Where(thisUser => thisUser.AccountOwner.UserId == HttpContext.Session.GetInt32("Id"))
            .ToList();
            foreach(var y in AllTransactions)
            {
                Console.WriteLine(y.CurrentBalance);
            }
            User loggedIn = dbContext.Users.FirstOrDefault(x => x.UserId == HttpContext.Session.GetInt32("Id"));
            // Console.WriteLine(loggedIn.FName);
            string name = loggedIn.FName;
            @ViewBag.Transactions = AllTransactions;
            double total = 0;
            foreach(var x in AllTransactions)
            {
                total += x.CurrentBalance;
            }
            @ViewBag.Total =total;
            ViewBag.loggedIn = loggedIn;
            return View("Account");
        }

        [HttpPost("transaction")]
        public IActionResult Transaction(Transaction newTransaction)
        {
            User loggedIn = dbContext.Users.FirstOrDefault(x => x.UserId == HttpContext.Session.GetInt32("Id"));
            // Console.WriteLine(loggedIn.FName);
            string name = loggedIn.FName;
            List<Transaction> AllTransactions = dbContext.Transactions
            .Include(a => a.AccountOwner)
            .Where(thisUser => thisUser.AccountOwner.UserId == HttpContext.Session.GetInt32("Id"))
            .ToList();
            double total = 0;
            foreach(var x in AllTransactions)
            {
                total += x.CurrentBalance;
            }
            if(ModelState.IsValid)
            {
                if(newTransaction.CurrentBalance < 0)
                {
                    if((total + newTransaction.CurrentBalance) < 0)
                    {
                        ModelState.AddModelError("CurrentBalance", "Insufficient Funds.");
                        @ViewBag.Transactions = AllTransactions;
                        @ViewBag.Total =total;
                        @ViewBag.loggedIn = loggedIn;
                        return View("Account");
                    }
                    User WithdrawUser = dbContext.Users
                    .FirstOrDefault(user => user.UserId == HttpContext.Session.GetInt32("Id"));
                    Console.WriteLine(WithdrawUser.FName);
                    newTransaction.UserId = WithdrawUser.UserId;
                    newTransaction.AccountOwner = WithdrawUser;
                    dbContext.Add(newTransaction);
                    dbContext.SaveChanges();
                    return Redirect($"account/{WithdrawUser.UserId}");
                }
                User AccountUser = dbContext.Users
                .FirstOrDefault(user => user.UserId == HttpContext.Session.GetInt32("Id"));
                Console.WriteLine(AccountUser.FName);
                newTransaction.UserId = AccountUser.UserId;
                newTransaction.AccountOwner = AccountUser;
                dbContext.Add(newTransaction);
                dbContext.SaveChanges();
                return Redirect($"account/{AccountUser.UserId}");
                
            }
            @ViewBag.Transactions = AllTransactions;
            @ViewBag.Total =total;
            ViewBag.loggedIn = loggedIn;
            return View("Account");
        }

    }
}
