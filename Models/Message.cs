using Microsoft.Maui.ApplicationModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

/* Unmerged change from project 'MyFamily (net8.0-windows10.0.19041.0)'
Before:
#if ANDROID
After:
using MyFamily;
using MyFamily.Models;
#if ANDROID
*/

#if ANDROID
using Android.App;
#endif

namespace MyFamily
{
    public class Message
    {
        public string SenderName { get; set; }
        public string SenderUUID { get; set; }
        public string Text { get; set; }
        public string Date { get; set; }
        public string Host { get; set; }
        public Location? Location { get; set; }
        public Color BackColor { get; set; } 
        public bool CanSwipe { get; set; }

        public Message()
        {
            BackColor = Colors.LightGray;
		}
        public void Clear()
        {
            SenderName = string.Empty;
            SenderUUID = string.Empty;
            Text = string.Empty;
            Date = string.Empty;
            Location = null;
            CanSwipe = true;
        }




	}


}