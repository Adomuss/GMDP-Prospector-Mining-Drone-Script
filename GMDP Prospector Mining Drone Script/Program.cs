using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        //program start
        //Mining controller spotter drone V0.322A
        #region mdk preserve
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        int drone_id = 1;
        string drone_tag = "SWRM_D";
        string scout_tag = "PSMD";
        string main_display_tag = "D1";
        string interface_display_tag = "D2";

        double safe_position = 30.0;
        double free_center_position = 20000.0;
        double raycast_scan_distance = 32.0;
        //statics        
        string scan_cmd = "scan";
        string reset_cmd = "reset";
        string send_cmd = "send";
        string ast_en_cmd = "asten";
        string ast_dis_cmd = "astdis";
        string retry_send_cmd = "retx";
        string free_form_en_cmd = "freeen";
        string free_form_dis_cmd = "freedis";
        string confirmval = "confirm";
        string increase = "incrval";
        string decrease = "decrval";
        string incrsel = "incrsel";
        string itemdown = "itemdown";
        string itemup = "itemup";
        string menureturn = "menu";
        string command = "command";
        string jobconf = "jobconf";
        string cancelcommand = "cancel";

        int lcd_display_index_main = 0; //used for devices with multiple screen panels (0+) 
        int lcd_display_index_interface = 0; //used for devices with multiple screen panels (0+) 
        #endregion
        string ver = "V0.322";
        string drone_id_name = "";
        string tx_channel = "";
        string light_transmit_tag = "";
        string light_target_tag = "";
        string main_display_name = "";
        string interface_display_name = "";
        string txl = "TX";
        string tgt = "TGT";
        string prospC = "prospector";
        string scan_camera = "scan";        
        string intfs = "Interface";
        string postfix = "Display";
        float t_stored_power;
        float stored_power_total;
        float t_max_power;
        float max_power_total;
        float t_current_power;
        float current_power_total;
        float font_zoom = 1.0f;

        bool scan_complete = false;
        bool surface_found = false;
        bool asteroidsDetected = false;
        bool transmit_complete = false;
        bool free_form = false;
        bool enable_asteroid_detection;
        double distance_scan = 0.0;
        string data_out;
        string temp_id_scout;
        string temp_id_name;
        int temp_id_num;
        int stateshift = 0;
        string icon = "";
        int scan_type = 0;
        string scan_type_display = "";
        float percent_battery_power = 0.0f;


        IMyRadioAntenna antenna_actual;
        IMySensorBlock sensor_actual;
        IMyCameraBlock camera_actual;
        IMyRemoteControl remote_control_actual;
        IMyBatteryBlock currentbatteryblock;
        IMyLightingBlock target_aquired_light_actual;
        IMyLightingBlock target_transmit_light_actual;

        Vector3D surface_coords;
        Vector3D target_coords;
        Vector3D asteroid_coords;
        Vector3D camera_coords;
        Vector3D free_centre_target_coords;
        Vector3D gravity;
        Vector3D TargetVec;

        List<IMyRadioAntenna> antenna_all;
        List<IMyRadioAntenna> antenna_tag;
        List<IMyBatteryBlock> batteries_all;
        List<IMyBatteryBlock> batteries_tag;
        List<IMyRemoteControl> remote_control_all;
        List<IMyRemoteControl> remote_control_tag;
        List<IMySensorBlock> sensor_all;
        List<IMySensorBlock> sensor_tag;
        List<IMyCameraBlock> camera_all;
        List<IMyCameraBlock> camera_tag;
        List<IMyCameraBlock> camera_scan;
        List<IMyLightingBlock> lighting_all;
        List<IMyLightingBlock> lighting_target_aquired;
        List<IMyLightingBlock> lighting_target_transmit;
        List<IMyThrust> thrust_all;
        List<IMyThrust> thrust_tag;
        List<IMyShipConnector> connector_all;
        List<IMyShipConnector> connector_tag;
        List<IMyTerminalBlock> display_all;
        List<IMyTerminalBlock> display_tag_main;
        List<IMyTerminalBlock> display_tag_interface;
        IMyTextSurface display_surface_main;
        IMyTextSurface display_surface_interface;
        StringBuilder display_string_interface;
        StringBuilder display_string_main;

        bool setup_complete = false;
        bool interface_presence = false;

        //secondary menu stuff
        List<string> item_line_0;
        List<string> item_line_1;
        List<string> item_line_2;
        List<string> item_line_3;
        List<string> item_line_4;
        List<string> item_line_5;
        List<string> item_line_6;
        List<string> item_line_7;
        List<string> item_line_8;
        List<string> item_line_9;
        List<string> item_line_10;
        List<string> scroll_command_item;
        List<string> scroll_command_item_2;
        int scroll_item_val = 0;
        int scroll_item_val_min_limit = 0;
        int scroll_item_val_max_limit = 2;
        int scroll_item_val_2 = 0;
        int scroll_item_val_min_limit_2 = 0;
        int scroll_item_val_max_limit_2 = 3;
        string line_highlight_0 = "[ ]";
        string line_highlight_1 = "[ ]";
        string line_highlight_2 = "[ ]";
        string line_highlight_3 = "[ ]";
        string line_highlight_4 = "[ ]";
        string line_highlight_5 = "[ ]";
        string line_highlight_6 = "[ ]";
        string line_highlight_7 = "[ ]";
        string line_highlight_8 = "[ ]";
        string line_highlight_9 = "[ ]";
        string line_highlight_10 = "[ ]";

        int menu_level = 0;
        int item_number = 0;
        int iteration_val = 0;
        string last_command = "";
        string disp_command = "";
        string iteration_view = "";
        string cancel_display = "";
        string menu_display = "";
        string displayconfirm_1 = "";
        string displayconfirm_2 = "";
        string displayconfirm_3 = "";

        bool item_up = false;
        bool item_down = false;
        bool has_iterated = false;
        bool has_increased = false;
        bool has_decreased = false;


        int temp_cancel = 0;
        int temp_confirmval_1 = 0;
        bool confirm_sel_1 = false;
        int temp_confirmval_2 = 0;
        bool confirm_sel_2 = false;
        int temp_confirmval_3 = 0;
        bool confirm_sel_3 = false;        

        bool confirm_send = false;
        bool confirm_command = false;
        int temp_menu = 0;        

        double temp_drillshaft_length = 00.0;
        double new_drillshaft_length = 0.0;
        double temp_ignore_depth = 0.0;
        double new_ignore_depth = 0.0;
        int temp_scan_type = 0;
        int new_scan_type = 0;
        string new_scan_type_display = "";
        int temp_command = 0;       
        int interface_command = 0;


        int item_max_limit = 1;
        int item_min_limit = 0;        

        MyIni _Storage = new MyIni();

        

        bool interface_setup_complete = false;



        public void Save()
        {
            _Storage.Set("State", "Safedistance", safe_position);
            _Storage.Set("State", "freecenterposition", free_center_position);
            _Storage.Set("State", "scantype", scan_type);
            Storage = _Storage.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            IMyGridTerminalSystem gts = GridTerminalSystem as IMyGridTerminalSystem;
            if (!setup_complete)
            {
                if (_Storage.TryParse(Storage))
                {
                    var str = _Storage.Get("State", "Safedistance").ToString();
                    double.TryParse(str, out safe_position);
                    str = _Storage.Get("State", "freecenterposition").ToString();
                    double.TryParse(str, out free_center_position);
                    str = _Storage.Get("State", "scantype").ToString();
                    int.TryParse(str, out scan_type);
                    Echo("Storage Loaded");
                }
                else
                {
                    safe_position = 30.0;
                    free_center_position = 20000.0;
                    scan_type = 0;
                    Echo("Default Loaded");
                }
                drone_id_name = "[" + scout_tag + " " + drone_id + "]";
                tx_channel = drone_tag + " " + prospC;
                light_transmit_tag = "[" + scout_tag + " " + drone_id + " " + txl + "]";
                light_target_tag = "[" + scout_tag + " " + drone_id + " " + tgt + "]";
                main_display_name = "[" + scout_tag + " " + drone_id + " " + main_display_tag + "]";
                interface_display_name = "[" + scout_tag + " " + intfs + " " + postfix + "]";
                antenna_all = new List<IMyRadioAntenna>();
                antenna_tag = new List<IMyRadioAntenna>();
                batteries_all = new List<IMyBatteryBlock>();
                batteries_tag = new List<IMyBatteryBlock>();
                remote_control_all = new List<IMyRemoteControl>();
                remote_control_tag = new List<IMyRemoteControl>();
                sensor_all = new List<IMySensorBlock>();
                sensor_tag = new List<IMySensorBlock>();
                camera_all = new List<IMyCameraBlock>();
                camera_tag = new List<IMyCameraBlock>();
                camera_scan = new List<IMyCameraBlock>();
                lighting_all = new List<IMyLightingBlock>();
                lighting_target_aquired = new List<IMyLightingBlock>();
                lighting_target_transmit = new List<IMyLightingBlock>();
                display_all = new List<IMyTerminalBlock>();
                display_tag_main = new List<IMyTerminalBlock>();
                display_tag_interface = new List<IMyTerminalBlock>();
                thrust_all = new List<IMyThrust>();
                thrust_tag = new List<IMyThrust>();
                connector_all = new List<IMyShipConnector>();
                connector_tag = new List<IMyShipConnector>();
                display_string_main = new StringBuilder();

                string n = "";
                //find antennas with tag
                gts.GetBlocksOfType<IMyRadioAntenna>(antenna_all, b => b.CubeGrid == Me.CubeGrid);
                if (antenna_all.Count > 0)
                {
                    for (int i = 0; i < antenna_all.Count; i++)
                    {
                        if (!antenna_all[i].CustomName.Contains(drone_id_name))
                        {
                            string checker = antenna_all[i].CustomData;
                            drone_custom_data_check(checker, i);
                            if (drone_tag == "" || drone_tag == null)
                            {
                                Echo($"Invalid name for drone_tag {drone_tag}. Please add drone tag to antenna e.g. '1:PSMD:SWRM_D', '<drone_id>:<prospector_drone_name>:<drone_group_tag>'");
                                return;
                            }
                            n = $"Antenna {(i + 1)}";
                            antenna_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            antenna_tag.Add(antenna_all[i]);
                        }
                        if (antenna_all[i].CustomName.Contains(drone_id_name))
                        {
                            string checker = antenna_all[i].CustomData;
                            drone_custom_data_check(checker, i);
                            if (drone_tag == "" || drone_tag == null)
                            {
                                Echo($"Invalid name for drone_tag {drone_tag} Please add drone tag to antenna e.g. '1:PSMD:SWRM_D', '<drone_id>:<prospector_drone_name>:<drone_group_tag>'");
                                return;
                            }
                            n = $"Antenna {(i + 1)}";
                            antenna_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            antenna_tag.Add(antenna_all[i]);
                        }

                    }
                }
                antenna_all.Clear();
                // find remote control block
                gts.GetBlocksOfType<IMyRemoteControl>(remote_control_all, b => b.CubeGrid == Me.CubeGrid);
                if (remote_control_all.Count > 0)
                {
                    for (int i = 0; i < remote_control_all.Count; i++)
                    {
                        //create new array from search array with containers matching tag

                        if (remote_control_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Remote Control {(i + 1)}";
                            remote_control_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            remote_control_tag.Add(remote_control_all[i]);
                        }
                        if (!remote_control_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Remote Control {(i + 1)}";
                            remote_control_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            remote_control_tag.Add(remote_control_all[i]);
                        }

                    }
                }
                remote_control_all.Clear();
                //get camera for raycast distance to planet reglar for test
                gts.GetBlocksOfType<IMyCameraBlock>(camera_all, b => b.CubeGrid == Me.CubeGrid);
                if (camera_all.Count > 0)
                {
                    for (int i = 0; i < camera_all.Count; i++)
                    {
                        //create new array from search array with containers matching tag

                        if (camera_all[i].CustomName.Contains(scan_camera))
                        {
                            n = $"Camera {(i + 1)}";
                            camera_all[i].CustomName = n + " " + drone_id_name + " " + scan_camera + " " + "[" + tx_channel + "]";
                            camera_tag.Add(camera_all[i]);
                            camera_scan.Add(camera_all[i]);
                            break;
                        }
                        if (!camera_all[i].CustomName.Contains(scan_camera))
                        {
                            n = $"Camera {(i + 1)}";
                            camera_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            camera_tag.Add(camera_all[i]);
                        }
                    }
                }
                camera_all.Clear();
                //populate array with batteries on grid(s) 

                gts.GetBlocksOfType<IMyBatteryBlock>(batteries_all, b => b.CubeGrid == Me.CubeGrid);
                if (batteries_all.Count > 0)
                {
                    for (int i = 0; i < batteries_all.Count; i++)
                    {
                        if (batteries_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Battery {(i + 1)}";
                            batteries_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            batteries_tag.Add(batteries_all[i]);
                        }
                        if (!batteries_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Battery {(i + 1)}";
                            batteries_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            batteries_tag.Add(batteries_all[i]);
                        }

                    }
                }
                batteries_all.Clear();
                // find remote control block

                //displays
                display_all = new List<IMyTerminalBlock>();
                display_tag_main = new List<IMyTerminalBlock>();
                gts.GetBlocksOfType<IMyTerminalBlock>(display_all, b => b.CubeGrid == Me.CubeGrid);
                if (display_all.Count > 0)
                {
                    for (int i = 0; i < display_all.Count; i++)
                    {
                        if (display_all[i].CustomName.Contains(main_display_tag))
                        {
                            display_all[i].CustomName = $"Prospector Main Display {main_display_tag} {main_display_name} ";
                            display_tag_main.Add(display_all[i]);

                        }
                        if (display_all[i].CustomName.Contains(interface_display_tag))
                        {
                            display_all[i].CustomName = $"Prospector Interface Display {interface_display_tag} {interface_display_name}";
                            display_tag_interface.Add(display_all[i]);
                        }
                    }
                }
                display_all.Clear();

                //populate light lists
                gts.GetBlocksOfType<IMyLightingBlock>(lighting_all, b => b.CubeGrid == Me.CubeGrid);
                if (lighting_all.Count > 0)
                {
                    for (int i = 0; i < lighting_all.Count; i++)
                    {
                        //create new array from search array with lights matching tag
                        if (lighting_all[i].CustomName.Contains(light_transmit_tag) || lighting_all[i].CustomName.Contains(txl))
                        {
                            n = $"Interior light {(i + 1)}";
                            lighting_all[i].CustomName = $"{n} {light_transmit_tag} [{tx_channel}]";
                            lighting_target_transmit.Add(lighting_all[i]);
                            break;
                        }
                    }
                    for (int i = 0; i < lighting_all.Count; i++)
                    {
                        //create new array from search array with lights matching tag
                        if (lighting_all[i].CustomName.Contains(light_target_tag) || lighting_all[i].CustomName.Contains(tgt))
                        {
                            n = $"Interior light {(i + 1)}";
                            lighting_all[i].CustomName = $"{n} {light_target_tag} [{tx_channel}]";
                            lighting_target_aquired.Add(lighting_all[i]);
                            break;
                        }
                    }

                    for (int i = 0; i < lighting_all.Count; i++)
                    {
                        //create new array from search array with lights matching tag
                        if (!lighting_all[i].CustomName.Contains(tgt))
                        {
                            if (!lighting_all[i].CustomName.Contains(txl))
                            {
                                n = $"Interior light {(i + 1)}";
                                lighting_all[i].CustomName = $"{n} {drone_id_name}";
                                lighting_target_aquired.Add(lighting_all[i]);
                            }
                        }
                    }
                }
                lighting_all.Clear();
                //find sensors with tag
                gts.GetBlocksOfType<IMySensorBlock>(sensor_all, b => b.CubeGrid == Me.CubeGrid);
                if (sensor_all.Count > 0)
                {

                    for (int i = 0; i < sensor_all.Count; i++)
                    {
                        if (sensor_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Sensor {(i + 1)}";
                            sensor_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            sensor_tag.Add(sensor_all[i]);
                        }
                        if (!sensor_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Sensor {(i + 1)}";
                            sensor_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            sensor_tag.Add(sensor_all[i]);
                        }

                    }
                }
                sensor_all.Clear();
                gts.GetBlocksOfType<IMyThrust>(thrust_all, b => b.CubeGrid == Me.CubeGrid);
                if (thrust_all.Count > 0)
                {

                    for (int i = 0; i < thrust_all.Count; i++)
                    {
                        if (thrust_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Thruster {(i + 1)}";
                            thrust_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            thrust_tag.Add(thrust_all[i]);
                        }
                        if (!thrust_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Thruster {(i + 1)}";
                            thrust_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            thrust_tag.Add(thrust_all[i]);
                        }
                    }
                }
                thrust_all.Clear();
                gts.GetBlocksOfType<IMyShipConnector>(connector_all, b => b.CubeGrid == Me.CubeGrid);
                if (connector_all.Count > 0)
                {

                    for (int i = 0; i < connector_all.Count; i++)
                    {
                        if (connector_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Connector {(i + 1)}";
                            connector_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            connector_tag.Add(connector_all[i]);
                        }
                        if (!connector_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = $"Connector {(i + 1)}";
                            connector_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                            connector_tag.Add(connector_all[i]);
                        }
                    }
                }
                connector_all.Clear();

                if (display_tag_main.Count > 0)
                {
                    display_surface_main = ((IMyTextSurfaceProvider)display_tag_main[0]).GetSurface(lcd_display_index_main);
                    Echo($"Main display: '{main_display_name}' found.");
                }
                if (display_tag_interface.Count > 0)
                {
                    display_surface_interface = ((IMyTextSurfaceProvider)display_tag_interface[0]).GetSurface(lcd_display_index_interface);
                    Echo($"Interface display: '{interface_display_name}' found.");
                    interface_presence = true;
                }
                else
                {
                    interface_presence = false;
                }
                setup_complete = true;
                Echo("Setup complete!");
            }

            presence_check();

            if (interface_presence)
            {
                if (!interface_setup_complete)
                {
                    Echo("Interface Setup..");
                    item_line_0 = new List<string>();
                    item_line_1 = new List<string>();
                    item_line_2 = new List<string>();
                    item_line_3 = new List<string>();
                    item_line_4 = new List<string>();
                    item_line_5 = new List<string>();
                    item_line_6 = new List<string>();
                    item_line_7 = new List<string>();
                    item_line_8 = new List<string>();
                    item_line_9 = new List<string>();
                    item_line_10 = new List<string>();
                    scroll_command_item = new List<string>();
                    scroll_command_item_2 = new List<string>();
                    display_string_interface = new StringBuilder();

                    //scroll command item text                
                    scroll_command_item.Add("Planetary");
                    scroll_command_item.Add("Asteroid");
                    scroll_command_item.Add("Free Align");
                    scroll_command_item.Add("");

                    //scroll command item text                
                    scroll_command_item_2.Add("---");
                    scroll_command_item_2.Add("Scan");
                    scroll_command_item_2.Add("Send");
                    scroll_command_item_2.Add("Reset");
                    scroll_command_item_2.Add("");

                    //menu text - level 0
                    item_line_0.Add("Mining Job Configuration");
                    item_line_1.Add("Scan Alignment");
                    item_line_2.Add("Command");
                    item_line_3.Add("");
                    item_line_4.Add("");
                    item_line_5.Add("");
                    item_line_6.Add("");
                    item_line_7.Add("");
                    item_line_8.Add("");
                    item_line_9.Add("");
                    item_line_10.Add("");

                    //menu text - level 1
                    item_line_0.Add("Alignment:");
                    item_line_1.Add("---");
                    item_line_2.Add("---");
                    item_line_3.Add("---");
                    item_line_4.Add("---");
                    item_line_5.Add("---");
                    item_line_6.Add("---");
                    item_line_7.Add("Cancel:");
                    item_line_8.Add("Main Menu:");
                    item_line_9.Add("---");
                    item_line_10.Add("Confirm:");

                    //menu text - level 2
                    item_line_0.Add("Surface distance:");
                    item_line_1.Add("Alignment depth:");
                    item_line_2.Add("Aligmnent type:");
                    item_line_3.Add("---");
                    item_line_4.Add("---");
                    item_line_5.Add("---");
                    item_line_6.Add("---");
                    item_line_7.Add("---");
                    item_line_8.Add("Main Menu:");
                    item_line_9.Add("---");
                    item_line_10.Add("Confirm:");

                    //menu text - level 3
                    item_line_0.Add("Command:");
                    item_line_1.Add("---");
                    item_line_2.Add("---");
                    item_line_3.Add("---");
                    item_line_4.Add("---");
                    item_line_5.Add("---");
                    item_line_6.Add("---");
                    item_line_7.Add("Cancel:");
                    item_line_8.Add("Main Menu:");
                    item_line_9.Add("---");
                    item_line_10.Add("Confirm:");
                    menu_level = 0;
                    item_number = 0;

                    interface_setup_complete = true;
                    Echo("Interface Setup Complete!");
                }
            }

            battery_check();

            //Logic Start
            Echo($"GMDP {ver} Running {icon}");

            if (interface_presence)
            {
                #region direct_command_menu_setting
                //menu stuff
                if (argument.Contains(jobconf))
                {
                    menu_level = 2;
                    item_number = 0;
                    iteration_val = 0;
                    argument = "";
                }
                if (argument.Contains(command))
                {
                    menu_level = 1;
                    item_number = 0;
                    iteration_val = 0;
                    argument = "";
                }
                if (argument.Contains(menureturn))
                {
                    menu_level = 0;
                    item_number = 0;
                    scroll_item_val = 0;
                    scroll_item_val_2 = 0;
                    iteration_val = 0;
                    argument = "";
                }
                if (argument.Contains(cancelcommand))
                {
                    menu_level = 0;
                    item_number = 0;
                    iteration_val = 0;
                    scroll_item_val = 0;
                    scroll_item_val_2 = 0;
                    //Me.CustomData = "";
                    last_command = "";
                    argument = "";
                }
                #endregion

                #region itemup_mng
                if (argument.Contains(itemup)) //item index up
                {
                    if (!item_up)
                    {
                        incr_item();
                        item_up = true;
                        argument = "";
                    }
                }
                if (item_up)
                {
                    item_up = false;
                }
                #endregion

                #region itemdown_mng
                if (argument.Contains(itemdown)) //item index down
                {
                    if (!item_down)
                    {
                        decr_item();
                        item_down = true;
                        argument = "";
                    }
                }
                if (item_down)
                {
                    item_down = false;
                }
                #endregion

                #region increase_selection_value
                if (argument.Contains(incrsel))
                {
                    if (menu_level == 0 && !has_iterated)
                    {
                        iteration_val = 0;
                        has_iterated = true;
                    }
                    if (menu_level == 1 && !has_iterated)
                    {
                        iteration_val = 0;
                        has_iterated = true;
                    }
                    if (menu_level == 2)
                    {
                        if (!has_iterated && !has_iterated)
                        {
                            iteration_val++;
                            has_iterated = true;
                        }
                        if (iteration_val > 4)
                        {
                            iteration_val = 0;
                            has_iterated = true;
                        }
                    }
                    if (menu_level == 3 && !has_iterated)
                    {
                        iteration_val = 0;
                        has_iterated = true;
                    }
                    argument = "";
                }
                if (has_iterated)
                {
                    has_iterated = false;
                }
                #endregion

                #region increase_menu_management
                if (argument.Contains(increase))
                {
                    if (!has_increased)
                    {
                        if (iteration_val == 0)
                        {
                            if (menu_level == 0)
                            {
                                incr_item();
                                has_increased = true;
                            }
                            if (menu_level == 1 && !has_increased)
                            {
                                if (item_number == 0)
                                {
                                    incr_scoll_command();
                                }
                                if (item_number == 7)
                                {
                                    temp_cancel++;
                                }
                                if (item_number == 8)
                                {
                                    temp_menu++;
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_1++;
                                }
                                has_increased = true;
                            }
                            if (menu_level == 2 && !has_increased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length + 0.1;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth + 0.1;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type + 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu++;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_2++;
                                }
                                has_increased = true;
                            }
                            if (menu_level == 3 && !has_increased)
                            {
                                if (item_number == 0)
                                {
                                    incr_scoll_command_2();
                                }
                                if (item_number == 7)
                                {
                                    temp_cancel++;
                                }
                                if (item_number == 8)
                                {
                                    temp_menu++;
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_3++;
                                }
                                has_increased = true;
                            }
                        }
                        if (iteration_val == 1)
                        {
                            if (menu_level == 2 && !has_increased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length + 1.0;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth + 1.0;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type + 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu++;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_2++;
                                }
                                has_increased = true;
                            }
                        }
                        if (iteration_val == 2)
                        {
                            if (menu_level == 2 && !has_increased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length + 10.0;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth + 10.0;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type + 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu++;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_2++;
                                }
                                has_increased = true;
                            }
                        }
                        if (iteration_val == 3)
                        {
                            if (menu_level == 2 && !has_increased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length + 100.0;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth + 100.0;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type + 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu++;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_2++;
                                }
                                has_increased = true;
                            }
                        }
                        if (iteration_val == 4)
                        {
                            if (menu_level == 2 && !has_increased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length + 1000.0;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth + 1000.0;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type + 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu++;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_2++;
                                }
                                has_increased = true;
                            }
                        }
                    }
                    argument = "";
                }
                if (has_increased)
                {
                    has_increased = false;
                }
                #endregion

                #region decrease_menu_management
                if (argument.Contains(decrease))
                {

                    if (!has_decreased)
                    {
                        if (iteration_val == 0)
                        {
                            if (menu_level == 0)
                            {
                                decr_item();
                                has_decreased = true;
                            }
                            if (menu_level == 1 && !has_decreased)
                            {
                                if (item_number == 0)
                                {
                                    decr_scoll_command_2();
                                }
                                if (item_number == 7)
                                {
                                    temp_cancel--;
                                }
                                if (item_number == 8)
                                {
                                    temp_menu--;
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_1--;
                                }
                                has_decreased = true;
                            }
                            if (menu_level == 2 && !has_decreased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length - 0.1;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth - 0.1;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type - 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu--;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                }
                                has_decreased = true;
                            }
                            if (menu_level == 3 && !has_decreased)
                            {
                                if (item_number == 0)
                                {
                                    decr_scoll_command();
                                }
                                if (item_number == 7)
                                {
                                    temp_cancel--;
                                }
                                if (item_number == 8)
                                {
                                    temp_menu--;
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_3--;
                                }
                                has_decreased = true;
                            }
                        }
                        if (iteration_val == 1)
                        {
                            if (menu_level == 2 && !has_decreased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length - 1.0;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth - 1.0;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type - 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu--;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_2--;
                                }
                                has_decreased = true;
                            }
                        }
                        if (iteration_val == 2)
                        {
                            if (menu_level == 2 && !has_decreased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length - 10.0;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth - 10.0;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type - 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu--;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_2--;
                                }
                                has_decreased = true;
                            }
                        }
                        if (iteration_val == 3)
                        {
                            if (menu_level == 2 && !has_decreased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length - 100.0;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth - 100.0;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type - 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu--;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_2--;
                                }
                                has_decreased = true;
                            }
                        }
                        if (iteration_val == 4)
                        {
                            if (menu_level == 2 && !has_decreased)
                            {
                                if (item_number == 0)
                                {
                                    temp_drillshaft_length = temp_drillshaft_length - 1000.0;
                                }
                                if (item_number == 1)
                                {
                                    temp_ignore_depth = temp_ignore_depth - 1000.0;
                                }
                                if (item_number == 2)
                                {
                                    temp_scan_type = temp_scan_type - 1;
                                }
                                if (item_number == 3)
                                {
                                }
                                if (item_number == 4)
                                {
                                }
                                if (item_number == 5)
                                {
                                }
                                if (item_number == 6)
                                {
                                }
                                if (item_number == 7)
                                {
                                }
                                if (item_number == 8)
                                {
                                    temp_menu--;
                                }
                                if (item_number == 9)
                                {
                                }
                                if (item_number == 10)
                                {
                                    temp_confirmval_2--;
                                }
                                has_decreased = true;
                            }
                        }
                    }
                    argument = "";
                }

                if (has_decreased)
                {
                    has_decreased = false;
                }
                #endregion

                #region iteration_value_menu_management
                if (menu_level == 0)
                {
                    if (iteration_val >= 0)
                    {
                        if (item_number >= 0)
                        {
                            iteration_view = "";
                        }

                    }
                }
                if (menu_level == 1)
                {
                    if (iteration_val >= 0)
                    {
                        if (item_number == 0)
                        {
                            iteration_view = "1";
                        }
                        if (item_number == 7 || item_number == 8 || item_number == 9 || item_number == 10)
                        {
                            iteration_view = "Yes/No";
                        }
                    }
                }
                if (menu_level == 3)
                {
                    if (iteration_val >= 0)
                    {
                        if (item_number == 0)
                        {
                            iteration_view = "1";
                        }
                        if (item_number == 7 || item_number == 8 || item_number == 9 || item_number == 10)
                        {
                            iteration_view = "Yes/No";
                        }
                    }
                }

                if (menu_level == 2)
                {
                    if (iteration_val == 0)
                    {
                        if (item_number == 0 || item_number == 1)
                        {
                            iteration_view = "0.1";
                        }
                        if (item_number == 2)
                        {
                            iteration_view = "1";
                        }
                        if (item_number >= 3 && item_number <= 7)
                        {
                            iteration_view = "";
                        }
                        if (item_number == 8)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 9)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 10)
                        {
                            iteration_view = "Yes/No";
                        }
                    }

                    if (iteration_val == 1)
                    {
                        if (item_number == 0 || item_number == 1)
                        {
                            iteration_view = "1.0";
                        }
                        if (item_number == 2)
                        {
                            iteration_view = "1";
                        }
                        if (item_number >= 3 && item_number <= 7)
                        {
                            iteration_view = "";
                        }
                        if (item_number == 8)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 9)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 10)
                        {
                            iteration_view = "Yes/No";
                        }
                    }

                    if (iteration_val == 2)
                    {
                        if (item_number == 0 || item_number == 1)
                        {
                            iteration_view = "10.0";
                        }
                        if (item_number == 2)
                        {
                            iteration_view = "1";
                        }
                        if (item_number >= 3 && item_number <= 7)
                        {
                            iteration_view = "";
                        }
                        if (item_number == 8)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 9)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 10)
                        {
                            iteration_view = "Yes/No";
                        }
                    }

                    if (iteration_val == 3)
                    {
                        if (item_number == 0 || item_number == 1)
                        {
                            iteration_view = "100.0";
                        }
                        if (item_number == 2)
                        {
                            iteration_view = "1";
                        }
                        if (item_number >= 3 && item_number <= 7)
                        {
                            iteration_view = "";
                        }
                        if (item_number == 8)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 9)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 10)
                        {
                            iteration_view = "Yes/No";
                        }
                    }

                    if (iteration_val == 4)
                    {
                        if (item_number == 0 || item_number == 1)
                        {
                            iteration_view = "1000.0";
                        }
                        if (item_number == 2)
                        {
                            iteration_view = "1";
                        }
                        if (item_number >= 3 && item_number <= 7)
                        {
                            iteration_view = "";
                        }
                        if (item_number == 8)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 9)
                        {
                            iteration_view = "Yes/No";
                        }
                        if (item_number == 10)
                        {
                            iteration_view = "Yes/No";
                        }
                    }
                }
                #endregion

                #region confirmation_management
                // confirm management
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 0)
                    {
                        if (item_number == 0)
                        {
                            if (temp_scan_type != scan_type)
                            {
                                temp_scan_type = scan_type;
                            }
                            menu_level = 2;
                            item_number = 0;
                            argument = "";

                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 0)
                    {
                        if (item_number == 1)
                        {
                            menu_level = 1;
                            item_number = 0;
                            argument = "";
                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 0)
                    {
                        if (item_number == 2)
                        {
                            menu_level = 3;
                            item_number = 0;
                            argument = "";
                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 1)
                    {


                        if (item_number == 10 && !confirm_sel_1)
                        {
                            item_number = 0;
                            confirm_command = false;
                            argument = "";
                        }

                        if (item_number == 10 && confirm_sel_1)
                        {
                            if (temp_cancel == 1)
                            {
                                scroll_item_val = 0;
                                temp_menu = 0;
                                last_command = "";
                            }

                            if (temp_menu == 0)
                            {
                                command_resolver();
                                last_command = disp_command;
                                scan_type = new_scan_type;
                                Storage_Update();
                                confirm_command = true;
                                item_number = 0;
                                scroll_item_val = 0;
                                temp_confirmval_1 = 0;
                                temp_cancel = 0;
                                confirm_sel_1 = false;
                                temp_menu = 0;
                                argument = "";
                            }
                            if (temp_menu == 1)
                            {
                                scroll_item_val = 0;
                                temp_menu = 0;
                                menu_level = 0;
                                confirm_command = true;
                                item_number = 0;
                                scroll_item_val = 0;
                                temp_confirmval_1 = 0;
                                temp_cancel = 0;
                                confirm_sel_1 = false;
                                argument = "";
                            }

                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    //if menu is job configuration
                    if (menu_level == 2)
                    {
                        if (item_number == 10 && !confirm_sel_2)
                        {
                            incr_item();
                            confirm_send = false;
                            argument = "";
                        }
                        if (item_number == 10 && confirm_sel_2)
                        {

                            if (temp_menu == 0)
                            {
                                safe_position = new_drillshaft_length;
                                free_center_position = new_ignore_depth;
                                scan_type = new_scan_type;
                                Storage_Update();
                                confirm_send = true;
                                temp_drillshaft_length = 0.0;
                                temp_ignore_depth = 0.0;
                                temp_confirmval_2 = 0;
                                confirm_sel_2 = false;
                                incr_item();
                                argument = "";
                            }
                            if (temp_menu == 1)
                            {
                                scroll_item_val = 0;
                                temp_menu = 0;
                                menu_level = 0;
                                item_number = 0;
                                confirm_send = true;
                                temp_drillshaft_length = 0.0;
                                temp_ignore_depth = 0.0;
                                temp_confirmval_2 = 0;
                                confirm_sel_2 = false;
                                argument = "";
                            }
                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 3)
                    {


                        if (item_number == 10 && !confirm_sel_3)
                        {
                            item_number = 0;
                            confirm_command = false;
                            argument = "";
                        }

                        if (item_number == 10 && confirm_sel_3)
                        {
                            if (temp_cancel == 1)
                            {
                                scroll_item_val_2 = 0;
                                temp_menu = 0;
                                temp_command = 0;
                                last_command = "";
                            }

                            if (temp_menu == 0)
                            {
                                command_resolver_2();
                                last_command = disp_command;
                                interface_command = temp_command;
                                confirm_command = true;
                                item_number = 0;
                                scroll_item_val_2 = 0;
                                temp_confirmval_3 = 0;
                                temp_cancel = 0;
                                confirm_sel_3 = false;
                                temp_menu = 0;
                                temp_command = 0;
                                argument = "";
                            }
                            if (temp_menu == 1)
                            {
                                scroll_item_val = 0;
                                temp_menu = 0;
                                menu_level = 0;
                                confirm_command = true;
                                item_number = 0;
                                scroll_item_val_2 = 0;
                                temp_confirmval_3 = 0;
                                temp_cancel = 0;
                                confirm_sel_3 = false;
                                interface_command = 0;
                                temp_command = 0;
                                last_command = disp_command;
                                argument = "";
                            }

                        }
                    }
                }


                if (argument.Contains("confirm"))
                {
                    // if menu is command configuration
                    if (menu_level == 1)
                    {
                        if (item_number == 0)
                        {
                            item_number = 7;
                            argument = "";
                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 1)
                    {
                        if (item_number == 7)
                        {
                            item_number = 8;
                            argument = "";
                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 1)
                    {
                        if (item_number == 8)
                        {
                            item_number = 10;
                            argument = "";
                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 2)
                    {
                        if (item_number >= 0 && item_number <= 10)
                        {
                            incr_item();
                            argument = "";
                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    // if menu is command configuration
                    if (menu_level == 3)
                    {
                        if (item_number == 0)
                        {
                            item_number = 7;
                            argument = "";
                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 3)
                    {
                        if (item_number == 7)
                        {
                            item_number = 8;
                            argument = "";
                        }
                    }
                }
                if (argument.Contains("confirm"))
                {
                    if (menu_level == 3)
                    {
                        if (item_number == 8)
                        {
                            item_number = 10;
                            argument = "";
                        }
                    }
                }


                if (setup_complete && interface_setup_complete)
                {
                    LineResolver(item_number);
                    screen_display();
                    if (display_tag_interface.Count > 0 && display_surface_interface != null)
                    {
                        display_surface_interface.WriteText(display_string_interface.ToString());
                    }
                }

                if (confirm_send)
                {
                    confirm_send = false;
                }
                if (confirm_command)
                {
                    confirm_command = false;
                }
                #endregion

            }


            if (!interface_presence)
            {
                if (argument.Contains(increase))
                {
                    scan_type++;
                    if (scan_type > 2)
                    {
                        scan_type = 0;
                    }
                    argument = "";
                }
                if (argument.Contains(decrease))
                {
                    scan_type--;
                    if (scan_type < 0)
                    {
                        scan_type = 2;
                    }
                    argument = "";
                }

            }

            #region interface_variable_limit_management
            //interface value limit management
            new_drillshaft_length = safe_position + temp_drillshaft_length;
            if (new_drillshaft_length < 0.0)
            {
                new_drillshaft_length = 0.0;
            }


            new_ignore_depth = free_center_position + temp_ignore_depth;
            if (new_ignore_depth < 0.1)
            {
                new_ignore_depth = 0.1;
            }


            if (temp_scan_type > 2)
            {
                temp_scan_type = 0;
            }
            if (temp_scan_type < 0)
            {
                temp_scan_type = 2;
            }

            new_scan_type = temp_scan_type;

            if (new_scan_type > 2)
            {
                new_scan_type = 0;
            }
            if (new_scan_type < 0)
            {
                new_scan_type = 2;
            }

            if (temp_command > 3)
            {
                temp_command = 0;
            }
            if (temp_command < 0)
            {
                temp_command = 3;
            }




            //cancel management
            if (temp_cancel < 0)
            {
                temp_cancel = 1;
            }
            if (temp_cancel > 1)
            {
                temp_cancel = 0;
            }
            if (temp_cancel == 0)
            {
                cancel_display = "No";
            }
            if (temp_cancel == 1)
            {
                cancel_display = "Yes";
            }

            //menu management
            if (temp_menu < 0)
            {
                temp_menu = 1;
            }
            if (temp_menu > 1)
            {
                temp_menu = 0;
            }
            if (temp_menu == 0)
            {
                menu_display = "No";
            }
            if (temp_menu == 1)
            {
                menu_display = "Yes";
            }

            //confirm management
            if (temp_confirmval_1 < 0)
            {
                temp_confirmval_1 = 1;
            }
            if (temp_confirmval_1 > 1)
            {
                temp_confirmval_1 = 0;
            }
            if (temp_confirmval_1 == 1)
            {
                confirm_sel_1 = true;
            }
            if (temp_confirmval_1 == 0)
            {
                confirm_sel_1 = false;
            }

            if (temp_confirmval_2 < 0)
            {
                temp_confirmval_2 = 1;
            }
            if (temp_confirmval_2 > 1)
            {
                temp_confirmval_2 = 0;
            }
            if (temp_confirmval_2 == 1)
            {
                confirm_sel_2 = true;
            }
            if (temp_confirmval_2 == 0)
            {
                confirm_sel_2 = false;
            }

            if (temp_confirmval_3 < 0)
            {
                temp_confirmval_3 = 1;
            }
            if (temp_confirmval_3 > 1)
            {
                temp_confirmval_3 = 0;
            }
            if (temp_confirmval_3 == 1)
            {
                confirm_sel_3 = true;
            }
            if (temp_confirmval_3 == 0)
            {
                confirm_sel_3 = false;
            }


            //item menu display
            if (new_scan_type == 0)
            {
                new_scan_type_display = "Planetary";
            }
            if (new_scan_type == 1)
            {
                new_scan_type_display = "Asteroid";
            }
            if (new_scan_type == 2)
            {
                new_scan_type_display = "Free Align";
            }
            
                //confirm display
                if (confirm_sel_1)
                {
                    displayconfirm_1 = "Yes";
                }
                if (!confirm_sel_1)
                {
                    displayconfirm_1 = "No";
                }
                if (confirm_sel_2)
                {
                    displayconfirm_2 = "Yes";
                }
                if (!confirm_sel_2)
                {
                    displayconfirm_2 = "No";
                }
                if (confirm_sel_3)
                {
                    displayconfirm_3 = "Yes";
                }
                if (!confirm_sel_3)
                {
                    displayconfirm_3 = "No";
                }
                #endregion



                #region direct_command_management
                //command management
                if (argument == "setup" && setup_complete)
                {
                    interface_command = 0;
                    scroll_item_val_2 = 0;
                    setup_complete = false;
                    interface_setup_complete = false;
                    argument = "";
                    Echo("Running setup...");
                }
                if (argument.Contains(reset_cmd) || interface_command == 3)
                {
                    interface_command = 0;
                    scroll_item_val_2 = 0;
                    scan_complete = false;
                    Echo("Scan reset.");
                    transmit_complete = false;
                    Echo("Transmission reset.");
                    target_aquired_light_actual.Enabled = false;
                    target_transmit_light_actual.Enabled = false;
                }

                //aligment mode management
                if (argument.Contains(ast_dis_cmd))
                {
                    if (enable_asteroid_detection == true)
                    {
                        enable_asteroid_detection = false;
                    }

                }

                if (argument.Contains(ast_en_cmd))
                {
                    if (enable_asteroid_detection == false)
                    {
                        enable_asteroid_detection = true;
                    }

                }

                if (argument.Contains(free_form_en_cmd))
                {
                    if (free_form == false)
                    {
                        free_form = true;
                    }

                }

                if (argument.Contains(free_form_dis_cmd))
                {
                    if (free_form == true)
                    {
                        free_form = false;
                    }

                }


                if (argument.Contains(retry_send_cmd))
                {
                    if (transmit_complete == true)
                    {
                        transmit_complete = false;
                    }

                }
                #endregion

                #region scan_command_management
                if (scan_type == 0)
                {
                    scan_type_display = "Planetary";
                    enable_asteroid_detection = false;
                    free_form = false;
                }
                if (scan_type == 1)
                {
                    scan_type_display = "Asteroid";
                    enable_asteroid_detection = true;
                    free_form = false;
                }
                if (scan_type == 2)
                {
                    scan_type_display = "Free Align";
                    enable_asteroid_detection = false;
                    free_form = true;
                }
                if (argument.Contains(scan_cmd) || interface_command == 1)
                {
                    interface_command = 0;
                    scroll_item_val_2 = 0;
                    if (scan_complete == false)
                    {
                        MyDetectedEntityInfo hitinfocamera = camera_actual.Raycast(raycast_scan_distance);
                        if (hitinfocamera.IsEmpty())
                        {
                            Echo($"Mining surface not found within: '{raycast_scan_distance}'m.");
                            surface_coords.X = remote_control_actual.GetPosition().X;
                            surface_coords.Z = remote_control_actual.GetPosition().Y;
                            surface_coords.Y = remote_control_actual.GetPosition().Z;
                            surface_found = false;
                        }

                        if (!hitinfocamera.IsEmpty())
                        {
                            distance_scan = (hitinfocamera.HitPosition.Value - camera_actual.GetPosition()).Length();
                            Echo($"Surface found'{distance_scan}'m.");
                            surface_coords.X = hitinfocamera.HitPosition.Value.X;
                            surface_coords.Y = hitinfocamera.HitPosition.Value.Y;
                            surface_coords.Z = hitinfocamera.HitPosition.Value.Z;
                            surface_found = true;
                        }

                        if (surface_found == true && scan_complete == false)
                        {
                            //check for asteroid


                            if (sensor_actual.IsActive == true && enable_asteroid_detection == true)
                            {
                                asteroidsDetected = true;
                                Echo("Asteroid detected");
                            }

                            if (sensor_actual.IsActive == false && enable_asteroid_detection == true || enable_asteroid_detection == false)
                            {
                                asteroidsDetected = false;
                            }

                            if (!asteroidsDetected && !free_form)
                            {
                                //set vector to gravity
                                gravity = remote_control_actual.GetNaturalGravity();
                                TargetVec = Vector3D.Normalize(new Vector3D(-gravity));
                                Echo("align to gravity");
                            }

                            if (asteroidsDetected && !free_form)
                            {
                                //set vector to asteroid                           
                                asteroid_coords = sensor_actual.LastDetectedEntity.BoundingBox.Center;
                                TargetVec = Vector3D.Normalize(new Vector3D(-(asteroid_coords - surface_coords)));
                                Echo("align to asteroid");
                            }

                            if (free_form)
                            {
                                //set vector to asteroid                           
                                camera_coords = camera_actual.GetPosition();
                                TargetVec = Vector3D.Normalize(new Vector3D(-(surface_coords - camera_coords)));
                                Echo("align to scan vector");
                            }

                            if (free_form)
                            {
                                Vector3D targetpositionc = TargetVec * -free_center_position;
                                free_centre_target_coords.Y = Math.Round(surface_coords.Y + targetpositionc.Y, 2);
                                free_centre_target_coords.X = Math.Round(surface_coords.X + targetpositionc.X, 2);
                                free_centre_target_coords.Z = Math.Round(surface_coords.Z + targetpositionc.Z, 2);
                            }

                            Vector3D targetpositiont = TargetVec * safe_position;
                            target_coords.Y = Math.Round(surface_coords.Y + targetpositiont.Y, 2);
                            target_coords.X = Math.Round(surface_coords.X + targetpositiont.X, 2);
                            target_coords.Z = Math.Round(surface_coords.Z + targetpositiont.Z, 2);
                            Echo("Navigation point calculated");
                            Echo("Coordinates ready.");

                            scan_complete = true;

                            target_aquired_light_actual.Enabled = true;
                            target_transmit_light_actual.Enabled = false;

                        } // surface found

                    } // scan complete

                    if (scan_complete == false)
                    {
                        Echo("Please initiate scan before sending coordinates to controller");
                        transmit_complete = false;
                        target_aquired_light_actual.Enabled = false;
                        target_transmit_light_actual.Enabled = false;
                    }

                }
                #endregion

                #region transmission_management

                if (argument.Contains(send_cmd) && scan_complete && !transmit_complete || interface_command == 2 && scan_complete && !transmit_complete)
                {
                    interface_command = 0;
                    scroll_item_val_2 = 0;
                    StringBuilder comms_out = new StringBuilder();
                    StringBuilder copy_target = new StringBuilder();
                    StringBuilder copy_asteroid = new StringBuilder();
                    comms_out.Append("GPS");
                    comms_out.Append(":");
                    comms_out.Append("TGT");
                    comms_out.Append(":");
                    comms_out.Append(target_coords.X);
                    comms_out.Append(":");
                    comms_out.Append(target_coords.Y);
                    comms_out.Append(":");
                    comms_out.Append(target_coords.Z);
                    comms_out.Append(":");
                    comms_out.Append("#FF75C9F1");
                    comms_out.Append(":");
                    comms_out.Append(safe_position);
                    comms_out.Append(":");

                    copy_target.Append("GPS");
                    copy_target.Append(":");
                    copy_target.Append("TGT");
                    copy_target.Append(":");
                    copy_target.Append(target_coords.X);
                    copy_target.Append(":");
                    copy_target.Append(target_coords.Y);
                    copy_target.Append(":");
                    copy_target.Append(target_coords.Z);
                    copy_target.Append(":");
                    copy_target.Append("#FF75C9F1");
                    copy_target.Append(":");

                    if (asteroidsDetected == true)
                    {
                        comms_out.Append("GPS");
                        comms_out.Append(":");
                        comms_out.Append("AST");
                        comms_out.Append(":");
                        comms_out.Append(Math.Round(asteroid_coords.X, 2));
                        comms_out.Append(":");
                        comms_out.Append(Math.Round(asteroid_coords.Y, 2));
                        comms_out.Append(":");
                        comms_out.Append(Math.Round(asteroid_coords.Z, 2));
                        comms_out.Append(":");
                        comms_out.Append("#FF1551");
                        comms_out.Append(":");

                        copy_asteroid.Append("GPS");
                        copy_asteroid.Append(":");
                        copy_asteroid.Append("AST");
                        copy_asteroid.Append(":");
                        copy_asteroid.Append(Math.Round(asteroid_coords.X, 2));
                        copy_asteroid.Append(":");
                        copy_asteroid.Append(Math.Round(asteroid_coords.Y, 2));
                        copy_asteroid.Append(":");
                        copy_asteroid.Append(Math.Round(asteroid_coords.Z, 2));
                        copy_asteroid.Append(":");
                        copy_asteroid.Append("#FF1551");
                        copy_asteroid.Append(":");
                        remote_control_actual.CustomData = copy_asteroid.ToString();
                    }

                    if (free_form == true)
                    {
                        comms_out.Append("GPS");
                        comms_out.Append(":");
                        comms_out.Append("FRE");
                        comms_out.Append(":");
                        comms_out.Append(Math.Round(free_centre_target_coords.X, 2));
                        comms_out.Append(":");
                        comms_out.Append(Math.Round(free_centre_target_coords.Y, 2));
                        comms_out.Append(":");
                        comms_out.Append(Math.Round(free_centre_target_coords.Z, 2));
                        comms_out.Append(":");
                        comms_out.Append("#FF1551");
                        comms_out.Append(":");

                        copy_asteroid.Append("GPS");
                        copy_asteroid.Append(":");
                        copy_asteroid.Append("FRE");
                        copy_asteroid.Append(":");
                        copy_asteroid.Append(Math.Round(free_centre_target_coords.X, 2));
                        copy_asteroid.Append(":");
                        copy_asteroid.Append(Math.Round(free_centre_target_coords.Y, 2));
                        copy_asteroid.Append(":");
                        copy_asteroid.Append(Math.Round(free_centre_target_coords.Z, 2));
                        copy_asteroid.Append(":");
                        copy_asteroid.Append("#FF1551");
                        copy_asteroid.Append(":");
                        remote_control_actual.CustomData = copy_asteroid.ToString();
                    }
                    data_out = comms_out.ToString();
                    Me.CustomData = copy_target.ToString();

                    IGC.SendBroadcastMessage(tx_channel, data_out, TransmissionDistance.TransmissionDistanceMax);


                    //build data string to for mining controller and transmit via IGC
                    transmit_complete = true;
                    target_transmit_light_actual.Enabled = true;
                    Echo("Data sent.");

                }
            #endregion

            #region main_display_mng
                Echo($"Runtime: {Runtime.LastRunTimeMs}");
                Echo($"Channel: {tx_channel}");
                Echo("Target: " + surface_found);
                Echo("TX: " + target_coords.X + " TY: " + target_coords.Y + " TZ: " + target_coords.Z + " SafeD: " + safe_position + "m");
                if (batteries_tag.Count > 0)
                {
                    Echo($"Power:{Math.Round((double)percent_battery_power, 1)}%");
                }
                Echo("Free scan: " + free_form);
                Echo("Asteroid detection: " + enable_asteroid_detection);
                Echo("Asteroid: " + asteroidsDetected);
                Echo("--------");
                Echo("PB Arguments:");
                Echo("========");
                Echo($"Scan = {scan_cmd}");
                Echo($"Reset = {reset_cmd}");
                Echo($"Send  = {send_cmd}");

                Echo("Use the below run arguments to interface:");
                Echo("----------------------------------------");
                Echo("");
                /*if (!interface_presence)
                {
                    Echo($"Interface not found. ");
                } */
                if (interface_presence)
                {
                    Echo($"Confirm = {confirmval}");
                }
                Echo($"Increase selection menu = {increase}");
                Echo($"Decrease selection menu = {decrease}");
                if (interface_presence)
                {
                    Echo($"Main menu = {menureturn}");
                    Echo($"Change increment = {incrsel}");
                    Echo($"Increase value = {increase}");
                    Echo($"Decrease value = {decrease}");
                }
                Echo("--------");
                Echo($"Direct commands:");
                Echo("--------");
                Echo($"Asteroid EN = {ast_en_cmd}");
                Echo($"Asteroid DIS = {ast_dis_cmd}");
                Echo($"Reset send = {retry_send_cmd}");
                Echo($"Free form EN = {free_form_en_cmd}");
                Echo($"Free form DIS = {free_form_dis_cmd}");

                if (asteroidsDetected == true)
                {
                    Echo("AX: " + Math.Round(asteroid_coords.X, 2) + " AY: " + Math.Round(asteroid_coords.Y, 2) + " AZ: " + Math.Round(asteroid_coords.Z, 2));
                }
                if (free_form == true && scan_complete)
                {
                    Echo("FX: " + Math.Round(free_centre_target_coords.X, 2) + " FY: " + Math.Round(free_centre_target_coords.Y, 2) + " FZ: " + Math.Round(free_centre_target_coords.Z, 2));
                }

                display_string_main.Clear();
                if (batteries_tag.Count > 0)
                {
                    display_string_main.Append('\n');
                    display_string_main.Append($"GMDP {ver} Running {icon}   Power:{Math.Round((double)percent_battery_power, 1)}%");
                    display_string_main.Append('\n');
                }
                if (batteries_tag.Count < 0)
                {
                    display_string_main.Append('\n');
                    display_string_main.Append($"GMDP {ver} Running {icon}");
                    display_string_main.Append('\n');
                }

                display_string_main.Append($"Channel: {tx_channel}");
                display_string_main.Append('\n');
                display_string_main.Append('\n');
                display_string_main.Append("Target: " + surface_found);
                display_string_main.Append('\n');
                display_string_main.Append("TX: " + target_coords.X + " TY: " + target_coords.Y + " TZ: " + target_coords.Z);
                display_string_main.Append('\n');
                display_string_main.Append($"Surface distance: {safe_position}m");
                display_string_main.Append('\n');
                if (free_form)
                {
                    display_string_main.Append($"Align Depth: {free_center_position}m");
                }
                display_string_main.Append('\n');
                display_string_main.Append('\n');
                display_string_main.Append(">: Scan type: " + scan_type_display + " :<");
                display_string_main.Append('\n');
                if (enable_asteroid_detection)
                {
                    display_string_main.Append("Asteroid detection: " + enable_asteroid_detection);
                    display_string_main.Append('\n');
                }
                if (free_form)
                {
                    display_string_main.Append("Free scan: " + free_form);
                    display_string_main.Append('\n');
                }
                if (enable_asteroid_detection)
                {
                    display_string_main.Append("Asteroid: " + asteroidsDetected);
                    display_string_main.Append('\n');
                }
                display_string_main.Append("Scan: " + scan_complete);
                display_string_main.Append('\n');
                display_string_main.Append("Transmit: " + transmit_complete);
                if (asteroidsDetected == true)
                {
                    display_string_main.Append("AX: " + Math.Round(asteroid_coords.X, 2) + " AY: " + Math.Round(asteroid_coords.Y, 2) + " AZ: " + Math.Round(asteroid_coords.Z, 2));
                }
                if (free_form && scan_complete)
                {
                    display_string_main.Append("FX: " + Math.Round(free_centre_target_coords.X, 2) + " FY: " + Math.Round(free_centre_target_coords.Y, 2) + " FZ: " + Math.Round(free_centre_target_coords.Z, 2));
                }
                if (display_surface_main != null)
                {
                    display_surface_main.WriteText(display_string_main);
                }

                #endregion

                state_shifter();
            } //end void main
        

        public void drone_custom_data_check(string custominfo, int index)
        {
            Echo("Checking for drone config information..");
            String[] temp_id = custominfo.Split(':');
            Echo($"{temp_id.Length}");

            if (temp_id.Length > 0)
            {
                if (temp_id[0] != null)
                {
                    if (int.TryParse(temp_id[0], out temp_id_num))
                    {
                        int.TryParse(temp_id[0], out temp_id_num);
                    }
                    else
                    {
                        temp_id_num = drone_id;
                        drone_id = temp_id_num;
                        Echo($"Resorting to default ID#.{drone_id}");
                    }
                }
            }
            else
            {
                temp_id_num = drone_id;
                Echo($"Resorting to default ID#.{drone_id}");
            }
            if (temp_id.Length > 1)
            {
                if (temp_id[1] != null)
                {
                    temp_id_scout = temp_id[1];
                    scout_tag = temp_id_scout;
                    if (temp_id_scout == "" || temp_id_scout == null)
                    {
                        temp_id_scout = scout_tag;
                        Echo($"Resorting to default scout tag {scout_tag}");
                    }
                }
            }
            else
            {
                temp_id_scout = scout_tag;
            }
            if (temp_id.Length > 2)
            {
                if (temp_id[2] != null)
                {
                    temp_id_name = temp_id[2];
                    drone_tag = temp_id_name;
                    if (temp_id_name == "" || temp_id_name == null)
                    {
                        temp_id_name = drone_tag;
                        Echo($"Resorting to default drone tag {drone_tag}.");
                    }
                }
            }
            else
            {
                temp_id_name = drone_tag;
            }

            if (temp_id.Length == 0)
            {
                temp_id_scout = scout_tag;
                temp_id_name = drone_tag;
                temp_id_num = drone_id;
                Echo($"Resorting to default config {temp_id_name} {temp_id_scout}.");
            }
            
            
            
            if (antenna_all[index] != null)
            {
                antenna_all[index].CustomData = $"{drone_id}:{scout_tag}:{drone_tag}";
            }
            Echo($"Drone info: {scout_tag}:{drone_tag}");
            drone_id_name = "[" + scout_tag + " " + drone_id + "]";
            tx_channel = drone_tag + " " + prospC;
            light_transmit_tag = "[" + scout_tag + " " + drone_id + " " + txl + "]";
            light_target_tag = "[" + scout_tag + " " + drone_id + " " + tgt + "]";
            main_display_name = "[" + scout_tag + " " + drone_id + " " + main_display_tag + "]";
            Me.CustomName = $"GMDP Programmable Block {drone_id_name} [{drone_tag}] {prospC} {drone_id}";
        }


        void runicon(int state)
        {
            if (state == 0)
            {
                icon = ".---";
            }
            if (state == 1)
            {
                icon = "-.--";
            }
            if (state == 2)
            {
                icon = "--.-";
            }
            if (state == 3)
            {
                icon = "---.";
            }
        }
        void state_shifter()
        {
            stateshift++;
            if (stateshift > 3)
            {
                stateshift = 0;
            }
            runicon(stateshift);
        }

        public void presence_check()
        {
            #region presence_check
            if (antenna_tag.Count <= 0 || antenna_tag[0] == null)
            {
                Echo($"Antenna with tag: '{drone_id_name}' not found.");
                return;
            }
            antenna_actual = antenna_tag[0];


            if (display_tag_main.Count <= 0 || ((IMyTextSurfaceProvider)display_tag_main[0]).GetSurface(lcd_display_index_main) == null)
            {
                Echo($"LCD display: '{main_display_tag}' not found.");
                //return;
            }
            if (display_tag_interface.Count <= 0 || ((IMyTextSurfaceProvider)display_tag_interface[0]).GetSurface(lcd_display_index_interface) == null)
            {
                Echo($"LCD display: '{interface_display_tag}' not found.");
                //return;
            }
            if (display_surface_main != null)
            {
                if (display_surface_main.ContentType != ContentType.TEXT_AND_IMAGE)
                {
                    display_surface_main.ContentType = ContentType.TEXT_AND_IMAGE;
                    display_surface_main.FontSize = font_zoom;
                }
            }
            if (display_surface_interface != null)
            {
                if (display_surface_interface.ContentType != ContentType.TEXT_AND_IMAGE)
                {
                    display_surface_interface.ContentType = ContentType.TEXT_AND_IMAGE;
                    display_surface_interface.FontSize = font_zoom;
                    interface_presence = true;
                }                
            } else
            {
                interface_presence = false;
            }

            //find remote control, end if not found
            if (remote_control_tag.Count <= 0 || remote_control_tag[0] == null)
            {
                Echo($"Remote control with tag: '{drone_id_name}' not found.");
                return;
            }
            //assign main remote control
            remote_control_actual = remote_control_tag[0];


            //find camera, end if not found
            if (camera_scan.Count <= 0 || camera_scan[0] == null)
            {
                Echo($"Camera with tag: '{scan_camera}' not found.");
                return;
            }

            camera_actual = camera_scan[0];
            if (!camera_actual.Enabled)
            {
                camera_actual.EnableRaycast = true;
            }

            //find batteries, end if not found
            if (batteries_tag.Count <= 0 || batteries_tag[0] == null)
            {
                Echo($"Batteries with tag: '{drone_id_name}' not found.");
                return;
            }

            //find lights, end if not found
            if (lighting_target_aquired.Count <= 0 || lighting_target_aquired[0] == null)
            {
                Echo($"dock indicator light with tag: '{light_target_tag}' not found.");
                return;
            }
            target_aquired_light_actual = lighting_target_aquired[0];

            if (lighting_target_transmit.Count <= 0 || lighting_target_transmit[0] == null)
            {
                Echo($"undock indicator light with tag: '{light_transmit_tag}' not found.");
                return;
            }
            target_transmit_light_actual = lighting_target_transmit[0];



            if (sensor_tag.Count <= 0 || sensor_tag[0] == null)
            {
                Echo($"Sensor with tag: '{drone_id_name}' not found.");
                return;
            }
            sensor_actual = sensor_tag[0];

            if (sensor_actual.DetectAsteroids == false)
            {
                sensor_actual.DetectAsteroids = true;
                sensor_actual.DetectEnemy = false;
                sensor_actual.DetectFriendly = false;
                sensor_actual.DetectLargeShips = false;
                sensor_actual.DetectSmallShips = false;
                sensor_actual.DetectSubgrids = false;
                sensor_actual.DetectFloatingObjects = false;
                sensor_actual.DetectStations = false;
                sensor_actual.DetectPlayers = false;
                sensor_actual.DetectNeutral = false;
                sensor_actual.DetectOwner = false;
            }

            //find remote control, end if not found
            if (display_tag_main.Count <= 0 || display_tag_main[0] == null)
            {
                Echo($"LCD display with tag: '{main_display_name}' not found.");
                return;
            }
            //find remote control, end if not found
            if (display_tag_interface.Count <= 0 || display_tag_interface[0] == null)
            {
                Echo($"LCD display with tag: '{interface_display_name}' not found.");
                //return;
            }
            #endregion
        }

        public void battery_check()
        {
            #region battery_power_monitoring
            //reset power totals for array addition
            t_stored_power = 0;
            stored_power_total = 0;
            t_max_power = 0;
            max_power_total = 0;
            t_current_power = 0;
            current_power_total = 0;
            percent_battery_power = 0.0f;
            if (batteries_tag.Count > 0)
            {
                for (int i = 0; i < batteries_tag.Count; i++)
                {
                    if (batteries_tag[i] != null)
                    {
                        currentbatteryblock = batteries_tag[i];
                        //record stored and max battery capacity
                        t_stored_power = currentbatteryblock.CurrentStoredPower;
                        t_max_power = currentbatteryblock.MaxStoredPower;
                        //record current power output
                        t_current_power = currentbatteryblock.CurrentOutput;
                    }
                    stored_power_total = stored_power_total + t_stored_power;
                    max_power_total = max_power_total + t_max_power;
                    current_power_total = current_power_total + t_current_power;
                    //calculate storage capacity percent
                    percent_battery_power = (stored_power_total / max_power_total) * 100;
                }
            }
            #endregion
        }

        public void incr_item()
        {
            if (menu_level == 0)
            {
                item_max_limit = 2;
                item_min_limit = 0;
            }
            if (menu_level == 1)
            {
                item_max_limit = 1;
                item_min_limit = 0;
            }
            if (menu_level == 2)
            {
                item_max_limit = 10;
                item_min_limit = 0;
            }
            if (menu_level == 3)
            {
                item_max_limit = 1;
                item_min_limit = 0;
            }
            item_number++;
            if (item_number > item_max_limit)
            {
                item_number = item_min_limit;
            }
        }

        public void decr_item()
        {
            if (menu_level == 0)
            {
                item_max_limit = 2;
                item_min_limit = 0;
            }
            if (menu_level == 1)
            {
                item_max_limit = 10;
                item_min_limit = 0;
            }
            if (menu_level == 2)
            {
                item_max_limit = 10;
                item_min_limit = 0;

            }
            if (menu_level == 3)
            {
                item_max_limit = 10;
                item_min_limit = 0;
            }
            item_number--;
            if (item_number < item_min_limit)
            {
                item_number = item_max_limit;
            }
        }

        public void incr_scoll_command()
        {
                scroll_item_val_min_limit = 0;
                scroll_item_val_max_limit = 2;

            scroll_item_val++;
            if (scroll_item_val > scroll_item_val_max_limit)
            {
                scroll_item_val = scroll_item_val_min_limit;
            }
        }

        public void incr_scoll_command_2()
        {

                scroll_item_val_min_limit_2 = 0;
                scroll_item_val_max_limit_2 = 3;
            scroll_item_val_2++;
            if (scroll_item_val_2 > scroll_item_val_max_limit_2)
            {
                scroll_item_val_2 = scroll_item_val_min_limit_2;
            }
        }


        public void decr_scoll_command()
        {            
                scroll_item_val_min_limit = 0;
                scroll_item_val_max_limit = 2;
            
            scroll_item_val--;
            if (scroll_item_val < scroll_item_val_min_limit)
            {
                scroll_item_val = scroll_item_val_max_limit;
            }
        }

        public void decr_scoll_command_2()
        {

                scroll_item_val_min_limit_2 = 0;
                scroll_item_val_max_limit_2 = 3;

            scroll_item_val_2--;
            if (scroll_item_val_2 < scroll_item_val_min_limit_2)
            {
                scroll_item_val_2 = scroll_item_val_max_limit_2;
            }
        }

        public void command_resolver()
        {
            if (menu_level == 1)
            {
                if (scroll_item_val == 0)
                {
                    disp_command = "Planetary";
                    new_scan_type = 0;
                }
                if (scroll_item_val == 1)
                {
                    disp_command = "Asteroid";
                    new_scan_type = 1;
                }
                if (scroll_item_val == 2)
                {
                    disp_command = "Free Align";
                    new_scan_type = 2;
                }
            }
        }
        public void command_resolver_2()
        {
            if (menu_level == 3)
            {
                if (scroll_item_val_2 == 0)
                {
                    disp_command = "";
                    temp_command = 0;
                }
                if (scroll_item_val_2 == 1)
                {
                    disp_command = "Scan";
                    temp_command = 1;
                }
                if (scroll_item_val_2 == 2)
                {
                    disp_command = "Send";
                    temp_command = 2;
                }
                if (scroll_item_val_2 == 3)
                {
                    disp_command = "Reset";
                    temp_command = 3;
                }
            }
        }

        public void Storage_Update()
        {
            _Storage.Set("State", "Safedistance", safe_position);
            _Storage.Set("State", "freecenterposition", free_center_position);
            _Storage.Set("State", "scantype", scan_type);
            Storage = _Storage.ToString();
            Echo("Settings saved");
        }

        public void LineResolver(int linevalin)
        {
            if (linevalin == 0)
            {
                line_highlight_0 = "[O]";
            }
            else
            {
                line_highlight_0 = "[ ]";
            }
            if (linevalin == 1)
            {
                line_highlight_1 = "[O]";
            }
            else
            {
                line_highlight_1 = "[ ]";
            }
            if (linevalin == 2)
            {
                line_highlight_2 = "[O]";
            }
            else
            {
                line_highlight_2 = "[ ]";
            }
            if (linevalin == 3)
            {
                line_highlight_3 = "[O]";
            }
            else
            {
                line_highlight_3 = "[ ]";
            }
            if (linevalin == 4)
            {
                line_highlight_4 = "[O]";
            }
            else
            {
                line_highlight_4 = "[ ]";
            }
            if (linevalin == 5)
            {
                line_highlight_5 = "[O]";
            }
            else
            {
                line_highlight_5 = "[ ]";
            }
            if (linevalin == 6)
            {
                line_highlight_6 = "[O]";
            }
            else
            {
                line_highlight_6 = "[ ]";
            }
            if (linevalin == 7)
            {
                line_highlight_7 = "[O]";
            }
            else
            {
                line_highlight_7 = "[ ]";
            }
            if (linevalin == 8)
            {
                line_highlight_8 = "[O]";
            }
            else
            {
                line_highlight_8 = "[ ]";
            }
            if (linevalin == 9)
            {
                line_highlight_9 = "[O]";
            }
            else
            {
                line_highlight_9 = "[ ]";
            }
            if (linevalin == 10)
            {
                line_highlight_10 = "[O]";
            }
            else
            {
                line_highlight_10 = "[ ]";
            }

        }

        public void screen_display()
        {
            display_string_interface.Clear();
            display_string_interface.Append('\n');
            if (menu_level == 0)
            {
                display_string_interface.Append($"GMDP - {ver}");
                display_string_interface.Append('\n');
                display_string_interface.Append("------------");
                display_string_interface.Append('\n');
                display_string_interface.Append($"Main Menu - Iteration: {iteration_view} Item: {(item_number + 1)}");
                display_string_interface.Append('\n');
                display_string_interface.Append('\n');
            }
            if (menu_level == 1)
            {
                display_string_interface.Append($"GMDP - {ver}");
                display_string_interface.Append('\n');
                display_string_interface.Append("------------");
                display_string_interface.Append('\n');
                display_string_interface.Append($"Alignment Menu - Iteration: {iteration_view} Item: {(item_number + 1)}");
                display_string_interface.Append('\n');
                display_string_interface.Append('\n');
            }
            if (menu_level == 2)
            {
                display_string_interface.Append($"GMDI - {ver}");
                display_string_interface.Append('\n');
                display_string_interface.Append("------------");
                display_string_interface.Append('\n');
                display_string_interface.Append($"Mining Job Config. - Iteration: {iteration_view} Item: {(item_number + 1)}");
                display_string_interface.Append('\n');
                display_string_interface.Append('\n');
            }
            if (menu_level == 3)
            {
                display_string_interface.Append($"GMDP - {ver}");
                display_string_interface.Append('\n');
                display_string_interface.Append("------------");
                display_string_interface.Append('\n');
                display_string_interface.Append($"Command Menu - Iteration: {iteration_view} Item: {(item_number + 1)}");
                display_string_interface.Append('\n');
                display_string_interface.Append('\n');
            }

            if (menu_level == 0)
            {
                display_string_interface.Append($"{line_highlight_0} 1. {item_line_0[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_1} 2. {item_line_1[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_2} 3. {item_line_2[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append('\n');
                display_string_interface.Append('\n');
                display_string_interface.Append($"Command: {last_command}");
                display_string_interface.Append('\n');
            }
            if (menu_level == 1)
            {
                display_string_interface.Append($"{line_highlight_0} 1. {scroll_command_item[scroll_item_val]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_1} ..  {item_line_1[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_7} 8. {item_line_7[menu_level]} {cancel_display}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_8} 9.  {item_line_8[menu_level]} {menu_display}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_1} ..  {item_line_1[menu_level]}");                
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_10} 11. {item_line_10[menu_level]} {displayconfirm_1}");
                if (confirm_command)
                {
                    display_string_interface.Append('\n');
                    display_string_interface.Append('\n');
                    display_string_interface.Append("Alignment confirmed!");
                    display_string_interface.Append('\n');
                }
                display_string_interface.Append('\n');
                display_string_interface.Append('\n');
                display_string_interface.Append($"Alignment: {last_command}");
                display_string_interface.Append('\n');
            }
            if (menu_level == 2)
            {
                display_string_interface.Append($"{line_highlight_0} 1. {item_line_0[menu_level]} {new_drillshaft_length}m");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_1} 2. {item_line_1[menu_level]} {new_ignore_depth}m");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_2} 3. {item_line_2[menu_level]} {new_scan_type_display}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_3} ..  {item_line_3[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_4} ..  {item_line_4[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_5} ..  {item_line_5[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_6} ..  {item_line_6[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_7} ..  {item_line_7[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_8} 9.  {item_line_8[menu_level]} {menu_display}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_9} ..  {item_line_9[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_10} 11. {item_line_10[menu_level]} {displayconfirm_2}");
                if (confirm_send)
                {
                    display_string_interface.Append('\n');
                    display_string_interface.Append('\n');
                    display_string_interface.Append("Data confirmed!");
                    display_string_interface.Append('\n');
                }
            }
            if (menu_level == 3)
            {
                display_string_interface.Append($"{line_highlight_0} 1. {scroll_command_item_2[scroll_item_val_2]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_1} ..  {item_line_1[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_7} 8. {item_line_7[menu_level]} {cancel_display}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_8} 9.  {item_line_8[menu_level]} {menu_display}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_1} ..  {item_line_1[menu_level]}");
                display_string_interface.Append('\n');
                display_string_interface.Append($"{line_highlight_10} 11. {item_line_10[menu_level]} {displayconfirm_3}");
                if (confirm_command)
                {
                    display_string_interface.Append('\n');
                    display_string_interface.Append('\n');
                    display_string_interface.Append("Command confirmed!");
                    display_string_interface.Append('\n');
                }
                display_string_interface.Append('\n');
                display_string_interface.Append('\n');
                display_string_interface.Append($"Command: {last_command}");
                display_string_interface.Append('\n');
            }




        }
        //end program
    }
}

