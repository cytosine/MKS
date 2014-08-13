﻿//Written by Flip van Toly for KSP community
//released under CC 3.0 Share Alike license

using System;
using System.Collections.Generic;
using System.Linq;
using LibNoise.Unity.Operator;
using Tac;
using UnityEngine;

namespace KolonyTools
{
    public class MKSLcentral : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiName = ">>")]
        public string status;

        //GUI
        private static GUIStyle windowStyle, labelStyle, redlabelStyle, textFieldStyle, buttonStyle;

        //GUI Main
        private static Rect windowPosGUIMain = new Rect(200, 200, 175, 450);
        private Vector2 scrollPositionGUICurrentTransfers;
        private Vector2 scrollPositionGUIPreviousTransfers;

        //GUI Edit
        private static Rect windowPosGUIEdit = new Rect(250, 250, 400, 450);
        private Vector2 scrollPositionEditGUIResources;

        //private bool newedit = false;
        private MKSLtransfer editGUITransfer = new MKSLtransfer();
        private MKSLresource editGUIResource = new MKSLresource();

        private List<Vessel> bodyVesselList = new List<Vessel>();
        private int vesselFrom = 0;
        private int vesselTo = 0;
              
        private string StrAmount = "0";
        private double currentAvailable;
        private string StrValidationMessage = "";

        //GUI View
        private bool viewCurrent = false;
        private static Rect windowPosGUIView = new Rect(270, 370, 175, 200);
        private MKSLtransfer viewGUITransfer = new MKSLtransfer();

        //string with the resources for which the partmodule must function
        [KSPField(isPersistant = false, guiActive = false)]
        public string ManagedResources = "";

        //Delivery time variables
        [KSPField(isPersistant = false, guiActive = false)]
        public float PrepTime = 1; 
        [KSPField(isPersistant = false, guiActive = false)]
        public float TimePerDistance = 1; 
        [KSPField(isPersistant = false, guiActive = false)]
        public float TimeToFromLO = 1;

        [KSPField(isPersistant = false, guiActive = false)]
        public string TimePerDistancePlanet = "";
        [KSPField(isPersistant = false, guiActive = false)]
        public string TimeToFromLOPlanet = "";

        //Delivery cost variables
        [KSPField(isPersistant = false, guiActive = false)]
        public float DistanceModifier = 1;
        [KSPField(isPersistant = false, guiActive = false)]
        public float SurfaceOrbitModifier = 1;
        [KSPField(isPersistant = false, guiActive = false)]
        public float OrbitSurfaceModifier = 1;
        
        [KSPField(isPersistant = false, guiActive = false)]
        public float AtmosphereUpModifier = 1;
        [KSPField(isPersistant = false, guiActive = false)]
        public float AtmosphereDownModifier = 1;
        
        [KSPField(isPersistant = false, guiActive = false)]
        public string DistanceModifierPlanet = "";
        [KSPField(isPersistant = false, guiActive = false)]
        public string SurfaceOrbitModifierPlanet = "";
        [KSPField(isPersistant = false, guiActive = false)]
        public string OrbitSurfaceModifierPlanet = "";

        [KSPField(isPersistant = false, guiActive = false)]
        public string Mix1CostName = "";
        [KSPField(isPersistant = false, guiActive = false)]
        public string Mix1CostResources = "";

        [KSPField(isPersistant = false, guiActive = false)]
        public string Mix2CostName = "";
        [KSPField(isPersistant = false, guiActive = false)]
        public string Mix2CostResources = "";

        [KSPField(isPersistant = false, guiActive = false)]
        public string Mix3CostName = "";
        [KSPField(isPersistant = false, guiActive = false)]
        public string Mix3CostResources = "";

        [KSPField(isPersistant = false, guiActive = false)]
        public string Mix4CostName = "";
        [KSPField(isPersistant = false, guiActive = false)]
        public string Mix4CostResources = "";

        [KSPField(isPersistant = false, guiActive = false)]
        public float maxTransferMass = 1000000f;

        //strings in which planned and completed tranfers are saved for persitance
        public List<MKSLtransfer> currentTransfers = new List<MKSLtransfer>();
        [KSPField(isPersistant = true, guiActive = false)]
        public MKSLTranferList saveCurrentTransfersList = new MKSLTranferList();
        public List<MKSLtransfer> previousTransfers = new List<MKSLtransfer>();
        [KSPField(isPersistant = true, guiActive = false)]
        public MKSLTranferList savePreviousTransfersList = new MKSLTranferList();

        private KSPLog _log = new KSPLog();

        /// <summary>
        /// Main window
        /// </summary>

        private void initStyle()
        {
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            windowStyle.stretchWidth = false;
            windowStyle.stretchHeight = false;

            labelStyle = new GUIStyle(HighLogic.Skin.label);
            labelStyle.stretchWidth = false;
            labelStyle.stretchHeight = false;


            redlabelStyle = new GUIStyle(HighLogic.Skin.label);
            redlabelStyle.stretchWidth = false;
            redlabelStyle.stretchHeight = false;
            redlabelStyle.normal.textColor = Color.red;

            textFieldStyle = new GUIStyle(HighLogic.Skin.textField);
            textFieldStyle.stretchWidth = false;
            textFieldStyle.stretchHeight = false;

            buttonStyle = new GUIStyle(HighLogic.Skin.button);
            buttonStyle.stretchHeight = false;
            buttonStyle.stretchWidth = false;
        }

        private void WindowGUIMain(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 150, 30));
            GUILayout.BeginVertical();

            if (GUILayout.Button("New Transfer", buttonStyle, GUILayout.Width(150)))
            {
                //newedit = true;
                makeBodyVesselList();

                editGUITransfer = new MKSLtransfer();
                System.Random rnd = new System.Random();
                editGUITransfer.transferName = rnd.Next(100000, 999999).ToString();
                editGUITransfer.initTransferList(ManagedResources);
                editGUITransfer.initCostList(Mix1CostResources);
                editGUITransfer.vesselFrom = vessel;
                editGUITransfer.vesselTo = vessel;

                editGUIResource = editGUITransfer.transferList[0];
                StrAmount = "0";
                currentAvailable = readResource(editGUITransfer.vesselFrom, editGUIResource.resourceName);

                openGUIEdit();
            }

            GUILayout.Label("Current transfers", labelStyle, GUILayout.Width(150));
            scrollPositionGUICurrentTransfers = GUILayout.BeginScrollView(scrollPositionGUICurrentTransfers, false,true, GUILayout.Width(160), GUILayout.Height(180));
            foreach (MKSLtransfer trans in currentTransfers)
            {
                if (GUILayout.Button(trans.transferName + " (" + deliveryTimeString(trans.arrivaltime, Planetarium.GetUniversalTime()) + ")", buttonStyle, GUILayout.Width(135), GUILayout.Height(22)))
                {
                    viewCurrent = true;
                    viewGUITransfer = trans;
                    openGUIView();
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Label("Previous tranfers", labelStyle, GUILayout.Width(150));
            scrollPositionGUIPreviousTransfers = GUILayout.BeginScrollView(scrollPositionGUIPreviousTransfers, false, true, GUILayout.Width(160), GUILayout.Height(80));
            foreach (MKSLtransfer trans in previousTransfers)
            {
                if (GUILayout.Button(trans.transferName + " " + (trans.delivered == true ? "succes" : "failure"), buttonStyle, GUILayout.Width(135), GUILayout.Height(22)))
                {
                    viewCurrent = false;
                    viewGUITransfer = trans;
                    openGUIView();
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Label("", labelStyle, GUILayout.Width(150));
            if (GUILayout.Button("Close", buttonStyle, GUILayout.Width(150)))
            {
                closeGUIMain();
            }
            GUILayout.EndVertical();
        }

        private void drawGUIMain()
        {
            windowPosGUIMain = GUILayout.Window(1404, windowPosGUIMain, WindowGUIMain, "Kolony Logistics", windowStyle);
        }

        [KSPEvent(name = "Kolony Logistics", isDefault = false, guiActive = true, guiName = "Kolony Logistics")]
        public void openGUIMain()
        {
            _log.LogWarning("[MKS] guiMain running");
            initStyle();
            
            loadTransferList(currentTransfers, saveCurrentTransfersList, true);
            loadTransferList(previousTransfers, savePreviousTransfersList, false);
            RenderingManager.AddToPostDrawQueue(140, new Callback(drawGUIMain));
        }

        public void closeGUIMain()
        {
            RenderingManager.RemoveFromPostDrawQueue(140, new Callback(drawGUIMain));
        }

        /// <summary>
        /// GUI Edit
        /// </summary>

        private void WindowGUIEdit(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 400, 30));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<<", buttonStyle, GUILayout.Width(40)))
            {
                previousBodyVesselList(ref vesselFrom);
                editGUITransfer.vesselFrom = bodyVesselList[vesselFrom];
            }
            GUILayout.Label("From:", labelStyle, GUILayout.Width(60));
            GUILayout.Label(editGUITransfer.vesselFrom.vesselName, labelStyle, GUILayout.Width(160));
            if (GUILayout.Button(">>", buttonStyle, GUILayout.Width(40)))
            {
                nextBodyVesselList(ref vesselFrom);
                editGUITransfer.vesselFrom = bodyVesselList[vesselFrom];
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<<", buttonStyle, GUILayout.Width(40)))
            {
                previousBodyVesselList(ref vesselTo);
                editGUITransfer.vesselTo = bodyVesselList[vesselTo];
            }
            GUILayout.Label("To:", labelStyle, GUILayout.Width(60));
            GUILayout.Label(editGUITransfer.vesselTo.vesselName, labelStyle, GUILayout.Width(160));
            if (GUILayout.Button(">>", buttonStyle, GUILayout.Width(40)))
            {
                nextBodyVesselList(ref vesselTo);
                editGUITransfer.vesselTo = bodyVesselList[vesselTo];
            }
            GUILayout.EndHorizontal();



            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical();
            scrollPositionEditGUIResources = GUILayout.BeginScrollView(scrollPositionEditGUIResources, GUILayout.Width(200), GUILayout.Height(150));
            foreach (MKSLresource res in editGUITransfer.transferList)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(res.resourceName + ": " + Math.Round(res.amount, 2).ToString()))
                {
                    editGUIResource = res;
                    StrAmount = Math.Round(res.amount, 2).ToString();
                    currentAvailable = readResource(editGUITransfer.vesselFrom, editGUIResource.resourceName);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();

            if (editGUIResource != null)
            {
                   GUILayout.BeginHorizontal();
                   GUILayout.Label("Resource:", labelStyle, GUILayout.Width(80));
                   GUILayout.Label(editGUIResource.resourceName, labelStyle, GUILayout.Width(100));
                   GUILayout.EndHorizontal();
                   
                   GUILayout.BeginHorizontal();
                   GUILayout.Label("Amount:", labelStyle, GUILayout.Width(80));
                   StrAmount = GUILayout.TextField(StrAmount, 10, textFieldStyle, GUILayout.Width(60));
                   if (GUILayout.Button("Set", buttonStyle, GUILayout.Width(30)))
                   {
                       double number = 0;
                       if (Double.TryParse(StrAmount, out number))
                       {
                           if (number < currentAvailable)
                               editGUIResource.amount = number;
                           else
                               editGUIResource.amount = currentAvailable;
                           StrAmount = Math.Round(number, 2).ToString();
                       }
                       else
                       {
                           StrAmount = "0";
                           editGUIResource.amount = 0;
                       }
                       editGUIResource.amount = Convert.ToDouble(StrAmount);
                       updateCostList(editGUITransfer);
                       validateTransfer(editGUITransfer,ref StrValidationMessage);
                   }
                   GUILayout.EndHorizontal();
                   
                   GUILayout.BeginHorizontal();
                   GUILayout.Label("Mass:", labelStyle, GUILayout.Width(80));
                   GUILayout.Label(Math.Round(editGUIResource.mass(), 2).ToString(), labelStyle, GUILayout.Width(100));
                   GUILayout.EndHorizontal();
                   
                   GUILayout.BeginHorizontal();
                   GUILayout.Label("Available:", labelStyle, GUILayout.Width(80));
                   GUILayout.Label(Math.Round(currentAvailable, 2).ToString(), labelStyle, GUILayout.Width(100));
                   GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Label("Tranfer Mass: " + Math.Round(editGUITransfer.totalMass(),2).ToString() + " (maximum: " + maxTransferMass.ToString() + ")", labelStyle, GUILayout.Width(300));

            GUILayout.Label("");
            GUILayout.BeginHorizontal();
            if (Mix1CostName != "")
            {
                if (GUILayout.Button(Mix1CostName, buttonStyle, GUILayout.Width(170)))
                {
                    editGUITransfer.initCostList(Mix1CostResources);
                    updateCostList(editGUITransfer);
                }
            }
            if (Mix2CostName != "")
            {
                if (GUILayout.Button(Mix2CostName, buttonStyle, GUILayout.Width(170)))
                {
                    editGUITransfer.initCostList(Mix2CostResources);
                    updateCostList(editGUITransfer);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (Mix3CostName != "")
            {
                if (GUILayout.Button(Mix3CostName, buttonStyle, GUILayout.Width(170)))
                {
                    editGUITransfer.initCostList(Mix3CostResources);
                    updateCostList(editGUITransfer);
                }
            }
            if (Mix4CostName != "")
            {
                if (GUILayout.Button(Mix4CostName, buttonStyle, GUILayout.Width(170)))
                {
                    editGUITransfer.initCostList(Mix4CostResources);
                    updateCostList(editGUITransfer);
                }
            }
            GUILayout.EndHorizontal();

            foreach (MKSLresource resCost in editGUITransfer.costList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(resCost.resourceName + ":", labelStyle, GUILayout.Width(100));
                GUILayout.Label(Math.Round(resCost.amount, 2).ToString(), labelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();
            }


            GUILayout.Label(StrValidationMessage, redlabelStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Initiate Transfer", buttonStyle, GUILayout.Width(200)))
            {
                updateCostList(editGUITransfer);
                if (validateTransfer(editGUITransfer, ref StrValidationMessage))
                {
                    createTransfer(editGUITransfer);
                }
            }
            if (GUILayout.Button("Cancel", buttonStyle, GUILayout.Width(100)))
            {
                closeGUIEdit();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        
        private void drawGUIEdit()
        {
            windowPosGUIEdit = GUILayout.Window(1414, windowPosGUIEdit, WindowGUIEdit, "Transfer " + editGUITransfer.transferName, windowStyle);
        }

        public void openGUIEdit()
        {
            makeBodyVesselList();
            RenderingManager.AddToPostDrawQueue(141, new Callback(drawGUIEdit));
        }

        public void closeGUIEdit()
        {
            RenderingManager.RemoveFromPostDrawQueue(141, new Callback(drawGUIEdit));
        }

        /// <summary>
        /// GUI View
        /// </summary>
        private void WindowGUIView(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 150, 30));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("From:", labelStyle, GUILayout.Width(50));
            GUILayout.Label(viewGUITransfer.vesselFrom.vesselName, labelStyle, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("To:", labelStyle, GUILayout.Width(50));
            GUILayout.Label(viewGUITransfer.vesselTo.vesselName, labelStyle, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Arrival:", labelStyle, GUILayout.Width(50));
            GUILayout.Label(deliveryTimeString(viewGUITransfer.arrivaltime, Planetarium.GetUniversalTime()), labelStyle, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.Label("");
            GUILayout.Label("Transfer", labelStyle, GUILayout.Width(100));
            foreach (MKSLresource res in viewGUITransfer.transferList)
            {
                if (res.amount > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(res.resourceName, labelStyle, GUILayout.Width(150));
                    GUILayout.Label(Math.Round(res.amount, 2).ToString(), labelStyle, GUILayout.Width(50));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.Label("Transfer Mass: " + Math.Round(viewGUITransfer.totalMass(), 2).ToString(), labelStyle, GUILayout.Width(150));
            GUILayout.Label("");
 
            GUILayout.Label("Cost", labelStyle, GUILayout.Width(100));
            foreach (MKSLresource res in viewGUITransfer.costList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(res.resourceName, labelStyle, GUILayout.Width(100));
                GUILayout.Label(Math.Round(res.amount, 2).ToString(), labelStyle, GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }

            if (viewCurrent == true)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Close", buttonStyle, GUILayout.Width(75)))
                {
                    closeGUIView();
                }
                if (GUILayout.Button("Remove", buttonStyle, GUILayout.Width(75)))
                {
                    removeCurrentTranfer(viewGUITransfer);
                    closeGUIView();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Close", buttonStyle, GUILayout.Width(150)))
                {
                    closeGUIView();
                }
            }
            GUILayout.EndVertical();
        }

        private void drawGUIView()
        {
            windowPosGUIView = GUILayout.Window(1424, windowPosGUIView, WindowGUIView, "Transfer " + viewGUITransfer.transferName, windowStyle);
        }

        public void openGUIView()
        {
            closeGUIView();
            RenderingManager.AddToPostDrawQueue(142, new Callback(drawGUIView));
        }

        public void closeGUIView()
        {
            RenderingManager.RemoveFromPostDrawQueue(142, new Callback(drawGUIView));
        }

        //return a day-hour-minute-seconds-time format for the delivery time
        public string deliveryTimeString(double deliveryTime, double currentTime)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            double time = 0;
            if (deliveryTime > currentTime)
                time = deliveryTime - currentTime;
            else
                time = currentTime - deliveryTime;


            days = (int)Math.Floor(time / 21600);
            time = time - (days * 21600);

            hours = (int)Math.Floor(time / 3600);
            time = time - (hours * 3600);

            minutes = (int)Math.Floor(time / 60);
            time = time - (minutes * 60);

            seconds = (int)Math.Floor(time);

            if (deliveryTime > currentTime)
                return (days.ToString() + "d" + hours.ToString() + "h" + minutes.ToString() + "m" + seconds + "s");
            else
                return ("-" + days.ToString() + "d" + hours.ToString() + "h" + minutes.ToString() + "m" + seconds + "s");
            
        }

        /// <summary>
        /// vessel list
        /// </summary>
        //make a list of all valid tranfer vessels on this celestial body.
        private void makeBodyVesselList()
        {
            bodyVesselList.Clear();
            foreach (Vessel ves in FlightGlobals.Vessels)
            {
                if (vessel.mainBody.name == ves.mainBody.name && ves.vesselType != VesselType.Debris && ves.vesselType != VesselType.SpaceObject && ves.vesselType != VesselType.Unknown
                    && (ves.situation == Vessel.Situations.ORBITING || ves.situation == Vessel.Situations.SPLASHED || ves.situation == Vessel.Situations.LANDED))
                {
                    bodyVesselList.Add(ves);
                }
            }
        }

        //go to previous entry in vessel list for this body
        public void previousBodyVesselList(ref int ListPosition)
        {
            if (ListPosition <= 0)
            {
                ListPosition = bodyVesselList.Count - 1;
                
            }
            else
            {
                ListPosition = ListPosition - 1;
            }
        }

        //go to next entry in vessel list for this body
        public void nextBodyVesselList(ref int ListPosition)
        {
            if (ListPosition >= bodyVesselList.Count - 1)
            {
                ListPosition = 0;
            }
            else
            {
                ListPosition = ListPosition + 1;
            }
        }


        /// <summary>
        /// manipulate save lists
        /// </summary>
        /// <param name="transferlist"></param>

        //converts a save string into a list of MKSLtransfers
        public void loadTransferList(List<MKSLtransfer> transferlist,MKSLTranferList savestring, bool longsave)
        {
            _log.LogWarning("[MKS] loading transferlist with "+savestring.Count+" transfers");
            transferlist.Clear();

            transferlist.AddRange(savestring);
            _log.LogWarning("[MKS] loaded " + transferlist.Count + " transfers");
            //if (savestring == "") { return; }
            //string[] deliveries = savestring.Split('@');

            //if (longsave)
            //{
            //    for (int i = deliveries.Length - 1; i >= 0; i--)
            //    {
            //        string[] geninfo = deliveries[i].Split('>');
            //        if (null != geninfo[3]) { deliveries[i] = geninfo[3]; }
            //    }
            //}

            //for (int i = deliveries.Length - 1; i >= 0; i--)
            //{
            //    MKSLtransfer trans = new MKSLtransfer();
            //    trans.loadstring(deliveries[i]);
            //    transferlist.Add(trans);
            //}
        }

        //removes an entry from the current transfers 
        public void removeCurrentTranfer(MKSLtransfer transRemove)
        {

            saveCurrentTransfersList.Remove(transRemove);
            savePreviousTransfersList.Add(transRemove);
            //remove from current transfers save string
            //string[] deliveries = saveCurrentTransfers.Split('@');
            //string newSaveCurrentTransfers = "";
            //foreach (String delivery in deliveries)
            //{
            //    string[] geninfo = delivery.Split('>');

            //    MKSLtransfer transfer = new MKSLtransfer();
            //    transfer.loadstring(geninfo[3]);
            //    string geninfo3 = geninfo[3];
            //    //if return is true then add the returned
            //    if (transfer.transferName != transRemove.transferName)
            //    {
            //        if (newSaveCurrentTransfers == "")
            //        {
            //            newSaveCurrentTransfers = delivery;

            //        }
            //        else
            //        {
            //            newSaveCurrentTransfers = saveCurrentTransfers + "@" + delivery;
            //        }
            //    }

            //}

            ////add to previous transfers save string
            //if (savePreviousTransfers == "")
            //{
            //    savePreviousTransfers = transRemove.savestring();
            //}
            //else
            //{
            //    savePreviousTransfers = savePreviousTransfers + "@" + transRemove.savestring();
            //}

            //saveCurrentTransfers = newSaveCurrentTransfers;

            loadTransferList(currentTransfers, saveCurrentTransfersList, true);
            loadTransferList(previousTransfers, savePreviousTransfersList, false);
        }

        /// <summary>
        /// transfer functions
        /// </summary>
        /// <param name="trans"></param>
        public bool validateTransfer(MKSLtransfer trans, ref string validationMess)
        {
            bool check = true;
            validationMess = "";


            //check if origin is not the same as destination
            if (trans.vesselFrom.id.ToString() == trans.vesselTo.id.ToString())
            {
                validationMess = "origin and destination are equal";
                return (false);
            }

            //check situation origin vessel
            if (trans.vesselFrom.situation != Vessel.Situations.ORBITING && trans.vesselFrom.situation != Vessel.Situations.SPLASHED && trans.vesselFrom.situation != Vessel.Situations.LANDED)
            {
                validationMess = "origin of transfer is not in a stable situation";
                return (false);
            }

            //check situation destination vessel
            if (trans.vesselTo.situation != Vessel.Situations.ORBITING && trans.vesselTo.situation != Vessel.Situations.SPLASHED && trans.vesselTo.situation != Vessel.Situations.LANDED)
            {
                validationMess = "destination of transfer is not in a stable situation";
                return (false);
            }

            //check for sufficient transfer resources
            foreach (MKSLresource transRes in trans.transferList)
            {
                if (readResource(trans.vesselFrom, transRes.resourceName) < transRes.amount)
                {
                    check = false;
                    validationMess = validationMess + "insufficient " + transRes.resourceName + "    ";
                }
            }

            //check for sufficient cost resources

            foreach (MKSLresource costRes in trans.costList)
            {
                double totalResAmount = 0;

                totalResAmount = costRes.amount;

                foreach (MKSLresource transRes in trans.transferList)
                {
                    if (costRes.resourceName == transRes.resourceName)
                    {
                        totalResAmount = totalResAmount + transRes.amount;
                    }
                }

                if ((readResource(trans.vesselFrom, costRes.resourceName) + readResource(vessel, costRes.resourceName)) < totalResAmount)
                {
                    check = false;
                    validationMess = validationMess + "insufficient " + costRes.resourceName + "    ";
                }
            }
                        
            if (check)
            {
                validationMess = "";
                _log.LogWarning("[MKS] valid transfer");
                return(true);
            }
            else
            {
                _log.LogWarning("[MKS] not a valid transfer");
                return(false);
            }
        }


        public void updateCostList(MKSLtransfer trans)
        {
            double PSSM = getValueFromStrPlanet(DistanceModifierPlanet, vessel.mainBody.name);
            double PSOM = getValueFromStrPlanet(SurfaceOrbitModifierPlanet, vessel.mainBody.name);
            double POSM = getValueFromStrPlanet(OrbitSurfaceModifierPlanet, vessel.mainBody.name); 

            double ATUP = 1;
            double ATDO = 1;
            if (vessel.mainBody.atmosphere)
            {
                ATUP = (double)AtmosphereUpModifier;
                ATDO = (double)AtmosphereDownModifier;
            }

            foreach (MKSLresource res in trans.costList)
            {
                ///take into account amount

                ///take into account celestialbody
                if ((trans.vesselFrom.protoVessel.situation == Vessel.Situations.LANDED || trans.vesselFrom.protoVessel.situation == Vessel.Situations.SPLASHED) &&
                    (trans.vesselTo.protoVessel.situation == Vessel.Situations.LANDED || trans.vesselTo.protoVessel.situation == Vessel.Situations.SPLASHED))
                {
                    double distance = GetDistanceBetweenPoints(trans.vesselFrom.protoVessel.latitude, trans.vesselFrom.protoVessel.longitude, trans.vesselTo.protoVessel.latitude, trans.vesselTo.protoVessel.longitude);
                    res.amount = res.costPerMass * trans.totalMass() * distance * vessel.mainBody.GeeASL * DistanceModifier * PSSM;
                }
                else if ((trans.vesselFrom.protoVessel.situation == Vessel.Situations.LANDED || trans.vesselFrom.protoVessel.situation == Vessel.Situations.SPLASHED) &&
                         (trans.vesselTo.protoVessel.situation == Vessel.Situations.ORBITING))
                {
                    res.amount = res.costPerMass * trans.totalMass() * vessel.mainBody.GeeASL * vessel.mainBody.Radius * ATUP * SurfaceOrbitModifier * PSOM;
                }
                else if ((trans.vesselFrom.protoVessel.situation == Vessel.Situations.ORBITING) &&
                         (trans.vesselTo.protoVessel.situation == Vessel.Situations.LANDED || trans.vesselTo.protoVessel.situation == Vessel.Situations.SPLASHED))
                {
                    res.amount = res.costPerMass * trans.totalMass() * vessel.mainBody.GeeASL * vessel.mainBody.Radius * ATDO * OrbitSurfaceModifier * POSM;
                }
                else //Working code - going to just use the same calc as surface to surface for orbit to orbit for now
                {
                    double distance = GetDistanceBetweenPoints(trans.vesselFrom.protoVessel.latitude, trans.vesselFrom.protoVessel.longitude, trans.vesselTo.protoVessel.latitude, trans.vesselTo.protoVessel.longitude);
                    res.amount = res.costPerMass * trans.totalMass() * distance * vessel.mainBody.GeeASL * DistanceModifier * PSSM;
                }

            }
        }

        public void updateArrivalTime(MKSLtransfer trans)
        {
            double prepT = (double)PrepTime;
            double TpD = getValueFromStrPlanet(TimePerDistancePlanet, vessel.mainBody.name);
            if (1 == TpD)
            {
                TpD = TimePerDistance;
            }
            double TtfLO = getValueFromStrPlanet(TimeToFromLOPlanet, vessel.mainBody.name);
            if (1 == TtfLO)
            {
                TtfLO = TimeToFromLO;
            }

            if ((trans.vesselFrom.protoVessel.situation == Vessel.Situations.LANDED || trans.vesselFrom.protoVessel.situation == Vessel.Situations.SPLASHED) &&
                (trans.vesselTo.protoVessel.situation == Vessel.Situations.LANDED || trans.vesselTo.protoVessel.situation == Vessel.Situations.SPLASHED))
            {
                double distance = GetDistanceBetweenPoints(trans.vesselFrom.protoVessel.latitude, trans.vesselFrom.protoVessel.longitude, trans.vesselTo.protoVessel.latitude, trans.vesselTo.protoVessel.longitude);
                trans.arrivaltime = Planetarium.GetUniversalTime() + prepT + (distance * TpD);
            }
            else if ((trans.vesselFrom.protoVessel.situation == Vessel.Situations.LANDED || trans.vesselFrom.protoVessel.situation == Vessel.Situations.SPLASHED) &&
                        (trans.vesselTo.protoVessel.situation == Vessel.Situations.ORBITING))
            {
                trans.arrivaltime = Planetarium.GetUniversalTime() + prepT + TtfLO;
            }
            else if ((trans.vesselFrom.protoVessel.situation == Vessel.Situations.ORBITING) &&
                        (trans.vesselTo.protoVessel.situation == Vessel.Situations.LANDED || trans.vesselTo.protoVessel.situation == Vessel.Situations.SPLASHED))
            {
                trans.arrivaltime = Planetarium.GetUniversalTime() + prepT + TtfLO;
            }
            else //More working code
            {
                double distance = GetDistanceBetweenPoints(trans.vesselFrom.protoVessel.latitude, trans.vesselFrom.protoVessel.longitude, trans.vesselTo.protoVessel.latitude, trans.vesselTo.protoVessel.longitude);
                trans.arrivaltime = Planetarium.GetUniversalTime() + prepT + (distance * TpD);
            }

        }

        private double getValueFromStrPlanet(string StrPlanet, string PlanetName)
        {
            string[] planets = StrPlanet.Split(',');
            foreach (String planet in planets)
            {
                string[] planetInfo = planet.Split(':');
                if (planetInfo[0] == PlanetName)
                    return (Convert.ToDouble(planetInfo[1]));
            }
            return (1);
        }

        public void createTransfer(MKSLtransfer trans)
        {
            _log.LogWarning("[MKS] creating transfer "+trans.longsavestring());
            foreach (MKSLresource costRes in trans.costList)
            {
                double AmountToGather = costRes.amount;
                double AmountGathered = 0;
                AmountGathered = AmountGathered + takeResources(vessel, costRes.resourceName, AmountToGather - AmountGathered);
                AmountGathered = AmountGathered + takeResources(trans.vesselFrom, costRes.resourceName, AmountToGather - AmountGathered);
            }

            foreach (MKSLresource transRes in trans.transferList)
            {
                transRes.amount = takeResources(trans.vesselFrom, transRes.resourceName, transRes.amount);
            }

            trans.delivered = false;
            updateArrivalTime(trans);

            if (trans.vesselTo.situation == Vessel.Situations.ORBITING)
            {
                trans.orbit = true;
                trans.SMA = trans.vesselTo.protoVessel.orbitSnapShot.semiMajorAxis;
                trans.ECC = trans.vesselTo.protoVessel.orbitSnapShot.eccentricity;
                trans.INC = trans.vesselTo.protoVessel.orbitSnapShot.inclination;
            }

            if (trans.vesselTo.situation == Vessel.Situations.LANDED || trans.vesselTo.situation == Vessel.Situations.SPLASHED)
            {
                trans.surface = true;
                trans.LON = trans.vesselTo.protoVessel.longitude;
                trans.LAT = trans.vesselTo.protoVessel.latitude;
            }

                saveCurrentTransfersList.Add(trans);
            
            loadTransferList(currentTransfers, saveCurrentTransfersList, true);
            closeGUIEdit();
            _log.LogWarning("[MKS] finished creating transfer");
        }

        /// <summary>
        ///  read and take resources
        /// </summary>
        public double readResource(Vessel ves ,string ResourceName)
        {
            double amountCounted = 0;
            if (ves.packed && !ves.loaded)
            {
                //Thanks to NathanKell for explaining how to access and edit parts of unloaded vessels and pointing me for some example code is NathanKell's own Mission Controller Extended mod!
                foreach (ProtoPartSnapshot p in ves.protoVessel.protoPartSnapshots)
                {
                    foreach (ProtoPartResourceSnapshot r in p.resources)
                    {
                        if (r.resourceName == ResourceName)
                        {
                            amountCounted = amountCounted + Convert.ToDouble(r.resourceValues.GetValue("amount"));
                        }
                    }
                }
            }
            else
            {
                foreach (Part p in ves.parts)
                {
                    foreach (PartResource r in p.Resources)
                    {
                        if (r.resourceName == ResourceName)
                        {
                            amountCounted = amountCounted + r.amount;
                        }
                    }
                } 
            }
            return amountCounted;
        }
        
        private double takeResources(Vessel ves, string ResourceName, double AmountToGather)
        {
            double AmountGathered = 0;

            if (ves.packed && !ves.loaded)
            {
                //Thanks to NathanKell for explaining how to access and edit parts of unloaded vessels and pointing me for some example code is NathanKell's own Mission Controller Extended mod!
                foreach (ProtoPartSnapshot p in ves.protoVessel.protoPartSnapshots)
                {
                    foreach (ProtoPartResourceSnapshot r in p.resources)
                    {
                        if (r.resourceName == ResourceName)
                        {
                            double AmountInPart = Convert.ToDouble(r.resourceValues.GetValue("amount"));

                            if (AmountInPart <= AmountToGather - AmountGathered)
                            {
                                AmountGathered = AmountGathered + AmountInPart;
                                r.resourceValues.SetValue("amount", "0");
                            }
                            else
                            {
                                r.resourceValues.SetValue("amount", Convert.ToString(AmountInPart - (AmountToGather - AmountGathered)));
                                AmountGathered = AmountToGather;
                            }
                        }
                    }
                } 
            }
            else
            {
                foreach (Part p in ves.parts)
                {
                    foreach (PartResource r in p.Resources)
                    {
                        if (r.resourceName == ResourceName)
                        {
                            if (r.amount <= AmountToGather - AmountGathered)
                            {
                                AmountGathered = AmountGathered + r.amount;
                                r.amount = 0;
                            }
                            else
                            {
                                r.amount = r.amount - (AmountToGather - AmountGathered);
                                AmountGathered = AmountToGather;
                            }
                        }
                    }
                }            
            }
            return (AmountGathered);
        }

        // adapted from: www.consultsarath.com/contents/articles/KB000012-distance-between-two-points-on-globe--calculation-using-cSharp.aspx
        public double GetDistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
        {
            double distance = 0;

            double dLat = (lat2 - lat1) / 180 * Math.PI;
            double dLong = (long2 - long1) / 180 * Math.PI;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                        + Math.Cos(lat2) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            //Calculate radius of planet
            // For this you can assume any of the two points.
            double radiusE = FlightGlobals.ActiveVessel.mainBody.Radius; // Equatorial radius, in metres
            double radiusP = FlightGlobals.ActiveVessel.mainBody.Radius; // Polar Radius

            //Numerator part of function
            double nr = Math.Pow(radiusE * radiusP * Math.Cos(lat1 / 180 * Math.PI), 2);
            //Denominator part of the function
            double dr = Math.Pow(radiusE * Math.Cos(lat1 / 180 * Math.PI), 2)
                            + Math.Pow(radiusP * Math.Sin(lat1 / 180 * Math.PI), 2);
            double radius = Math.Sqrt(nr / dr);

            //Calaculate distance in metres.
            distance = radius * c;
            return distance;
        }

    }

    public class MKSLtransfer : PartModule, IConfigNode
    {
        public string transferName = "";
        public List<MKSLresource> transferList = new List<MKSLresource>();
        public List<MKSLresource> costList = new List<MKSLresource>();
        public double arrivaltime = 0;
        public bool delivered = false;

        public Vessel vesselFrom = new Vessel();
        public Vessel vesselTo = new Vessel();
        public bool orbit = false;
        public double SMA = 0;
        public double ECC = 0;
        public double INC = 0;

        public bool surface = false;
        public double LAT = 0;
        public double LON = 0;


        public double totalMass()
        {
            double mass = 0;

            foreach (MKSLresource res in transferList)
            {
                mass = mass + res.mass();
            }

            return mass;
        }


        public void initTransferList(string resourceString)
        {
            transferList.Clear();

            string[] SplitArray = resourceString.Split(',');

            foreach (String resName in SplitArray)
            {
                MKSLresource res = new MKSLresource();
                res.resourceName = resName.Trim();
                transferList.Add(res);
            }

        }

        public void initCostList(string costString)
        {
            costList.Clear();

            string[] SplitArray = costString.Split(',');

            foreach (String resource in SplitArray)
            {
                string[] component = resource.Split(':');
                
                MKSLresource res = new MKSLresource();
                res.resourceName = component[0].Trim();
                res.costPerMass = Convert.ToDouble(component[1].Trim());
                costList.Add(res);
            }
        }

        public string longsavestring()
        {
            string save = "";

            save = vesselTo.id.ToString() + ">" + vesselTo.vesselName.Trim() + ">" + arrivaltime.ToString() + ">";
            save = save + savestring();

            return (save);
        }

        public string savestring()
        {
            string save = "";
            
            save = "transferName=" + transferName;
            save = save + "%" + "transferList=" + saveliststring(transferList);
            //print("1");
            save = save + "%" + "costList=" + saveliststring(costList);
            //print("2");
            save = save + "%" + "arrivaltime=" + arrivaltime.ToString();
            //print("3");
            save = save + "%" + "delivered=" + delivered.ToString();
            //print("4");
            save = save + "%" + "vesselFrom=" + vesselFrom.vesselName.Trim() +":" + vesselFrom.id.ToString();

            //print("5");
            save = save + "%" + "vesselTo=" + vesselTo.vesselName.Trim() +":" + vesselTo.id.ToString();
            //print("6");
            save = save + "%" + "orbit=" + orbit.ToString();
            //print("7");
            save = save + "%" + "SMA=" + SMA.ToString();
            //print("8");
            save = save + "%" + "ECC=" + ECC.ToString();
            //print("9");
            save = save + "%" + "INC=" + INC.ToString();
            //print("10");
            save = save + "%" + "surface=" + orbit.ToString();
            //print("11");
            save = save + "%" + "LAT=" + LAT.ToString();
            //print("12");
            save = save + "%" + "LON=" + LON.ToString();
            //print("13");            
            return (save);
        }

        private string saveliststring(List<MKSLresource> resourceList)
        {
            string save = "";
            
            foreach (MKSLresource res in resourceList)
            {
                if (save == "")
                {
                    save = res.savestring();
                }
                else
                {
                    save = save + "," + res.savestring();
                }
            }
            return (save);
        }

        public void loadstring(string load)
        {

            string[] SplitArray = load.Split('%');
            
            foreach (String str in SplitArray)
            {
                string[] Line = str.Split('=');

                switch (Line[0]) {
                    case "transferName":
                        transferName = Line[1];                
                        break;
                    case "transferList":
                        loadlist(ref transferList,Line[1]);
                        break;
                    case "costList":
                        loadlist(ref costList,Line[1]);
                        break;
                    case "arrivaltime":
                        arrivaltime = Convert.ToDouble(Line[1]);
                        break;
                    case "delivered":
                        delivered = (Line[1] == "True");  
                        break;
                    case "vesselFrom":
                        vesselFrom = loadvessel(Line[1]);
                        break;
                    case "vesselTo":
                        vesselTo = loadvessel(Line[1]);
                        break;
                    case "orbit":
                        orbit = (Line[1] == "True");
                        break;
                    case "SMA":
                        SMA = Convert.ToDouble(Line[1]);
                        break;
                    case "ECC":
                        ECC = Convert.ToDouble(Line[1]);
                        break;
                    case "INC":
                        INC = Convert.ToDouble(Line[1]);
                        break;
                    case "surface":
                        surface = (Line[1] == "True");
                        break;
                    case "LAT":
                        LAT = Convert.ToDouble(Line[1]);
                        break;
                    case "LON":
                        LON = Convert.ToDouble(Line[1]);
                        break;
                }
            }
        }

        private void loadlist(ref List<MKSLresource> resourceList, string load)
        {
            string[] SplitArray = load.Split(',');

            foreach (String st in SplitArray)
            {
                MKSLresource res = new MKSLresource();
                res.loadstring(st);
                resourceList.Add(res);
            }
        }

        private Vessel loadvessel(string load)
        {
            var retves = new Vessel();
            var splitArray = load.Split(':');
            retves.vesselName = splitArray[0];
            retves.id = new Guid(splitArray[1]);
            return (retves);
        }

        void IConfigNode.Load(ConfigNode node)
        {
            loadstring(node.GetValue("key"));
        }

        void IConfigNode.Save(ConfigNode node)
        {
            node.AddValue("key",longsavestring());
        }
    }


    public class MKSLTranferList : List<MKSLtransfer>, IConfigNode
    {
        public void Load(ConfigNode node)
        {
            Clear();
            var bbla = new KSPLog();
            bbla.LogWarning("[MKS] loading node");
            bbla.LogWarning("[MKS] loading node "+node.GetValue("key"));
            string savestring = node.GetValue("key");
            if (savestring == "") { return; }
            string[] deliveries = savestring.Split('@');
            bbla.LogWarning("[MKS] loading node length" + deliveries.Length);
                for (int i = deliveries.Length - 1; i >= 0; i--)
                {
                    string[] geninfo = deliveries[i].Split('>');
                    if (null != geninfo[3]) { deliveries[i] = geninfo[3]; }
                }

            for (int i = deliveries.Length - 1; i >= 0; i--)
            {
                var trans = new MKSLtransfer();
                trans.loadstring(deliveries[i]);
                Add(trans);
            }
        }

        public void Save(ConfigNode node)
        {
            node.AddValue("key", this.Aggregate("", (current, transfer) => current + (transfer.longsavestring() + "@")).TrimEnd('@'));
            
        }
    }
    public class MKSLresource
    {
        public string resourceName = "";
        public double amount = 0;

        public double costPerMass = 0;

        public double mass()
        {
            PartResourceDefinition prd = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return (amount * prd.density);
        }

        public string savestring()
        {
            return (resourceName + ":" + amount.ToString());
        }

        public void loadstring(string load)
        {
            string[] SplitArray = load.Split(':');
            resourceName = SplitArray[0];
            amount = Convert.ToDouble(SplitArray[1]);
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class MKSLlocal : MonoBehaviour
    {
        KSPLog _log = new KSPLog();
        double nextchecktime = 0;

        private void Awake()
        {
            RenderingManager.AddToPostDrawQueue(144, Ondraw);
            nextchecktime = Planetarium.GetUniversalTime() + 2;
        }

        private void Ondraw()
        {
            if (!(nextchecktime < Planetarium.GetUniversalTime())) return;
            _log.Log("[MKS] onDraw");
            foreach (Vessel ves in FlightGlobals.Vessels)
            {
                if (FlightGlobals.ActiveVessel.protoVessel.orbitSnapShot.ReferenceBodyIndex !=
                    ves.protoVessel.orbitSnapShot.ReferenceBodyIndex) continue;
                if (ves.packed && !ves.loaded)
                {
                    foreach (ProtoPartSnapshot p in ves.protoVessel.protoPartSnapshots)
                    {
                        foreach (ProtoPartModuleSnapshot pm in p.modules)
                        {
                            if (pm.moduleName != "MKSLcentral") continue;

                            var savestring = new MKSLTranferList();
                            savestring.Load(pm.moduleValues.GetNode("saveCurrentTransfersList"));
                            var completeddeliveries = new MKSLTranferList();
                            _log.Log("[MKS] onDraw with an unloaded vessel");
                            if (checkDeliveries(ref savestring, ref completeddeliveries))
                            {
                                var currentNode = new ConfigNode();
                                savestring.Save(currentNode);
                                pm.moduleValues.SetNode("saveCurrentTransfersList", currentNode);

                                var previouseList = pm.moduleValues.GetNode("savePreviousTransfersList");
                                var previouse = new MKSLTranferList();
                                previouse.Load(previouseList);
                                previouse.AddRange(completeddeliveries);
                                var previousNode = new ConfigNode();
                                previouse.Save(previousNode);
                                pm.moduleValues.SetNode("savePreviousTransfersList", previousNode);
                                
                            }
                        }
                    }
                }
                else
                {
                    foreach (Part p in ves.parts)
                    {
                        foreach (PartModule pm in p.Modules)
                        {
                            if (pm.moduleName == "MKSLcentral")
                            {
                                MKSLcentral MKSLc = p.Modules.OfType<MKSLcentral>().FirstOrDefault();

                                var savestring = MKSLc.saveCurrentTransfersList;
                                var completeddeliveries = new MKSLTranferList();
                                _log.Log("[MKS] onDraw with a loaded vessel");
                                if (checkDeliveries(ref savestring,ref completeddeliveries))
                                {
                                    MKSLc.saveCurrentTransfersList = savestring;
                                    MKSLc.savePreviousTransfersList.AddRange(completeddeliveries);
                                }
                            }
                        }
                    }
                }
            }
            nextchecktime = Planetarium.GetUniversalTime() + 60;
        }

        public bool checkDeliveries(ref MKSLTranferList savestring,ref MKSLTranferList completeddeliveries)
        {
            if (!savestring.Any())
            {
                return false;
            }

            bool action = false;
            var newsavestring = new MKSLTranferList();

            completeddeliveries = new MKSLTranferList();

            foreach (var delivery in savestring)
            {
                if (FlightGlobals.ActiveVessel.id == delivery.vesselTo.id && Planetarium.GetUniversalTime() > Convert.ToDouble(delivery.arrivaltime))
                {
                    _log.Log("[MKS] delivering to " + FlightGlobals.ActiveVessel.id);
                    action = true;
                    //if return is true then add the returned
                    if (attemptDelivery(delivery))
                    {
                        completeddeliveries.Add(delivery);
                    }
                    else
                    {
                        newsavestring.Add(delivery);
                    }
                }
                else
                {
                    _log.Log("[MKS] ignoring delivery to " + FlightGlobals.ActiveVessel.id +" from "+delivery.vesselTo.id);
                    newsavestring.Add(delivery);
                }
            }

            if (action)
            {
                savestring = newsavestring;
            }

            return action;
        }


        private bool attemptDelivery(MKSLtransfer delivery)
        {

            //check if orbit changed of destination if destination is orbital
            if (delivery.orbit)
            {
                if (!checkStaticOrbit(FlightGlobals.ActiveVessel, delivery.SMA, delivery.ECC, delivery.INC))
                {
                    delivery.delivered = false;

                    return true;
                }
            }

            //check if location changed of destination if destination is surface
            if (delivery.orbit) //todo: bug?
            {
                if (!checkStaticLocation(FlightGlobals.ActiveVessel, delivery.LON, delivery.LAT))
                {
                    delivery.delivered = false;
                    return true;
                }
            }

            makeDelivery(delivery);

            //delivery = transfer.longsavestring();
            //return false;

            return (true);
        }

        private void makeDelivery(MKSLtransfer transfer)
        {
            foreach (MKSLresource res in transfer.transferList)
            {
                giveResources(FlightGlobals.ActiveVessel, res.resourceName, res.amount);
            }
            transfer.delivered = true;
        }

        public void giveResources(Vessel deliverVessel, string ResourceName, double deliverAmount)
        {
            print(deliverVessel.name + " " + ResourceName + " " + deliverAmount);
            //deliver to parts
            foreach (Part op in deliverVessel.parts)
            {
                foreach (PartResource or in op.Resources)
                {
                    if (or.info.name == ResourceName && deliverAmount > 0)
                    {
                        if (deliverAmount >= or.maxAmount - or.amount)
                        {
                            deliverAmount = deliverAmount - (or.maxAmount - or.amount);
                            or.amount = or.maxAmount;
                        }
                        else
                        {
                            or.amount = or.amount + deliverAmount;
                            deliverAmount = 0;
                        }
                    }
                }
            }
        }


        private bool checkStaticOrbit(Vessel ves, double SMA, double ECC, double INC)
        {
            double MaxDeviationValue = 10;

            bool[] parameters = new bool[5];
            parameters[0] = false;
            parameters[1] = false;
            parameters[2] = false;

            //lenght
            if (MaxDeviationValue / 100 > Math.Abs(((ves.orbit.semiMajorAxis - SMA) / SMA))) { parameters[0] = true; }
            //ratio
            if (MaxDeviationValue / 100 > Math.Abs(ves.orbit.eccentricity - ECC)) { parameters[1] = true; }

            double angleD = MaxDeviationValue;
            //angle
            if (angleD > Math.Abs(ves.orbit.inclination - INC) || angleD > Math.Abs(Math.Abs(ves.orbit.inclination - INC) - 360)) { parameters[2] = true; }

            if (parameters[0] == false || parameters[1] == false || parameters[2] == false)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }

        private bool checkStaticLocation(Vessel ves, double LAT, double LON)
        {
            double distance = GetDistanceBetweenPoints(ves.latitude, ves.longitude, LAT, LON);

            return (true);
        }

        // adapted from: www.consultsarath.com/contents/articles/KB000012-distance-between-two-points-on-globe--calculation-using-cSharp.aspx
        public double GetDistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
        {
            double distance = 0;

            double dLat = (lat2 - lat1) / 180 * Math.PI;
            double dLong = (long2 - long1) / 180 * Math.PI;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                        + Math.Cos(lat2) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            //Calculate radius of planet
            // For this you can assume any of the two points.
            double radiusE = FlightGlobals.ActiveVessel.mainBody.Radius; // Equatorial radius, in metres
            double radiusP = FlightGlobals.ActiveVessel.mainBody.Radius; // Polar Radius

            //Numerator part of function
            double nr = Math.Pow(radiusE * radiusP * Math.Cos(lat1 / 180 * Math.PI), 2);
            //Denominator part of the function
            double dr = Math.Pow(radiusE * Math.Cos(lat1 / 180 * Math.PI), 2)
                            + Math.Pow(radiusP * Math.Sin(lat1 / 180 * Math.PI), 2);
            double radius = Math.Sqrt(nr / dr);

            //Calaculate distance in metres.
            distance = radius * c;
            return distance;
        }
    }
}





