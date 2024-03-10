using Microsoft.AspNetCore.Mvc;
using Chu_Ky_So_RSA.Models;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
namespace Chu_Ky_So_RSA.Controllers
{
    public class CreateKeyController : Controller
    {

        bool check = false; 
        string chiper = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!,0123456789 ";
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
        public IActionResult Index(int check = 0)
        {
            if (check == 1)
            {
                ModelState.AddModelError("", "Trùng");
            }
            else if(check == 2) 
            {
                ModelState.AddModelError("", "Không trùng");

            }
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
            return RedirectToAction("Index");
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
            return RedirectToAction("Index",new {check = 0});
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

        //[HttpPost]
        //public IActionResult Encrypt([FromForm] string plainNumber)
        //{
        //    this.ingredients = this.getValueFromSession();

        //    int number = int.Parse(plainNumber);
        //    this.ingredients.plainNumber = number;

        //    this.ingredients.cipherNumber = powMod(number, this.ingredients.e, this.ingredients.n);

        //    this.setValueToSession(this.ingredients);

        //    return RedirectToAction("Index");
        //}

        public string HashPlainText(string rawData)
        {
            SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }


        [HttpPost]
        public IActionResult Hash([FromForm] string plainText)
        {
            this.ingredients = this.getValueFromSession();

            this.ingredients.hashText = HashPlainText(plainText);

            this.ingredients.plainText = plainText;

            this.setValueToSession(this.ingredients);

            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Encrypt([FromForm] string privateKey)
        {
            this.ingredients = this.getValueFromSession();

            string excypted_text = string.Empty;
            for(int i = 0;i < this.ingredients.hashText.Length;i++)
            {
               
                for(int j = 0;j < chiper.Length;j++)
                {
                    if (this.ingredients.hashText[i] == chiper[j])
                    {
                        string encryptPerChar = powMod(j, long.Parse(privateKey), this.ingredients.n).ToString() ;
                        excypted_text = excypted_text + encryptPerChar  + " "  ;
                    }
                }
            }
            excypted_text = excypted_text.Trim();

            this.ingredients.e = int.Parse(privateKey);

            this.ingredients.cipherText = excypted_text;

            this.setValueToSession(this.ingredients);

            return RedirectToAction("Index");   
        }

        [HttpPost]
        public IActionResult Decrypt([FromForm] string signature,string publicKey)
        {
            this.ingredients = getValueFromSession();

            //dũ liệu ban đầu được hash lại lần nữa
            //string hashText = this.HashPlainText(plainText);


            //phần chữ ký số được giải bằng public key
            var words = signature.Split(' ').ToList();

            string result = string.Empty;    

            foreach(string s in words)
            {
                //chuyen doi so thanh ki tu
                long numEncrypted = long.Parse(s);
               
                long publicKeyToNumber = long.Parse(publicKey);

                int numberToChar = (int)powMod(numEncrypted, publicKeyToNumber, this.ingredients.n);

                result = result + chiper[numberToChar];

            }

            TempData["result"] = result;
            return RedirectToAction("Index");
        }

        public IActionResult CheckFinalResult([FromForm] string hash1,string hash2)
        {
            int check = 0;
            if(hash1 == hash2)
            {
                check = 1;
            }
            else
            {
                check = 2;  
            }
            return RedirectToAction("Index", new {check = check});
        }

        //[HttpPost]
        //public IActionResult Decrypt([FromForm] string sercretKey)
        //{
        //    this.ingredients = getValueFromSession();
        //    long result = powMod(this.ingredients.cipherNumber, int.Parse(sercretKey), this.ingredients.n);
        //    TempData["result"] = result.ToString();
        //    return RedirectToAction("Index");
        //}

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
