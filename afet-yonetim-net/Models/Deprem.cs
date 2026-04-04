namespace afet_yonetim_net.Models
{
    public class Deprem
    {
        public int id { get; set; }
        public double mag { get; set; }
        public string yer { get; set; }
        public DateTime zaman { get; set; }
        public double enlem { get; set; }
        public double boylam { get; set; }
    }
}
