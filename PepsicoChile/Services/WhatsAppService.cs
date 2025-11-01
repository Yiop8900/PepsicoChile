namespace PepsicoChile.Services
{
    public interface IWhatsAppService
    {
        Task<string> EnviarMensaje(string numeroDestino, string mensaje);
    }

    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;

        public WhatsAppService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> EnviarMensaje(string numeroDestino, string mensaje)
        {
            try
            {
                // Configuración de Twilio desde appsettings.json
                var accountSid = _configuration["Twilio:AccountSid"];
                var authToken = _configuration["Twilio:AuthToken"];
                var fromNumber = _configuration["Twilio:WhatsAppNumber"];

                Console.WriteLine($"[DEBUG] Intentando enviar WhatsApp...");
                Console.WriteLine($"[DEBUG] AccountSid: {accountSid?.Substring(0, 10)}...");
                Console.WriteLine($"[DEBUG] FromNumber: {fromNumber}");
                Console.WriteLine($"[DEBUG] DestinoNumber: {numeroDestino}");

                // Validar configuración
                if (string.IsNullOrEmpty(accountSid) || accountSid == "TU_ACCOUNT_SID")
                {
                    Console.WriteLine($"[SIMULADO] WhatsApp a {numeroDestino}: {mensaje}");
                    return "SIM-" + Guid.NewGuid().ToString().Substring(0, 8);
                }

                // PRODUCCIÓN: Envío real con Twilio
                try
                {
                    Twilio.TwilioClient.Init(accountSid, authToken);

                    // Formatear números para WhatsApp
                    var from = new Twilio.Types.PhoneNumber($"whatsapp:{fromNumber}");
                    var to = new Twilio.Types.PhoneNumber($"whatsapp:{numeroDestino}");

                    Console.WriteLine($"[TWILIO] Enviando de {from} a {to}");

                    var messageResource = await Twilio.Rest.Api.V2010.Account.MessageResource.CreateAsync(
                        to: to,
                        from: from,
                        body: mensaje
                    );

                    Console.WriteLine($"[TWILIO] Mensaje enviado exitosamente. SID: {messageResource.Sid}");
                    Console.WriteLine($"[TWILIO] Status: {messageResource.Status}");

                    return messageResource.Sid;
                }
                catch (Twilio.Exceptions.ApiException twilioEx)
                {
                    Console.WriteLine($"[ERROR TWILIO] Código: {twilioEx.Code}");
                    Console.WriteLine($"[ERROR TWILIO] Mensaje: {twilioEx.Message}");
                    Console.WriteLine($"[ERROR TWILIO] Más info: {twilioEx.MoreInfo}");

                    // Retornar SID simulado pero logear el error
                    return "ERR-" + Guid.NewGuid().ToString().Substring(0, 8);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR GENERAL] Error enviando WhatsApp: {ex.Message}");
                Console.WriteLine($"[ERROR GENERAL] StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
