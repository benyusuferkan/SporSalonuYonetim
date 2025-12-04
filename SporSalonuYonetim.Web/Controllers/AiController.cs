using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json; 

namespace SporSalonuYonetim.Web.Controllers
{
    [Authorize] // Sadece üyeler girebilsin
    public class AiController : Controller
    {
        // 👇 SENİN API KEY'İN (Bunu buraya yazdım)
        private readonly string _apiKey = "AIzaSyCf3ruJFmpIVshVlVcG_U9ManCPH_Zki8M"; 

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetPlan(int age, int weight, int height, string goal)
        {
            string resultText = "";

            try
            {
                using (var client = new HttpClient())
                {
                    var prompt = $"Ben {age} yaşında, {weight} kilo ve {height} cm boyunda biriyim. Hedefim: {goal}. Bana kişisel bir spor hocası gibi hitap ederek; motive edici, emojili ve kısa maddeler halinde 1 günlük örnek antrenman ve beslenme programı hazırla. Cevabı Markdown formatında ver.";

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new {
                                parts = new[] { new { text = prompt } }
                            }
                        }
                    };

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    
                    // DÜZELTME BURADA: Modeli 'gemini-pro' yaptık, bu kesin çalışır.
                    string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";
                    
                    var response = await client.PostAsync(url, jsonContent);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                        resultText = jsonResponse.candidates[0].content.parts[0].text;
                    }
                    else
                    {
                        // Hata detayını görelim
                        var errorContent = await response.Content.ReadAsStringAsync();
                        resultText = $"⚠️ Hata: {response.StatusCode}. Detay: {errorContent}";
                    }
                }
            }
            catch (Exception ex)
            {
                resultText = "Bir hata oluştu: " + ex.Message;
            }

            return Json(new { success = true, message = resultText });
        }
    }
}