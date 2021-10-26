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
        private static string eventName = "";
        public static bool isRunning = false;
        private static object token = null;
        public static async void PostAsync(float _red, float _green, float _blue, float _brightness, string _eventName)
        {
            string json = $"{{\"red\": {_red}, \"green\": {_green}, \"blue\": {_blue}, \"bright\": {_brightness}, \"event\": \"{_eventName}\"}}";
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
                if(InGameUI.I.mState != InGameUI.State.PausePage)
                {
                    PostAsync(red, green, blue, brightness, eventName);
                }
                yield return new WaitForSecondsRealtime(.04f);
            }
        }

        private static void Start()
        {
            isRunning = true;
            brightness = RenderSettings.skybox.GetFloat("_Exposure") * 255f;
            eventName = "";
            token = MelonCoroutines.Start(StartPost());
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

        public static void SetEvent(string _event)
        {
            if (!Config.postToApi) return;
            eventName = _event;
            if (!isRunning)
            {
                Start();
            }
        }

        public static void TurnOff()
        {
            if (!Config.postToApi) return;
            brightness = 0f;
            red = 255f;
            green = 255f;
            blue = 255;
            red = green = blue = 255f;
            eventName = "";
            isRunning = false;
            MelonCoroutines.Stop(token);
            PostAsync(red, green, blue, brightness, eventName);
        }
    }
}
