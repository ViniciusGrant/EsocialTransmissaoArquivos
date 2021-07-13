using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Web.Http;
using System.Xml;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.ServiceModel;
using System.IO;

namespace E_Social.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();

        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void AssinaturaXml()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = false;

            //PASSAR O ARQUIVO DO EVENTO PARA ASSINAR
            xmlDoc.Load(@"LOCAL DO XML");

            X509Certificate2 certificate = new X509Certificate2(@"SEU CERTIFICADO DIGITAL AQUI", "SENHA DO CERTIFICADO");

            //SignXmlDoc(xmlDoc, certificate);
            //enviarRequisicao();
            ConsultarEnvio();

        }

        private static void SignXmlDoc(XmlDocument xmlDoc, X509Certificate2 certificate)
        {
            SignedXml signedXml = new SignedXml(xmlDoc);

            signedXml.SigningKey = certificate.GetRSAPrivateKey();   

            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url; 

            Reference reference = new Reference("");

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform());
            reference.DigestMethod = SignedXml.XmlDsigSHA256Url; 
            signedXml.AddReference(reference);

            signedXml.KeyInfo = new KeyInfo();

            signedXml.KeyInfo.AddClause(new KeyInfoX509Data(certificate));

            signedXml.ComputeSignature();

            XmlElement xmlDigitalSignature = signedXml.GetXml();

            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

            if (xmlDoc.FirstChild is XmlDeclaration)
                xmlDoc.RemoveChild(xmlDoc.FirstChild);
            xmlDoc.Save(@"LOCAL DO XML");

        }

        private void enviarRequisicao()
        {
            try
            {
            
                string web_service_teste = "https://webservices.producaorestrita.esocial.gov.br/servicos/empregador/enviarloteeventos/WsEnviarLoteEventos.svc?wsdl";
                //string web_service_teste = "https://webservices.envio.esocial.gov.br/servicos/empregador/enviarloteeventos/WsEnviarLoteEventos.svc?wsdl";
                string xml_soap = @"LOCAL DO XML";
                string url = web_service_teste;
                X509Certificate2 cert = new X509Certificate2(@"SEU CERTIFICADO", "SENHA DO CERTIFICADO");

                XDocument soapEnvelopeXml = XDocument.Load(xml_soap);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web_service_teste);
            
                var urlServicoEnvio = @"https://webservices.producaorestrita.esocial.gov.br/servicos/empregador/enviarloteeventos/WsEnviarLoteEventos.svc";
                //var urlServicoEnvio = @"https://webservices.envio.esocial.gov.br/servicos/empregador/enviarloteeventos/WsEnviarLoteEventos.svc";

                var addressEnvio = new EndpointAddress(urlServicoEnvio);
                var binding = new BasicHttpsBinding();  
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

                var wsClient = new ServiceReference1.ServicoEnviarLoteEventosClient(binding, addressEnvio);
                wsClient.ClientCredentials.ClientCertificate.Certificate = cert;

                var retornoEnvioXElement = wsClient.EnviarLoteEventos(soapEnvelopeXml.Root);
                wsClient.Close();


            }
            catch (WebException webex)
            {
               
            }
        }

        public void ConsultarEnvio()
        {
            string xml_ret = @"SEU XML DE RETORNO";
            XDocument doc = XDocument.Load(xml_ret);


            X509Certificate2 cert = new X509Certificate2(@"SEU CERTIFICADO DIGITAL", "SENHA DO CERTIFICADO");
        
            //var urlServicoConsulta = @"https://webservices.consulta.esocial.gov.br/servicos/empregador/consultarloteeventos/WsConsultarLoteEventos.svc";
            var urlServicoConsulta = @"https://webservices.producaorestrita.esocial.gov.br/servicos/empregador/consultarloteeventos/WsConsultarLoteEventos.svc";
            var addressConsulta = new EndpointAddress(urlServicoConsulta);
            var binding = new BasicHttpsBinding();
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

            var wsClient = new ServiceReferenceConsultaLote.ServicoConsultarLoteEventosClient(binding, addressConsulta);
            wsClient.ClientCredentials.ClientCertificate.Certificate = cert;

            var retorno = wsClient.ConsultarLoteEventos(doc.Root);
        }

    }
}