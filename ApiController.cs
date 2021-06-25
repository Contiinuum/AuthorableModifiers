using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using UnityEngine;
using MelonLoader;
using System.Web.Script.Serialization;
using System.Collections;
using System.Net;

namespace AuthorableModifiers
{
    public static class ApiController
    {
        private static float brightness = RenderSettings.skybox.GetFloat("_Exposure") * 255f;
        private static float red, green, blue = 255f;
        public static bool isRunning = false;
        public static async void PostAsync(float _red, float _green, float _blue, float _brightness)
        {
            string json = $"{{\"red\": {_red}, \"green\": {_green}, \"blue\": {_blue}, \"bright\": {_brightness}}}";
            try
            {
                HttpClient client = new HttpClient();
                await client.PostAsync(
                Config.apiUrl,
                new StringContent(json, Encoding.UTF8, "application/json"));                              
            }
            catch(HttpRequestException ex)
            {
                MelonLogger.Msg(ex.InnerException.Message);
            }
           
        }

        public static IEnumerator StartPost()
        {
            while (isRunning)
            {
                PostAsync(red, green, blue, brightness);
                yield return new WaitForSecondsRealtime(.04f);
            }
        }

        private static void Start()
        {
            isRunning = true;
            brightness = RenderSettings.skybox.GetFloat("_Exposure") * 255f;
            MelonCoroutines.Start(StartPost());
        }

        public static void SetBrightness(float _brightness)
        {
            if (!Config.postToApi) return;
            brightness = _brightness * 255f;
            if (!isRunning)
            {
                Start();
            }
        }

        public static void SetColor(Color color)
        {
            if (!Config.postToApi) return;
            red = color.r * 255f;
            green = color.g * 255f;
            blue = color.b * 255f;
            if (!isRunning)
            {
                Start();
            }
        }

        public static void TurnOff()
        {
            if (!Config.postToApi) return;
            brightness = 0f;
            red = green = blue = 255f;
            isRunning = false;
            PostAsync(red, green, blue, brightness);
        }
    }
}
