using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MyTelegramBot.Statuses;
using MyTelegramBot.GetArtistInfo;
using MyTelegramBot.GetTrack;
using MyTelegramBot.GetTopTracks;

namespace MyTelegramBot
{
    class Program
    {
        static TelegramBotClient Bot;
        private const string stpath = @"DBst.json";
        static void Main(string[] args)
        {
            Bot = new TelegramBotClient("1117267207:AAEyCpdw2vzfU9goeLzVHFl7xnH0NiGwafE");

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnCallbackQuery += Bot_OnCallbackQueryReceived;

            var me = Bot.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);
            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private async static void Bot_OnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            //для консоли
            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName} (id:{e.CallbackQuery.From.Id})";
            Console.WriteLine(name + " pressed the button " + buttonText);


            ClassStatusList st = JsonConvert.DeserializeObject<ClassStatusList>(File.ReadAllText(stpath));

            UserStatus usrstatus = new UserStatus();
            int index = 0;
            bool contains = false;
            usrstatus.Id = Convert.ToString(e.CallbackQuery.From.Id);
            var message = e.CallbackQuery.Data;

            for (int i = 0; i < st.statuses.Count; i++)
            {
                if (st.statuses[i].Id == usrstatus.Id)
                {
                    contains = true;
                    usrstatus = st.statuses[i];
                    index = i;
                    break;
                }
            }

            if (contains == false)
            {
                usrstatus.status1 = 0;
                usrstatus.status2 = 0;
                usrstatus.Id = Convert.ToString(e.CallbackQuery.From.Id);
                st.statuses.Add(usrstatus);
                index = st.statuses.IndexOf(usrstatus);
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Введите команду.");
                File.WriteAllText(stpath, JsonConvert.SerializeObject(st));
            }

            switch (st.statuses[index].status2)
            {
                case 0:
                    {
                        await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Введите, пожалуйста, команду.");
                        break;
                    }
                case 1:
                    { //addfavorite
                        usrstatus.status1 = 0;
                        usrstatus.status2 = 1;
                        usrstatus.Id = Convert.ToString(e.CallbackQuery.From.Id);
                        st.statuses[index] = usrstatus;
                        File.WriteAllText(stpath, JsonConvert.SerializeObject(st));

                        string[] mass = e.CallbackQuery.Data.Split('⁜');
                        User userrr = new User();
                        userrr.Id = mass[0];
                        userrr.Name = mass[1];
                        userrr.Artist = mass[2];
                        var json = JsonConvert.SerializeObject(userrr);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");


                        HttpClient client = new HttpClient();
                        var response = await client.PostAsync($"https://mymusicbot.azurewebsites.net/api/Music/AddFavorite", data);
                        string result = response.Content.ReadAsStringAsync().Result;

                        if (result == "BAD")
                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Данный трек уже находится в списке избранных.");
                        else
                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Трек был успешно добавлен в список избранных.");

                        break;
                    }
                case 2:
                    {
                        usrstatus.status1 = 0;
                        usrstatus.Id = Convert.ToString(e.CallbackQuery.From.Id);
                        st.statuses[index] = usrstatus;
                        File.WriteAllText(stpath, JsonConvert.SerializeObject(st));

                        //deletefavorite
                        string[] mass = e.CallbackQuery.Data.Split('⁜');
                        User userrr = new User();
                        userrr.Id = mass[0];
                        userrr.Name = mass[1];
                        var json = JsonConvert.SerializeObject(userrr);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");


                        HttpClient client = new HttpClient();
                        var response = await client.PutAsync($"https://mymusicbot.azurewebsites.net/api/Music/DeleteFavorite", data);
                        string result = response.Content.ReadAsStringAsync().Result;

                        if (result == "BAD")
                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Данного трека нет в списке избранных.");
                        else
                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Трек был успешно удалён из списка избранных.");
                        break;
                    }
                default:
                    break;
            }
        }

        private async static void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text)
                return;
            string name = $"{message.From.FirstName } {message.From.LastName}";
            Console.WriteLine($"{name} sent text: '{message.Text}'");


            User userrr = new User();
            ClassStatusList st = JsonConvert.DeserializeObject<ClassStatusList>(File.ReadAllText(stpath));

            int index = 0;
            bool contains = false;
            UserStatus usrstatus = new UserStatus();


            for (int i = 0; i < st.statuses.Count; i++)
            {
                if (st.statuses[i].Id == Convert.ToString(message.From.Id))
                {
                    contains = true;
                    usrstatus = st.statuses[i];
                    index = i;
                    break;
                }
            }

            if (contains == false)
            {
                usrstatus.status1 = 0;
                usrstatus.status2 = 0;
                usrstatus.Id = Convert.ToString(message.From.Id);
                st.statuses.Add(usrstatus);
            }



            switch (st.statuses[index].status1)
            {
                case 0:
                    {
                        switch (message.Text)
                        {
                            case "/start":
                                {
                                    string text =
            @"Список команд:
/gettoptracks - Топ-10 треков
/gettrack - Найти трек по названию
/getartistinfo - Информация об исполнителе
/getfavorite - Список избранных";

                                    await Bot.SendTextMessageAsync(message.From.Id, text);

                                    usrstatus.status1 = 0;
                                    usrstatus.status2 = 0;
                                    usrstatus.Id = Convert.ToString(message.From.Id);
                                    st.statuses[index] = usrstatus;
                                    File.WriteAllText(stpath, JsonConvert.SerializeObject(st));
                                    break;
                                }
                            case "/gettoptracks":
                                {
                                    var json = JsonConvert.SerializeObject(userrr);
                                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                                    userrr.Id = Convert.ToString(message.From.Id);

                                    HttpClient client = new HttpClient();
                                    var response = await client.PostAsync($"https://mymusicbot.azurewebsites.net/api/Music/GetTopTracks", data);
                                    string result = response.Content.ReadAsStringAsync().Result;
                                    ClassTracks useritem = JsonConvert.DeserializeObject<ClassTracks>(result);

                                    List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();


                                    foreach (var track in useritem.tracks.track)
                                    {
                                        //if (track.url.Length <= 60)
                                        {
                                            List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                            ts.Add(InlineKeyboardButton.WithUrl(track.name + " - " + track.artist.name, track.url));
                                            ts.Add(InlineKeyboardButton.WithCallbackData("+", Convert.ToString(message.From.Id) + "⁜" + track.name + "⁜" + track.artist.name));
                                            inlineKeyboardList.Add(ts);
                                        }
                                    }
                                    var inline = new InlineKeyboardMarkup(inlineKeyboardList);

                                    await Bot.SendTextMessageAsync(message.From.Id, "Топ-10 треков:", replyMarkup: inline);


                                    usrstatus.status1 = 0;
                                    usrstatus.status2 = 1;
                                    st.statuses[index] = usrstatus;

                                    File.WriteAllText(stpath, JsonConvert.SerializeObject(st));

                                    break;
                                }
                            case "/getartistinfo":
                                {
                                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Введите исполнителя");

                                    st.statuses[index].status1 = 1;
                                    File.WriteAllText(stpath, JsonConvert.SerializeObject(st));

                                    break;
                                }
                            case "/gettrack":
                                {
                                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Введите название песни");


                                    st.statuses[index].status1 = 2;
                                    File.WriteAllText(stpath, JsonConvert.SerializeObject(st));

                                    break;
                                }
                            case "/getfavorite":
                                {
                                    userrr.Id = Convert.ToString(message.From.Id);

                                    var json = JsonConvert.SerializeObject(userrr);
                                    var data = new StringContent(json, Encoding.UTF8, "application/json");


                                    HttpClient client = new HttpClient();
                                    var response = await client.PostAsync($"https://mymusicbot.azurewebsites.net/api/Music/GetFavorite", data);
                                    string result = response.Content.ReadAsStringAsync().Result;

                                    if (result == "BAD")
                                    {
                                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Списка нету");
                                        break;
                                    }


                                    List<SearchTrackMain> js = JsonConvert.DeserializeObject<List<SearchTrackMain>>(result);

                                    List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();


                                    foreach (var track in js)
                                    {
                                        List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                        ts.Add(InlineKeyboardButton.WithUrl(track.name + " - " + track.artist, track.url));
                                        ts.Add(InlineKeyboardButton.WithCallbackData("х", Convert.ToString(message.From.Id) + "⁜" + track.name + "⁜" + track.artist));
                                        inlineKeyboardList.Add(ts);
                                    }
                                    var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                                    if (js.Count != 0)
                                        await Bot.SendTextMessageAsync(message.From.Id, "Список фаворитов:", replyMarkup: inline);
                                    else
                                        await Bot.SendTextMessageAsync(message.From.Id, "Список фаворитов пуст.");

                                    usrstatus.status1 = 0;
                                    usrstatus.status2 = 2;
                                    st.statuses[index] = usrstatus;

                                    File.WriteAllText(stpath, JsonConvert.SerializeObject(st));

                                    break;

                                }
                            default:
                                {
                                    break;
                                }

                        }
                        break;
                    }
                case 1: //для getartistinfo
                    {
                        try
                        {
                            st.statuses[index].status1 = 0;
                            File.WriteAllText(stpath, JsonConvert.SerializeObject(st));

                            userrr.Id = Convert.ToString(message.From.Id);
                            userrr.Artist = e.Message.Text;

                            var json = JsonConvert.SerializeObject(userrr);
                            var data = new StringContent(json, Encoding.UTF8, "application/json");

                            HttpClient client = new HttpClient();
                            var response = await client.PostAsync($"https://mymusicbot.azurewebsites.net/api/Music/GetArtistInfo", data);
                            string result = response.Content.ReadAsStringAsync().Result;
                            ClassListArtistInfo useritem = JsonConvert.DeserializeObject<ClassListArtistInfo>(result);

                            await Bot.SendTextMessageAsync(message.From.Id, $"Artist: {useritem.artist.name}\n\nBio: {useritem.artist.bio.summary}.");

                            break;
                        }
                        catch
                        {
                            await Bot.SendTextMessageAsync(message.From.Id, "Было введено некорректное имя исполнителя.");
                            break;
                        }
                    }
                case 2: //для gettrack
                    {

                        st.statuses[index].status1 = 0;
                        st.statuses[index].status2 = 1;

                        userrr.Id = Convert.ToString(message.From.Id);
                        userrr.Name = e.Message.Text;

                        var json = JsonConvert.SerializeObject(userrr);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");


                        HttpClient client = new HttpClient();
                        var response = await client.PostAsync($"https://mymusicbot.azurewebsites.net/api/Music/GetTrack", data);
                        string result = response.Content.ReadAsStringAsync().Result;
                        ClassResults useritem = JsonConvert.DeserializeObject<ClassResults>(result);

                        List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();


                        foreach (var track in useritem.results.trackmatches.track)
                        {
                            if (track.url.Length <= 60)
                            {
                                List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                ts.Add(InlineKeyboardButton.WithUrl(track.name + " - " + track.artist, track.url));
                                ts.Add(InlineKeyboardButton.WithCallbackData("+", Convert.ToString(message.From.Id) + "⁜" + track.name + "⁜" + track.artist));
                                inlineKeyboardList.Add(ts);
                            }
                        }
                        var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                        if (result == "It's bad")
                            await Bot.SendTextMessageAsync(message.From.Id, "Ничего не найдено. Попробуйте ввести название по-другому.");
                        else
                            await Bot.SendTextMessageAsync(message.From.Id, "Найденные треки:", replyMarkup: inline);
                        File.WriteAllText(stpath, JsonConvert.SerializeObject(st));

                        break;
                    }
                default:
                    {
                        goto case 0;
                    }
            }
        }
    }
}