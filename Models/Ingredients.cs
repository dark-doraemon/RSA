namespace Chu_Ky_So_RSA.Models
{
    public class Ingredients
    {
        public int p { get; set; } 
        public int q { get; set; } 
        public int phiN { get; set; } 

        public int n { get; set; } 

        public int d { get; set; } 


        public List<int> ListE { get; set; } = new List<int>();

        public string strE { get; set; } = string.Empty;

        public int e { get; set; }

        public int privateKey { get; set; }
        public int publicKey { get; set; }

        public string plainText { get; set; }   

        public string cipherText { get; set; }

        public long plainNumber { get; set; }
        public long cipherNumber { get; set; }  
    }
}
