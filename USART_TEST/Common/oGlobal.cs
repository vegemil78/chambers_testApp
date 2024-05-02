using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util;

namespace DEVICE_SETTING_PROGRAM
{
    delegate void delChangeText(string strMsg);
    delegate void delUpdateListView();


    class oGlobal
    {
        public static string display1_str = "";

        public static string test_str = "";
        public static string test_str2 = ""; 
        public static string gateway_set_gui_trig_str = "";
        public static string nodepow_set_gui_trig_str = "";
        public static string nodecom_set_gui_trig_str = "";
        public static string noderel_set_gui_trig_str = "";
        public static string nodeair_set_gui_trig_str = "";
        public static string nodeair2_set_gui_trig_str = "";
        public static string[] nodeid_item = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        public static string[] node_type_item = { "NONE", "POW", "COM", "REL", "AIR", "AIR2", "RELA" ,"RELB" };
        public static string[] wifi_lan_sel_item = { "WIFI", "LAN" };
        public static string[] rf_485_sel_item = { "RF", "485" };
        public static string[] gateway_single_sel_item = { "GATEWAY", "SINGLE" };
        public static string[] bypass_control_sel_item = { "BYPASS", "CONTROL" };
        public static string[] relay_off_on_item = { "OFF", "ON" };
        public static int usScreen_thread_enable_num = 0;
        public static string[] cur_volt_sel_item = { "CUR", "VOLT" };
    }

}
