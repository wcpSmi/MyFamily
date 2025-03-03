using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MyFamily
{
    public static class MessageHandler
    {

        public static bool IsLocationQuestion(Message message)
        {
            return message.Text.Substring(0, 3) == "???";
        }

        public static bool IsLocationAnswer(Message message)
        {
                return message.Text.Substring(0, 3) == "-!-";
        }
        public static bool IsSOS(Message message)
        {
			return message.Text.Substring(0,5)=="S.O.S";
        }


        public static Message JsonStrToMessage(string jsonStr)
        {
            Message newMessage = new Message();
            newMessage = ParseJSON(ConvertJsonStringToJsonObject(jsonStr));
            return newMessage;
        }

        public static Message ParseJSON(JsonObject obj)
        {
            Debug.Print(obj.ToJsonString());
            Message newMessage = new Message();

            if (obj != null)
            {
                try
                {
                    newMessage.SenderName = obj["senderName"].GetValue<string>();
                    newMessage.SenderUUID = obj["senderUUID"].GetValue<string>();
                    newMessage.Text = obj["text"].GetValue<string>();
                    newMessage.Date = obj["date"].GetValue<string>();
                    newMessage.Host = obj["host"].GetValue<string>();
					newMessage.BackColor = Color.FromRgba(obj["backColor"].GetValue<string>());
                    newMessage.CanSwipe = obj["canSwipe"].GetValue<bool>();
					// A Location objektum beállítása alapértelmezett értékekkel, ha nincs adat vagy a location null értékű
					if (obj.ContainsKey("location") && obj["location"] != null)
                    {
                        var locationObj = obj["location"].AsObject();
                        newMessage.Location = new Location
                        {
                            Longitude = locationObj.ContainsKey("longitude") && locationObj["longitude"] != null
                                        ? locationObj["longitude"].GetValue<double>() 
                                        : 0.0,

                            Latitude = locationObj.ContainsKey("latitude") && locationObj["latitude"] != null
                                       ? locationObj["latitude"].GetValue<double>()   
                                       : 0.0
                        };
                    }
                    else
                    {
                        newMessage.Location = new Location { Longitude = 0.0, Latitude = 0.0 };
                    }

                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    newMessage.SenderName = "Unknow";

                }


            }
            return newMessage;

        }

        public static JsonObject ConvertJsonStringToJsonObject(string jsonStr)
        {
            try
            {
                string receivedJsonString = jsonStr; 

                Debug.Print(receivedJsonString);
                JsonObject parsedObject;
                try
                {
                    parsedObject = JsonNode.Parse(receivedJsonString) as JsonObject;
                }
                catch
                {
                    // Készítse el a JsonObject-t a üres értékekkel
                    parsedObject = new JsonObject
                    {
                        ["senderName"] = "Unknow",
                        ["senderUUID"] = "00000000-0000-0000-0000-000000000000",
                        ["text"] = "<-?->",
                        ["date"] = "",
                        ["host"]="",
                        ["backColor"]=Colors.LightGray.ToHex(),
                        ["canSwipe"]=false,
                        ["location"] = new JsonObject
                        {
                            ["longitude"] = 0.0,
                            ["latitude"] = 0.0
                        }
                    };
                }

                return parsedObject;
            }
            catch (Exception)
            {

                //throw new Exception("A JsonString convertálása JsonObjectre nem sikerült !");
                throw;
            }
        }

        public static string ToJsonText(Message message)
        {
            var jsonObject = ToJSON(message); // Várakozás a ToJSON metódus befejezésére
            Debug.Print("ToJsonText: " + jsonObject.ToString());
            return jsonObject.ToString(); // A JsonObject eredmény használata
        }

        private static JsonObject ToJSON(Message message)
        {

            //throw new NotImplementedException();
            return new JsonObject(new Dictionary<string, JsonNode?>()
            {
                { "senderName", message.SenderName != null ? JsonValue.Create(message.SenderName) : JsonValue.Create(string.Empty) },
                { "senderUUID", message.SenderUUID != null ?  JsonValue.Create(message.SenderUUID) : JsonValue.Create(string.Empty)},
                { "text",message.Text != null ?  JsonValue.Create(message.Text): JsonValue.Create(string.Empty)},
                { "date",message.Date != null ?  JsonValue.Create(message.Date): JsonValue.Create(string.Empty)},
                { "host", message.Host != null ?  JsonValue.Create(message.Host) : JsonValue.Create(string.Empty)},
                { "backColor", message.BackColor != null ?  JsonValue.Create(message.BackColor.ToHex()) : JsonValue.Create(string.Empty)},
                {"canSwipe", message.CanSwipe !=null ? JsonValue.Create(message.CanSwipe) : JsonValue.Create(false) },
                { "location",  message.Location != null ? new JsonObject
                    {
                        { "longitude",  JsonValue.Create(message.Location.Longitude) } ,
                        { "latitude",JsonValue.Create(message.Location.Latitude) },
                    }:null
                }
            });
         }

		public static Message GetLocationQueryMessage()
		{
			var message = new Message
			{
				Text = "???",
				SenderUUID = AppSetting.UUID,
				SenderName = AppSetting.UserName,
				Date = DateTime.Now.ToString(),
				Host = AppSetting.UUID //Akinek majd a válaszokat kell küldeni 
			};
			return message;
		}

		public static async Task<Message> GetAlarmMessage(Message selectedMessage)
		{

			var locationData = await LocationHandler.LocationData();

			var message = new Message
			{
				Text = $"S.O.S !\n{locationData.Item1}",
				SenderUUID = AppSetting.UUID,
				SenderName = AppSetting.UserName,
				Date = DateTime.Now.ToString(),
				Location = locationData.Item2,
				Host = selectedMessage.SenderUUID, //beállítjuk az SOS üzenetünk címzetjének, a kiválaszott 
				BackColor = Colors.Red,
				CanSwipe = false
			};

			return message;

		}
	}
}
