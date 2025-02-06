namespace comment.Data.Model
{
    public class RECaptcha
    {
        public string Key = "<RECaptcha Site Key>";

        public string Secret = "<RECaptcha Secret Key>";
        public string Response { get; set; }
    }
}
