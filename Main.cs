using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NppPluginNET;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace AutoIDs
{
    class Main
    {
        #region " Fields "
        internal const string PluginName = "AutoIDs";
        static string iniFilePath = null;

        static frmMyDlg frmMyDlg = null;
        static int idMyDlg = -1;
        static Bitmap tbBmp = Properties.Resources.star;
        static Bitmap tbBmp_tbTab = Properties.Resources.star_bmp;
        static Icon tbIcon = null;

        static bool _ShowConfig = false;
        static int _CurrentID;
        static string _Mask = @"sca_17296_";

        #endregion



        #region " StartUp/CleanUp "
        internal static void CommandMenuInit()
        {
            _CurrentID = -1;
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");

            var sb = new StringBuilder(128);
            var res = Win32.GetPrivateProfileString("AutoIds", "Mask", null, sb, (uint)sb.Capacity, iniFilePath);

                _Mask = sb.ToString();
            
            
            PluginBase.SetCommand(0, "Add ID", AddId, new ShortcutKey(false, true, false, Keys.I));
            PluginBase.SetCommand(1, "Validate ID's", ValidateIds, new ShortcutKey(false, true, false, Keys.V));
            PluginBase.SetCommand(2, "---", null);
            PluginBase.SetCommand(3, "Configure", myDockableDialog);
            idMyDlg = 1;


            frmMyDlg = new frmMyDlg();
            frmMyDlg.Mask = _Mask;
        }


        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }


        internal static void PluginCleanUp()
        {
            
        }

        #endregion

        #region " Menu functions "


        internal static void SaveConfigs(string mask)
        {
            _Mask = mask;
            Win32.WritePrivateProfileString("AutoIds", "Mask", _Mask, iniFilePath);
        }

        internal static void ValidateIds()
        {
            var hCurrentEditView = PluginBase.GetCurrentScintilla();
            int length = (int)Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETLENGTH, 0, 0) + 1;
            StringBuilder sb = new StringBuilder(length);
            Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETTEXT, length, sb);

            var text = sb.ToString();

            var mask = _Mask + @"(?'N'[\d]*)";

            var regex = new Regex(mask);

            var matches = regex.Matches(text);
            List<string> values = new List<string>();

            string result = "";

            foreach (Match m in matches)
            {
                var v = m.Value;
                var n = m.Groups["N"].Value;

                if (values.Contains(n))
                {
                    result += string.Format("Duplicate Id {0}, line:{1} ", n, m.Index);
                }
                else
                {
                    values.Add(n);
                }
            }

            if (result != "")
            {
                MessageBox.Show(result, "Auto Ids - Validations");
            }
            else
            {
                MessageBox.Show("All OK!", "Auto Ids - Validations");
            }

        }

        internal static void UpdateForm()
        {
            if (frmMyDlg != null)
                frmMyDlg.CurrentID =  (_CurrentID + "").PadLeft(4, '0');
        }


        internal static void GetCurrentId()
        {
            var hCurrentEditView = PluginBase.GetCurrentScintilla();
            int length = (int)Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETLENGTH, 0, 0) + 1;
            StringBuilder sb = new StringBuilder(length);
            Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETTEXT, length, sb);

            var text = sb.ToString();

            var mask = _Mask + @"(?'N'[\d]*)";

            var regex = new Regex(mask);

            var matches = regex.Matches(text);
            _CurrentID = 0;

            foreach (Match m in matches)
            {
                var v = m.Value;
                var n = m.Groups["N"].Value;

                var intV = -1;

                if (int.TryParse(n, out intV))
                {

                    if (_CurrentID < intV)
                        _CurrentID = intV;
                }
            }
        }


        internal static void AddId()
        {
            if (_CurrentID == -1)
            {
                GetCurrentId();
            }


            if (_CurrentID > -1)
            {
                _CurrentID += 1;
                var hCurrentEditView = PluginBase.GetCurrentScintilla();
                var newValue = _Mask + (_CurrentID + "").PadLeft(4, '0');
                
                UpdateForm();

                Win32.SendMessage(hCurrentEditView, SciMsg.SCI_BEGINUNDOACTION, 0, 0);
                Win32.SendMessage(hCurrentEditView, SciMsg.SCI_REPLACESEL, 0, newValue);
                Win32.SendMessage(hCurrentEditView, SciMsg.SCI_ENDUNDOACTION, 0, 0);
            }
        }


        internal static void myDockableDialog()
        {
            if (_ShowConfig == false)
            {
                _ShowConfig = true;
                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmMyDlg.Handle;
                _nppTbData.pszName = "Auto IDs Configuration";
                _nppTbData.dlgID = idMyDlg;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, frmMyDlg.Handle);
            }
        }
        #endregion
    }
}