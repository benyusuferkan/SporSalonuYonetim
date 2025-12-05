using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace SporSalonuYonetim.Web.Controllers
{
    public class AiController : Controller
    {
        private readonly IConfiguration _configuration;

        public AiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetPlan(int? age, int? weight, int? height, string goal)
        {
            // 1. Veri Kontrolü
            if (age == null || weight == null || height == null || string.IsNullOrEmpty(goal))
            {
                return Json(new { success = false, message = "Lütfen tüm alanları doldurun." });
            }

            string apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return Json(new { success = false, message = "API Anahtarı bulunamadı." });
            }

            // 2. Prompt Hazırlığı
            string prompt = $@"
                Sen uzman bir spor hocasısın.
                Kullanıcı: Yaş {age}, Kilo {weight}, Boy {height}, Hedef {goal}.
                Lütfen Türkçe olarak Markdown formatında şunları yaz:
                ### 1. Vücut Analizi
                ### 2. Haftalık Antrenman Planı (Tablo şeklinde)
                ### 3. Beslenme Tavsiyeleri (Maddeler halinde)
            ";

            using (var client = new HttpClient())
            {
                try
                {
                    // --- ADIM 1: ÇALIŞAN MODELİ BUL (Auto-Discovery) ---
                    // Google'a soruyoruz: "Elimdeki anahtarla hangi modeller açık?"
                    string listModelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
                    var listResponse = await client.GetAsync(listModelsUrl);
                    
                    if (!listResponse.IsSuccessStatusCode)
                    {
                        var error = await listResponse.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = $"Model listesi alınamadı. Hata: {error}" });
                    }

                    var listJson = await listResponse.Content.ReadAsStringAsync();
                    dynamic listData = JsonConvert.DeserializeObject(listJson);
                    
                    string validModelName = "";

                    // Listeden 'generateContent' yeteneği olan ilk modeli seçiyoruz
                    foreach (var model in listData.models)
                    {
                        // Modelin yeteneklerine bak
                        string supportedMethods = model.supportedGenerationMethods?.ToString() ?? "";
                        string name = model.name?.ToString() ?? "";

                        if (supportedMethods.Contains("generateContent"))
                        {
                            validModelName = name; // Örn: "models/gemini-1.0-pro"
                            
                            // Eğer daha iyi bir model (1.5 veya Flash) bulursak onu tercih et
                            if (name.Contains("1.5") || name.Contains("flash"))
                            {
                                validModelName = name;
                                break; // En iyiyi bulduk, döngüden çık
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(validModelName))
                    {
                        return Json(new { success = false, message = "API anahtarınızla uyumlu metin modeli bulunamadı." });
                    }

                    // --- ADIM 2: BULUNAN MODEL İLE İSTEK YAP ---
                    // validModelName zaten "models/gemini-..." formatında gelir, o yüzden url'e direkt ekliyoruz.
                    string generateUrl = $"https://generativelanguage.googleapis.com/v1beta/{validModelName}:generateContent?key={apiKey}";

                    var requestBody = new
                    {
                        contents = new[] { new { parts = new[] { new { text = prompt } } } }
                    };
                    
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(generateUrl, jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(jsonString);
                        string aiText = result.candidates[0].content.parts[0].text;
                        
                        return Json(new { success = true, message = aiText });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = $"Üretim Hatası ({validModelName}): {errorContent}" });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Sistem Hatası: {ex.Message}" });
                }
            }
        }
    }
}