using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
        //Mining controller spotter drone V0.308A
        #region mdk preserve
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        int drone_id = 1;
        string drone_tag = "SWRM_D";
        string scout_tag = "PSMD";
        double safe_position = 30.0;
        double raycast_scan_distance = 32.0;
        //statics        
        string scan_cmd = "scan";
        string reset_cmd = "reset";
        string send_cmd = "send";
        string ast_en_cmd = "asten";
        string ast_dis_cmd = "astdis";
        string retry_send_cmd = "retx";

        #endregion
        string version = "V0.308";
        string drone_id_name = "";
        string tx_channel = "";
        string light_transmit_tag = "";
        string light_target_tag = "";
        string txl = "TX";
        string tgt = "TX";
        string prospC = "prospector";
        float t_stored_power;
        float stored_power_total;
        float t_max_power;
        float max_power_total;
        float t_current_power;
        float current_power_total;

        bool scan_complete = false;
        bool surface_found = false;
        bool asteroidsDetected = false;
        bool transmit_complete = false;
        bool enable_asteroid_detection;
        double distance_scan = 0.0;
        string data_out;

        IMyRadioAntenna antenna_actual;
        IMySensorBlock sensor_actual;
        IMyCameraBlock camera_actual;
        IMyRemoteControl remote_control_actual;
        IMyBatteryBlock currentbatteryblock;
        IMyLightingBlock target_aquired_light_actual;
        IMyLightingBlock target_transmit_light_actual;
        IMyTextPanel LCD_actual;

        Vector3D surface_coords;
        Vector3D target_coords;
        Vector3D asteroid_coords;
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
        List<IMyLightingBlock> lighting_all;
        List<IMyLightingBlock> lighting_target_aquired;
        List<IMyLightingBlock> lighting_target_transmit;
        List<IMyTextPanel> lcd_all;
        List<IMyTextPanel> lcd_tag;
        List<IMyThrust> thrust_all;
        List<IMyThrust> thrust_tag;
        List<IMyShipConnector> connector_all;
        List<IMyShipConnector> connector_tag;

        bool setup_complete = false;
        public void Save()
        {
        }



        public void Main(string argument, UpdateType updateSource)
        {
            IMyGridTerminalSystem gts = GridTerminalSystem as IMyGridTerminalSystem;
            if (!setup_complete)
            {
                drone_id_name = "[" + scout_tag + " " + drone_id + "]";
                tx_channel = drone_tag + " " + prospC;
                light_transmit_tag = "[" + scout_tag + " " + drone_id + " " + txl + "]";
                light_target_tag = "[" + scout_tag + " " + drone_id + " " + tgt + "]";
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
                lighting_all = new List<IMyLightingBlock>();
                lighting_target_aquired = new List<IMyLightingBlock>();
                lighting_target_transmit = new List<IMyLightingBlock>();
                lcd_all = new List<IMyTextPanel>();
                lcd_tag = new List<IMyTextPanel>();
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
                            n = antenna_all[i].CustomName;
                            antenna_all[i].CustomName = n +" " + drone_id_name + " " + "[" + tx_channel + "]";
                        }
                        if (antenna_all[i].CustomName.Contains(drone_id_name))
                        {
                            if (!antenna_all[i].CustomName.Contains(tx_channel))
                            {
                                n = antenna_all[i].CustomName;
                                antenna_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            antenna_tag.Add(antenna_all[i]);
                        }
                    }
                }
                // find remote control block
                gts.GetBlocksOfType<IMyRemoteControl>(remote_control_all, b => b.CubeGrid == Me.CubeGrid);
                if (remote_control_all.Count > 0)
                {
                    for (int i = 0; i < remote_control_all.Count; i++)
                    {
                        //create new array from search array with containers matching tag
                        if (!remote_control_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = remote_control_all[i].CustomName;
                            remote_control_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                        }
                        if (remote_control_all[i].CustomName.Contains(drone_id_name))
                        {
                            if (!remote_control_all[i].CustomName.Contains(tx_channel))
                            {
                                n = remote_control_all[i].CustomName;
                                remote_control_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            remote_control_tag.Add(remote_control_all[i]);
                        }
                    }
                }
                //get camera for raycast distance to planet reglar for test
                gts.GetBlocksOfType<IMyCameraBlock>(camera_all, b => b.CubeGrid == Me.CubeGrid);
                if (camera_all.Count > 0)
                {
                    for (int i = 0; i < camera_all.Count; i++)
                    {
                        //create new array from search array with containers matching tag
                        if (!camera_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = camera_all[i].CustomName;
                            camera_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                        }
                        if (camera_all[i].CustomName.Contains(drone_id_name))
                        {
                            if (!camera_all[i].CustomName.Contains(tx_channel))
                            {
                                n = camera_all[i].CustomName;
                                camera_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            camera_tag.Add(camera_all[i]);
                        }
                    }
                }
                //populate array with batteries on grid(s) 

                gts.GetBlocksOfType<IMyBatteryBlock>(batteries_all, b => b.CubeGrid == Me.CubeGrid);
                if (batteries_all.Count > 0)
                {
                    for (int i = 0; i < batteries_all.Count; i++)
                    {
                        if (!batteries_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = batteries_all[i].CustomName;
                            batteries_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                        }
                        //create new array from search array with batteries matching tag
                        if (batteries_all[i].CustomName.Contains(drone_id_name))
                        {
                            if (!batteries_all[i].CustomName.Contains(tx_channel))
                            {
                                n = batteries_all[i].CustomName;
                                batteries_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            batteries_tag.Add(batteries_all[i]);
                        }
                    }
                }
                // find remote control block
                gts.GetBlocksOfType<IMyTextPanel>(lcd_all, b => b.CubeGrid == Me.CubeGrid);
                if (lcd_all.Count > 0)
                {
                    for (int i = 0; i < lcd_all.Count; i++)
                    {
                        //create new array from search array with containers matching tag
                        if (!lcd_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = lcd_all[i].CustomName;
                            lcd_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                        }
                        if (lcd_all[i].CustomName.Contains(drone_id_name))
                        {
                            if (!lcd_all[i].CustomName.Contains(tx_channel))
                            {
                                n = lcd_all[i].CustomName;
                                lcd_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            lcd_tag.Add(lcd_all[i]);
                        }
                    }
                }
                //populate light lists

                gts.GetBlocksOfType<IMyLightingBlock>(lighting_all, b => b.CubeGrid == Me.CubeGrid);
                if (lighting_all.Count > 0)

                {
                    for (int i = 0; i < lighting_all.Count; i++)
                    {
                        //create new array from search array with lights matching tag
                        if (lighting_all[i].CustomName.Contains(light_target_tag))
                        {
                            if (!lighting_all[i].CustomName.Contains(tx_channel))
                            {
                                n = lighting_all[i].CustomName;
                                lighting_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            lighting_target_aquired.Add(lighting_all[i]);

                        }
                       
                        if (lighting_all[i].CustomName.Contains(light_transmit_tag))
                        {
                            if (!lighting_all[i].CustomName.Contains(tx_channel))
                            {
                                n = lighting_all[i].CustomName;
                                lighting_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            lighting_target_transmit.Add(lighting_all[i]);
                        }
                    }
                }
                //find sensors with tag
                gts.GetBlocksOfType<IMySensorBlock>(sensor_all, b => b.CubeGrid == Me.CubeGrid);
                if (sensor_all.Count > 0)
                {

                    for (int i = 0; i < sensor_all.Count; i++)
                    {
                        if (!sensor_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = sensor_all[i].CustomName;
                            sensor_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                        }
                        if (sensor_all[i].CustomName.Contains(drone_id_name))
                        {
                            if (!sensor_all[i].CustomName.Contains(tx_channel))
                            {
                                n = sensor_all[i].CustomName;
                                sensor_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            sensor_tag.Add(sensor_all[i]);
                        }
                    }
                }
                gts.GetBlocksOfType<IMyThrust>(thrust_all, b => b.CubeGrid == Me.CubeGrid);
                if (thrust_all.Count > 0)
                {

                    for (int i = 0; i < thrust_all.Count; i++)
                    {
                        if (!thrust_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = thrust_all[i].CustomName;
                            thrust_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                        }
                        if (thrust_all[i].CustomName.Contains(drone_id_name))
                        {
                            if (!thrust_all[i].CustomName.Contains(tx_channel))
                            {
                                n = thrust_all[i].CustomName;
                                thrust_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            thrust_tag.Add(thrust_all[i]);
                        }
                    }
                }
                gts.GetBlocksOfType<IMyShipConnector>(connector_all, b => b.CubeGrid == Me.CubeGrid);
                if (connector_all.Count > 0)
                {

                    for (int i = 0; i < connector_all.Count; i++)
                    {
                        if (!connector_all[i].CustomName.Contains(drone_id_name))
                        {
                            n = connector_all[i].CustomName;
                            connector_all[i].CustomName = n + " " + drone_id_name + " " + "[" + tx_channel + "]";
                        }
                        if (connector_all[i].CustomName.Contains(drone_id_name))
                        {
                            if (!connector_all[i].CustomName.Contains(tx_channel))
                            {
                                n = connector_all[i].CustomName;
                                connector_all[i].CustomName = n + " " + "[" + tx_channel + "]";
                            }
                            connector_tag.Add(connector_all[i]);
                        }
                    }
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


            //find remote control, end if not found
            if (remote_control_tag.Count <= 0 || remote_control_tag[0] == null)
            {
                Echo($"Remote control with tag: '{drone_id_name}' not found.");
                return;
            }
            //assign main remote control
            remote_control_actual = remote_control_tag[0];


            //find camera, end if not found
            if (camera_tag.Count <= 0 || camera_tag[0] == null)
            {
                Echo($"Camera with tag: '{drone_id_name}' not found.");
                return;
            }

            camera_actual = camera_tag[0];
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
            float percent_battery_power = 0.0f;
            for (int i = 0; i < batteries_tag.Count; i++)
            {
                currentbatteryblock = (IMyBatteryBlock)gts.GetBlockWithName(batteries_tag[i].CustomName) as IMyBatteryBlock;
                //record stored and max battery capacity
                t_stored_power = currentbatteryblock.CurrentStoredPower;
                stored_power_total = stored_power_total + t_stored_power;
                t_max_power = currentbatteryblock.MaxStoredPower;
                max_power_total = max_power_total + t_max_power;
                //record current power output
                t_current_power = currentbatteryblock.CurrentOutput;
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


            //find remote control, end if not found
            if (lcd_tag.Count <= 0 || lcd_tag[0] == null)
            {
                Echo($"LCD display with tag: '{drone_id_name}' not found.");
                return;
            }
            LCD_actual = lcd_tag[0];

            //Logic Start
            Echo($"GMDP {version} Running...");
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



            if (argument.Contains(reset_cmd))
            {
                scan_complete = false;
                Echo("Scan reset.");
                transmit_complete = false;
                Echo("Transmission reset.");
                target_aquired_light_actual.Enabled = false;
                target_transmit_light_actual.Enabled = false;
            }

            if (argument.Contains(scan_cmd))
            {
                /* if (scan_complete == true)
                 {
                     scan_complete = false;
                 } */

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
                        //List<MyDetectedEntityInfo> asteroids = new List<MyDetectedEntityInfo>();
                        // asteroids.Clear();
                        //sensor_actual.DetectedEntities(asteroids);
                        if (sensor_actual.IsActive == true && enable_asteroid_detection == true)
                        {
                            asteroidsDetected = true;
                            Echo("Asteroid detected");
                        }

                        if (sensor_actual.IsActive == false && enable_asteroid_detection == true || enable_asteroid_detection == false)
                        {
                            asteroidsDetected = false;
                        }

                        if (asteroidsDetected == false)
                        {
                            //set vector to gravity
                            gravity = remote_control_actual.GetNaturalGravity();
                            TargetVec = Vector3D.Normalize(new Vector3D(-gravity));
                            Echo("align to gravity");
                        }

                        if (asteroidsDetected == true)
                        {
                            //set vector to asteroid                           
                            asteroid_coords = sensor_actual.LastDetectedEntity.BoundingBox.Center;
                            TargetVec = Vector3D.Normalize(new Vector3D(-(asteroid_coords - surface_coords)));
                            Echo("align to asteroid");
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

            if (argument.Contains(retry_send_cmd))
            {
                if (transmit_complete == true)
                {
                    transmit_complete = false;
                }

            }




            if (argument.Contains(send_cmd) && scan_complete == true && transmit_complete == false)
            {
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
                data_out = comms_out.ToString();
                Me.CustomData = copy_target.ToString();

                IGC.SendBroadcastMessage(tx_channel, data_out, TransmissionDistance.TransmissionDistanceMax);
                //build data string to for mining controller and transmit via IGC
                transmit_complete = true;
                target_transmit_light_actual.Enabled = true;
                Echo("Data sent.");

            }

            Echo("Target: " + surface_found);
            Echo("TX: " + target_coords.X + " TY: " + target_coords.Y + " TZ: " + target_coords.Z + " SafeD: " + safe_position + "m");
            Echo("Asteroid detection: " + enable_asteroid_detection);
            Echo("Asteroid: " + asteroidsDetected);
            Echo("--------");
            Echo("PB Arguments:");
            Echo("========");
            Echo($"Scan = {scan_cmd}");
            Echo($"Reset = {reset_cmd}");
            Echo($"Send  = {send_cmd}");
            Echo($"Asteroid EN = {ast_en_cmd}");
            Echo($"Asteroid DIS = {ast_dis_cmd}");
            Echo($"Reset send = {retry_send_cmd}");
            if (asteroidsDetected == true)
            {
                Echo("AX: " + Math.Round(asteroid_coords.X, 2) + " AY: " + Math.Round(asteroid_coords.Y, 2) + " AZ: " + Math.Round(asteroid_coords.Z, 2));
            }
            StringBuilder display_string = new StringBuilder();
            display_string.Append('\n');
            display_string.Append("Target: " + surface_found);
            display_string.Append('\n');
            display_string.Append("TX: " + target_coords.X + " TY: " + target_coords.Y + " TZ: " + target_coords.Z);
            display_string.Append('\n');
            display_string.Append("Surface distance: " + safe_position + "m");
            display_string.Append('\n');
            display_string.Append("Asteroid detection: " + enable_asteroid_detection);
            display_string.Append('\n');
            display_string.Append("Asteroid: " + asteroidsDetected);
            display_string.Append('\n');
            display_string.Append("Scan: " + scan_complete);
            display_string.Append('\n');
            display_string.Append("Transmit: " + transmit_complete);
            if (asteroidsDetected == true)
            {
                display_string.Append("AX: " + Math.Round(asteroid_coords.X, 2) + " AY: " + Math.Round(asteroid_coords.Y, 2) + " AZ: " + Math.Round(asteroid_coords.Z, 2));
            }
            LCD_actual.WriteText(display_string);


        } //end void main




        //end program
    }
}
