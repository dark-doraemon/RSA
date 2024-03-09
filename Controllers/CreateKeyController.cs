using Microsoft.AspNetCore.Mvc;
using Chu_Ky_So_RSA.Models;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
namespace Chu_Ky_So_RSA.Controllers
{
    public class CreateKeyController : Controller
    {

        Ingredients ingredients = new Ingredients();

        //public CreateKeyController()
        //{
        //    if (base.HttpContext != null)
        //    {
        //        var stringIngre = HttpContext.Session.GetString("Ingredients");

        //        if(stringIngre != null)
        //        {
        //            var ingre = JsonConvert.DeserializeObject<Ingredients>(stringIngre);
        //            this.ingredients = ingre;
        //        }
                
        //    }
        //}
        
        [HttpGet]
        public IActionResult Index()
        {
            var stringIngre = HttpContext.Session.GetString("Ingredients");
            if(stringIngre == null)
            {
                return View(this.ingredients);
            }
            
            var ingre = JsonConvert.DeserializeObject<Ingredients>(stringIngre);
            return View(ingre);
        }

        [HttpPost]
        public IActionResult Index2([FromForm] Ingredients i)
        {
            this.ingredients.p = i.p;
            this.ingredients.q = i.q;   
            this.ingredients.n = i.p * i.q;
            this.ingredients.phiN = (i.p - 1) * (i.q - 1);

            for(int j = 2;j < this.ingredients.phiN; j++)
            {
                if(gcd(j,this.ingredients.phiN) == 1)
                {
                    this.ingredients.ListE.Add(j);
                    this.ingredients.strE += j + " ";
                }
            }

            Random rd = new Random();   
            this.ingredients.e = this.ingredients.ListE[rd.Next(0,this.ingredients.ListE.Count)];

            string value = JsonConvert.SerializeObject(this.ingredients);
            base.HttpContext.Session.SetString("Ingredients", value);
            return RedirectToAction("Index", this.ingredients);
        }

        public IActionResult setE(int e)
        {
            var stringIngre = HttpContext.Session.GetString("Ingredients");
            var ingre = JsonConvert.DeserializeObject<Ingredients>(stringIngre);
            ingre.e = e;
           
            int x, y;
            extended_euclid(ingre.e, ingre.phiN, out x,out y);
            
            while(x < 0)
            {
                x += ingre.phiN;
            }
            ingre.d = x;



            string value = JsonConvert.SerializeObject(ingre);
            base.HttpContext.Session.SetString("Ingredients", value);
            return RedirectToAction("Index");
        }

        public Ingredients getValueFromSession() 
        {
            var stringIngre = HttpContext.Session.GetString("Ingredients");
            var ingre = JsonConvert.DeserializeObject<Ingredients>(stringIngre);
            return ingre;
        }

        public void setValueToSession(Ingredients ingre )
        {
            string value = JsonConvert.SerializeObject(ingre);
            base.HttpContext.Session.SetString("Ingredients", value);
        }

        [HttpPost]
        public IActionResult Encry([FromForm] string plainNumber)
        {
            this.ingredients = this.getValueFromSession();

            int number = int.Parse(plainNumber);
            this.ingredients.plainNumber = number;

            this.ingredients.cipherNumber = powMod(number, this.ingredients.e, this.ingredients.n);

            this.setValueToSession(this.ingredients);

            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult DeEncry([FromForm] string sercretKey)
        {
            this.ingredients = getValueFromSession();
            long result = powMod(this.ingredients.cipherNumber, int.Parse(sercretKey), this.ingredients.n);
            ViewBag.result = result;
            return RedirectToAction("Index");
        }

        public int gcd(int a,int b)
        {
            if (b == 0) return a;

            return gcd(b, a % b);
        }

        public long extended_euclid(int a,int b,out int x,out int y)
        {
            if(b == 0)
            {
                x = 1;
                y = 0;
                return a;
            }
            int x1,y1;
            long d = extended_euclid(b, a % b, out x1, out  y1);

            x = y1;
            y = x1 - y1 * (a / b);

            return d;
        }

        public long powMod(long x, long y, int m)
        {
            if (y == 0)
            {
                return 1;
            }

            long  temp = powMod(x, y / 2, m);
            if (y % 2 == 1)
            {
                return (x % m * ((temp % m) * (temp % m))) % m;
            }

            else
            {
                return ((temp % m) * (temp % m)) % m;
            }
        }
    }
}
