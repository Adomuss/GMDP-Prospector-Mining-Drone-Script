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
        //Mining controller spotter drone V0.324A
        #region mdk preserve
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        int drone_id = 1;
        string drone_tag = "SWRM_D";
        string scout_tag = "PSMD";
        string lcd_display_tag = "D1";

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
        string up_val = "incrval";
        string down_val = "decrval";
        string menuitem_select = "select";
        string iterate_cmd = "iterate";
        string confirm_cmd = "confirm";

        int lcd_display_index = 0; //used for devices with multiple screen panels (0+) 
        #endregion
        string version = "V0.324";
        string drone_id_name = "";
        string tx_channel = "";
        string light_transmit_tag = "";
        string light_target_tag = "";
        string lcd_display_name = "";
        string txl = "TX";
        string tgt = "TGT";
        string prospC = "prospector";
        string scan_camera = "scan";
        float t_stored_power;
        float stored_power_total;
        float t_max_power;
        float max_power_total;
        float t_current_power;
        float current_power_total;
        float font_zoom = 0.850f;

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
        string command_display = "";
        bool command_scan = false;
        bool command_send = false;
        bool command_reset = false;
        bool confirm_pressed = false;




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
        IMyTextSurface display_surface_1;
        
        MyIni _Storage = new MyIni();
        float percent_battery_power = 0.0f;
        bool setup_complete = false;
        string sel_left_1 = "";
        string sel_left_2 = "";
        string sel_left_3 = "";
        string sel_left_4 = "";
        string sel_right_1 = "";
        string sel_right_2 = "";
        string sel_right_3 = "";
        string sel_right_4 = "";
        int item_value_menu = 4;
        string leftpip = ">:";
        string rightpip = ":<";
        int command_select = 0;
        double iterate_val = 0.1;
        int iterate_sel = 0;


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
                lcd_display_name = "[" + scout_tag + " " + drone_id + " " + lcd_display_tag + "]";
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
                thrust_all = new List<IMyThrust>();
                thrust_tag = new List<IMyThrust>();
                connector_all = new List<IMyShipConnector>();
                connector_tag = new List<IMyShipConnector>();

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
                        if (display_all[i].CustomName.Contains(lcd_display_tag))
                        {
                            display_all[i].CustomName = $"Prospector Interface Display {lcd_display_name} [{scout_tag}]";
                            display_tag_main.Add(display_all[i]);
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
                    display_surface_1 = ((IMyTextSurfaceProvider)display_tag_main[0]).GetSurface(lcd_display_index);
                    Echo($"LCD display: '{lcd_display_name}' found.");
                }
                setup_complete = true;
                Echo("Setup complete!");
            }

            if (antenna_tag.Count <= 0 || antenna_tag[0] == null)
            {
                Echo($"Antenna with tag: '{drone_id_name}' not found.");
                return;
            }
            antenna_actual = antenna_tag[0];


            if (display_tag_main.Count <= 0 || ((IMyTextSurfaceProvider)display_tag_main[0]).GetSurface(lcd_display_index) == null)
            {
                Echo($"LCD display: '{lcd_display_tag}' not found.");
                //return;
            }
            if (display_surface_1 != null)
            {
                if (display_surface_1.ContentType != ContentType.TEXT_AND_IMAGE)
                {
                    display_surface_1.ContentType = ContentType.TEXT_AND_IMAGE;
                    display_surface_1.FontSize = font_zoom;
                }
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
            camera_actual.EnableRaycast = true;

            //find batteries, end if not found
            if (batteries_tag.Count <= 0 || batteries_tag[0] == null)
            {
                Echo($"Batteries with tag: '{drone_id_name}' not found.");
                return;
            }

            //reset power totals for array addition
            t_stored_power = 0;
            stored_power_total = 0;
            t_max_power = 0;
            max_power_total = 0;
            t_current_power = 0;
            current_power_total = 0;
            percent_battery_power = 0.0f;
            for (int i = 0; i < batteries_tag.Count; i++)
            {
                if (batteries_tag[i] != null) {
                    currentbatteryblock = batteries_tag[i];
                    //record stored and max battery capacity
                    t_stored_power = currentbatteryblock.CurrentStoredPower;
                    t_max_power = currentbatteryblock.MaxStoredPower;
                    t_current_power = currentbatteryblock.CurrentOutput;
                }
                stored_power_total = stored_power_total + t_stored_power;
                max_power_total = max_power_total + t_max_power;
                //record current power output                
                current_power_total = current_power_total + t_current_power;
                //calculate storage capacity percent
                percent_battery_power = (stored_power_total / max_power_total) * 100;
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

            //find remote control, end if not found
            if (display_tag_main.Count <= 0 || display_tag_main[0] == null)
            {
                Echo($"LCD display with tag: '{lcd_display_name}' not found.");
                return;
            }

            //Logic Start
            Echo($"GMDP {version} Running {icon}");


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

            if (argument.Contains(confirm_cmd) && !confirm_pressed && item_value_menu >= 0 && item_value_menu < 4)
            {
                confirm_pressed = true;
            }
            if (argument.Contains(menuitem_select) || argument.Contains(confirm_cmd) && confirm_pressed)
            {
                item_value_menu++;
                if (item_value_menu == 2 && scan_type == 0)
                {
                    item_value_menu = 3;
                }
                if (item_value_menu == 2 && scan_type == 1)
                {
                    item_value_menu=3;
                }
                if (item_value_menu > 4)
                {
                    item_value_menu = 1;
                }
                if (item_value_menu < 1)
                {
                    item_value_menu = 4;                    
                }  
            }            

            if (argument.Contains(iterate_cmd))
            {
                iterate_sel++;
                if (iterate_sel > 4)
                {
                    iterate_sel = 0;
                }
            }

            if (item_value_menu == 0)
            {
                sel_left_1 = "";
                sel_right_1 = "";
                sel_left_2 = "";
                sel_right_2 = "";
                sel_left_3 = "";
                sel_right_3 = "";
                sel_left_4 = "";
                sel_right_4 = "";
            }
            if (item_value_menu == 1)
            {
                sel_left_1 = leftpip;
                sel_right_1 = rightpip;
                sel_left_2 = "";
                sel_right_2 = "";
                sel_left_3 = "";
                sel_right_3 = "";
                sel_left_4 = "";
                sel_right_4 = "";
            }
            if (item_value_menu == 2)
            {
                sel_left_2 = leftpip;
                sel_right_2 = rightpip;
                sel_left_1 = "";
                sel_right_1 = "";
                sel_left_3 = "";
                sel_right_3 = "";
                sel_left_4 = "";
                sel_right_4 = "";
            }
            if (item_value_menu == 3)
            {
                sel_left_3 = leftpip;
                sel_right_3 = rightpip;
                sel_left_2 = "";
                sel_right_2 = "";
                sel_left_1 = "";
                sel_right_1 = "";
                sel_left_4 = "";
                sel_right_4 = "";
                iterate_sel = 1;
            }
            if (item_value_menu == 4)
            {
                sel_left_4 = leftpip;
                sel_right_3 = rightpip;
                sel_left_2 = "";
                sel_right_2 = "";
                sel_left_1 = "";
                sel_right_1 = "";
                sel_left_3 = "";
                sel_right_3 = "";
                iterate_sel = 1;
            }

            if (iterate_sel == 0)
            {
                iterate_val = 0.1;
            }
            if (iterate_sel == 1)
            {
                iterate_val = 1.0;
            }
            if (iterate_sel == 2)
            {
                iterate_val = 10.0;
            }
            if (iterate_sel == 3)
            {
                iterate_val =100.0;
            }
            if (iterate_sel == 4)
            {
                iterate_val = 1000.0;
            }


            if (argument.Contains(up_val) && item_value_menu == 1)
            {
                safe_position += iterate_val;
                if (safe_position > 2000.0)
                {
                    safe_position = 2000.0;
                }
            }
            if (argument.Contains(down_val) && item_value_menu == 1)
            {
                safe_position -= iterate_val;
                if (safe_position < 0.0)
                {
                    safe_position = 0.0;
                }
            }
            if (argument.Contains(up_val) && item_value_menu == 2)
            {
                free_center_position+= iterate_val;
                if (free_center_position < 0.0)
                {
                    free_center_position = 0.0;
                }
            }
            if (argument.Contains(down_val) && item_value_menu == 2)
            {
                free_center_position -= iterate_val;
                if (free_center_position < 0.0)
                {
                    free_center_position = 0.0;
                }
            }

            if (argument.Contains(up_val) && item_value_menu == 3)
            {
                scan_type++;
                if (scan_type > 2)
                {
                    scan_type = 0;
                }
            }
            if (argument.Contains(down_val) && item_value_menu == 3)
            {
                scan_type--;
                if (scan_type < 0)
                {
                    scan_type = 2;
                }
            }
            if (argument.Contains(up_val) && item_value_menu == 4)
            {
                command_select++;
                if (command_select > 3)
                {
                    command_select = 0;
                }
            }
            if (argument.Contains(down_val) && item_value_menu == 4)
            {
                command_select--;
                if (command_select < 0)
                {
                    command_select = 3;
                }
            }

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

            if (command_select == 0)
            {
                command_display = "Scan";

            }
            if (command_select == 1)
            {
                command_display = "Send";

            }
            if (command_select == 2)
            {
                command_display = "Reset";
            }
            if (command_select == 3)
            {
                command_display = "Cancel";
            }

            if (argument.Contains(confirm_cmd) && item_value_menu == 4 && confirm_pressed)
            {
                argument = "";
                confirm_pressed = false;
            }

            if (argument.Contains(confirm_cmd) && item_value_menu == 4 && command_select == 0)
            {
                if (!command_scan)
                {
                    command_scan = true;
                }
                item_value_menu=0;
                if (confirm_pressed)
                {
                    confirm_pressed = false;
                }
            }
            if (argument.Contains(confirm_cmd) && item_value_menu == 4 && command_select == 1)
            {
                if (!command_send)
                {
                    command_send = true;
                }
                item_value_menu = 0;
                if (confirm_pressed)
                {
                    confirm_pressed = false;
                }
            }
            if (argument.Contains(confirm_cmd) && item_value_menu == 4 && command_select == 2)
            {
                if (!command_reset)
                {
                    command_reset = true;
                }
                item_value_menu = 0;
                if (confirm_pressed)
                {
                    confirm_pressed = false;
                }
            }
            if (argument.Contains(confirm_cmd) && item_value_menu == 4 && command_select == 3)
            {
                item_value_menu = 0;
                command_reset = false;
                command_send = false;
                command_scan = false;
                if (confirm_pressed)
                {
                    confirm_pressed = false;
                }
            }

            if (argument.Contains(reset_cmd) || command_reset)
            {
                scan_complete = false;
                Echo("Scan reset.");
                transmit_complete = false;
                Echo("Transmission reset.");
                target_aquired_light_actual.Enabled = false;
                target_transmit_light_actual.Enabled = false;
                asteroidsDetected = false;                
                surface_found = false;
                command_reset = false;
            }

            if (argument.Contains(scan_cmd) || command_scan)
            {
                command_scan = false;
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

            } //scan argument


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




            if (argument.Contains(send_cmd) && scan_complete == true && transmit_complete == false|| command_send && scan_complete == true && transmit_complete == false)
            {
                command_send = false;
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

            Echo($"Channel: {tx_channel}");
            Echo("Target: " + surface_found);
            Echo("TX: " + target_coords.X + " TY: " + target_coords.Y + " TZ: " + target_coords.Z + " SafeD: " + safe_position + "m");
            Echo("Free scan: " + free_form);
            Echo("Asteroid detection: " + enable_asteroid_detection);
            Echo("Asteroid: " + asteroidsDetected);
            Echo("--------");
            Echo("PB Arguments:");
            Echo("========");
            Echo($"Increase selection menu = {up_val}");
            Echo($"Decrease selection menu = {down_val}");
            Echo($"Menu item = {menuitem_select}");
            Echo($"Iteration value = {iterate_cmd}");
            Echo($"Confirm = {confirm_cmd}");
            Echo("--------");
            Echo($"Direct commands:");
            Echo("--------");
            Echo($"Scan = {scan_cmd}");
            Echo($"Reset = {reset_cmd}");
            Echo($"Send  = {send_cmd}");
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

            StringBuilder display_string = new StringBuilder();
            display_string.Append('\n');
            display_string.Append($"GMDP {version} Running {icon} ({Math.Round((double)percent_battery_power,1)})%");
            display_string.Append('\n');
            display_string.Append($"Channel: {tx_channel}");
            display_string.Append('\n');
            display_string.Append('\n');
            display_string.Append("Target: " + surface_found);
            display_string.Append('\n');
            display_string.Append("TX: " + target_coords.X + " TY: " + target_coords.Y + " TZ: " + target_coords.Z);
            display_string.Append('\n');
            display_string.Append('\n');
            display_string.Append($"Adjust value: {iterate_val}");
            display_string.Append('\n');
            display_string.Append($"{sel_left_1} Surface distance: {safe_position}m {sel_right_1}");
            display_string.Append('\n');
            if (free_form)
            {
                display_string.Append($"{sel_left_2}Align Depth: {free_center_position}m {sel_right_2}");
            }
            display_string.Append('\n');
            display_string.Append('\n');
            display_string.Append($"{sel_left_3} Scan type: {scan_type_display} {sel_right_3}");

            display_string.Append('\n');
            display_string.Append($"{sel_left_4} Command: {command_display} {sel_right_4}");
            display_string.Append('\n');
            display_string.Append('\n');
            display_string.Append('\n');
            if (enable_asteroid_detection)
            {
                display_string.Append("Asteroid detection: " + enable_asteroid_detection);
                display_string.Append('\n');
            }
            if (free_form)
            {
                display_string.Append("Free scan: " + free_form);
                display_string.Append('\n');
            }
            if (enable_asteroid_detection)
            {
                display_string.Append($"Asteroid: {asteroidsDetected} Sensor: {sensor_actual.IsActive}");
                display_string.Append('\n');
            }
            display_string.Append("Scan: " + scan_complete);
            display_string.Append('\n');
            display_string.Append("Transmit: " + transmit_complete);
            if (asteroidsDetected == true)
            {
                display_string.Append('\n');
                display_string.Append("AX: " + Math.Round(asteroid_coords.X, 2) + " AY: " + Math.Round(asteroid_coords.Y, 2) + " AZ: " + Math.Round(asteroid_coords.Z, 2));
            }
            if (free_form && scan_complete)
            {
                display_string.Append('\n');
                display_string.Append("FX: " + Math.Round(free_centre_target_coords.X, 2) + " FY: " + Math.Round(free_centre_target_coords.Y, 2) + " FZ: " + Math.Round(free_centre_target_coords.Z, 2));
            }
            if (display_surface_1 != null)
            {
                display_surface_1.WriteText(display_string);
            }
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
            lcd_display_name = "[" + scout_tag + " " + drone_id + " " + lcd_display_tag + "]";
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


        //end program
    }
}

