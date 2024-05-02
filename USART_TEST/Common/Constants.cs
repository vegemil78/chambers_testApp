using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DEVICE_SETTING_PROGRAM
{
    static class Constants
    {
        public const int MOD_RESET = 1;
        public const int MOD_COMPRESSION_VALVE = 2;
        public const int MOD_COMPRESSION_SHUTOFF_VALVE = 3;
        public const int MOD_DECOMPRESSION_VALVE = 4;
        public const int MOD_BREATHING_GAS_SELECT_VALVE = 5;
        public const int MOD_RESIDUAL_PRESSURE_REMOVE_VALVE = 6;
        public const int MOD_DRAIN_VALVE = 7;
        public const int MOD_ENTRANCEDOOR_LEFTANDRIGHT_VALVE = 8;
        public const int MOD_ENTRANCEDOOR_TOPANDBOTTOM_VALVE = 9;
        public const int MOD_EMERGENCY_EXHAUST_VALVE = 10;
        public const int MOD_FIRE_EXTTINGUISHING_WATER = 11;

        public const int  MOD_ENTRANCEDOOR_LEFTANDRIGHT_CHECK = 101;
        public const int  MOD_ENTRANCEDOOR_TOPANDBOTTOM_CHECK = 102;
        public const int  MOD_SAFETY_PHOTO_SENSOR = 103;
        public const int  MOD_SAFETY_MOTION_DETECTION1_SENSOR = 104;
        public const int  MOD_SAFETY_MOTION_DETECTION2_SENSOR = 105;

        public const int  MOD_SCRUBBER_FAN_VOLUME = 301;
        public const int  MOD_HEATINGANDCOOLING_FAN_VOLUME = 302;
        public const int  MOD_HEATINGANDCOOLING_SELECT = 303;
        public const int  MOD_LIGHT_VOLUME = 304;

        public const int MOD_TEMPERATURE_SET = 315;
        public const int MOD_TEMPERATURE_DEVIATION = 316;

        public const int  MOD_STATUS_LED = 305;
        public const ushort  BIT_COMPRESSION_VALVE = 0x0001;
        public const ushort BIT_COMPRESSION_SHUTOFF_VALVE = 0x0002;
        public const ushort BIT_DECOMPRESSION_VALVE = 0x0004;
        public const ushort BIT_BREATHING_GAS_SELECT_VALVE = 0x0008;
        public const ushort BIT_RESIDUAL_PRESSURE_REMOVE_VALVE = 0x0010;
        public const ushort BIT_DRAIN_VALVE = 0x0020;
        public const ushort BIT_ENTRANCEDOOR_LEFTANDRIGHT_VALVE = 0x0040;
        public const ushort BIT_ENTRANCEDOOR_TOPANDBOTTOM_VALVE = 0x0080;
        public const ushort BIT_EMERGENCY_EXHAUST_VALVE = 0x0100;
        public const ushort BIT_FIRE_EXTTINGUISHING_WATER = 0x0200;
        public const ushort BIT_ENTRANCEDOOR_LEFTANDRIGHT_CHECK = 0x0400;
        public const ushort BIT_ENTRANCEDOOR_TOPANDBOTTOM_CHECK = 0x0800;
        public const ushort BIT_SAFETY_PHOTO_SENSOR = 0x1000;
        public const ushort BIT_SAFETY_MOTION_DETECTION1_SENSOR = 0x2000;
        public const ushort BIT_SAFETY_MOTION_DETECTION2_SENSOR = 0x4000;

        public const int MOD_PRESSURE_SENSOR = 307;
        public const int MOD_OXYGEN_SENSOR = 308;
        public const int MOD_CARBON_DIOXIDE_SENSOR = 309;
        public const int MOD_TEMPERATURE_SENSOR = 310;
        public const int MOD_HUMIDITY_SENSOR = 311;
        public const int MOD_SPARE1_SENSOR = 312;
        public const int MOD_SPARE2_SENSOR = 313;
        public const int MOD_SPARE3_SENSOR = 314;


    }
}
