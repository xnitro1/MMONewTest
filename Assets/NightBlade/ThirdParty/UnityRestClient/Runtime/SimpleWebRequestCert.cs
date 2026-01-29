using UnityEngine.Networking;

namespace NightBlade.UnityRestClient
{
    public class SimpleWebRequestCert : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}







