namespace NearGo.Configurations
{
public class SEPaySettings
{
    public string BankAccount { get; set; } = "108887305171";
    public string BankName { get; set; } = "VietinBank";
    public string AccountHolder { get; set; } = "NGUYEN DINH NAM";
    public string VANumber { get; set; } = "WA";
    public string QRBaseUrl { get; set; } = "https://qr.sepay.vn/img";
    public string ReturnUrl { get; set; } = string.Empty;
    public string WebhookToken { get; set; } = string.Empty;
}
}
