using Newtonsoft.Json;
using System;
using Weather_Report;
using WeatherReport_jbot;

var telegramToken = "token";
var apiKey = "key";

var client = new HttpClient();
var offset = 0;

while(true)
{
    var tg_response = await client.GetAsync($"https://api.telegram.org/bot{telegramToken}/getUpdates?offset={offset}");
    var tg_data = JsonConvert.DeserializeObject<TelegramData>(tg_response.Content.ReadAsStringAsync().Result);

    if(tg_response.IsSuccessStatusCode && tg_data != null)
    {

        foreach (var data in tg_data.result)
        {
            var message = data.message.text.ToLower();
            string respone = "*Улитка*";

            if (message == "/start")
            {
                respone = "Ох, привет! Меня зовут Weather Report. Если тебе нужно узнать (/normalweather) или изенить погоду (/weather [Название города]) - обращайся ко мне! ";
            }
            else if(message == "/weather")
            {
                respone = "Прости, но я не смогу изменить погоду, не зная места (/weather [Название города])";
            }
            else if((message.IndexOf("/weather") == 0) || (message.IndexOf("/normalweather") == 0))
            {
                bool JoJo = true;
                int subStrInt = 0;
                if(message.IndexOf("/normalweather") == 0)
                {
                    JoJo = false;
                    subStrInt = 6;
                }


                string townName = message.Substring(9 + subStrInt);
                tg_response = await client.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={townName}&appid={apiKey}&lang=ru&units=metric");

                string beforeMassage = "Хм, посмотрим что я могу сделать..";
                await client.GetAsync($"https://api.telegram.org/bot{telegramToken}/sendMessage?chat_id={data.message.chat.id}&text={beforeMassage}");

                if (tg_response.IsSuccessStatusCode)
                {
                    var weatherData = JsonConvert.DeserializeObject<WeatherReport>(tg_response.Content.ReadAsStringAsync().Result);
                    if(JoJo)
                        respone = SetResponse(weatherData);
                    else
                        respone = SetNormalResponse(weatherData);
                }
                else
                {
                    respone = "Прости, но это место вне радиуса моего станда";
                }   
            }
            else
            {
                respone = "Прости, не могу тебя понять. Может тебе надо где-то установить погоду? (/weather [Город])";
            }

            
            await client.GetAsync($"https://api.telegram.org/bot{telegramToken}/sendMessage?chat_id={data.message.chat.id}&text={respone}");
        }

        if (tg_data.result.Length > 0)
        {
            offset = tg_data.result[^1].update_id + 1;
        }
    }

    System.Threading.Thread.Sleep(500);
}

static string SetResponse(WeatherReport weatherData)
{
    string message = "";

    message += $"В месте \"{weatherData.name}\" я смог установить температуру {weatherData.main.temp}. Будет ощущаться как {weatherData.main.feels_like} из-за влажности в {weatherData.main.humidity}ед.";
    message += $"\n"; // Отделительный пропуск
    message += $"\nТак-же сделал так, чтоб в городе было {weatherData.weather[0].description}, а ветер летел со скоростью {weatherData.wind.speed} в сторону {GetDirectByDeg(weatherData.wind.deg)}";
    message += $"\n"; // Отделительный пропуск
    message += $"\nНа посоедок установил давление рт.ст на отметке {weatherData.main.pressure}. Всегда рад помочь";

    return message;
}

static string GetDirectByDeg(int deg)
{
    string[] directions = { "Севера", "Северо-Запада", "Запада", "Юго-Запада", "Юга", "Юго-Востока", "Востока", "Северо-Востока", "Севера", "-" };
    double totalDeg = 360 - 22.5 - deg;
    int i = 0;

    while ((totalDeg > 0) && (i < 9))
    {
        i++;
        totalDeg -= 45;
    }

    return directions[i];
}

static string SetNormalResponse(WeatherReport weatherData)
{
    string message = "";

    message += $"[ | {weatherData.name} | ]"; // Название города
    message += $"\n"; // Отделительный пропуск
    message += $"\n| Текущая температура: {weatherData.main.temp}";
    message += $"\n| Ощщается как: {weatherData.main.feels_like}";
    message += $"\n| Влажность: {weatherData.main.humidity}";

    message += $"\n"; // Отделительный пропуск
    message += $"\n| Описание: {weatherData.weather[0].description}";
    message += $"\n| Скорость ветра: {weatherData.wind.speed}";
    message += $"\n| Направление: {GetMinDirectByDeg(weatherData.wind.deg)}";

    message += $"\n"; // Отделительный пропуск
    message += $"\n| Давление рт.ст: {weatherData.main.pressure}";


    return message;
}

static string GetMinDirectByDeg(int deg)
{
    string[] directions = { "С", "СЗ", "З", "ЮЗ", "Ю", "ЮВ", "В", "СВ", "С", "-" };
    double totalDeg = 360 - 22.5 - deg;
    int i = 0;

    while ((totalDeg > 0) && (i < 9))
    {
        i++;
        totalDeg -= 45;
    }

    return directions[i];
}