using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Locktimer
{
    public partial class MainForm : Form
    {
        [DllImport("user32")]
        public static extern void LockWorkStation();

        bool m_closeForRealz = false;
        int m_lockTime = 15;
        int m_warnTime = 1;
        int m_elapsed = 0;

        public MainForm()
        {
            InitializeComponent();
            timer.Start();
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_closeForRealz)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Stop the lock timer for real?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                m_closeForRealz = true;
                Close();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            m_elapsed++;
            if (m_warnTime > 0 && m_elapsed == m_lockTime - m_warnTime)
                notifyIcon.ShowBalloonTip(2000, "Lock warning", $"Screen will lock in {m_warnTime} minute(s).", ToolTipIcon.Warning);
            if (m_elapsed == m_lockTime)
            {
                m_elapsed = 0;
                LockWorkStation();
            }
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            labelNextLock.Text = (m_lockTime - m_elapsed).ToString();
            labelNextWarning.Text = ((m_lockTime - m_warnTime) - m_elapsed).ToString();
            if (timer.Enabled)
                notifyIcon.Text = $"LockScreen Timer - Next lock in {m_lockTime - m_elapsed} minutes.";
            else
                notifyIcon.Text = $"LockScreen Timer - Disabled";
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            m_lockTime = Convert.ToInt32(upDownLock.Value);
            m_warnTime = Convert.ToInt32(upDownWarn.Value);
            timer.Stop();
            timer.Start();
            m_elapsed = 0;
            buttonDisable.Text = "Disable";
            UpdateLabels();
        }

        private void ButtonDisable_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                timer.Stop();
                m_elapsed = 0;
                buttonDisable.Text = "Enable";
                
            }
            else
            {
                timer.Start();
                m_elapsed = 0;
                buttonDisable.Text = "Disable";
            }
            UpdateLabels();
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CheckBoxRunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string shortcutPath = GetShortcutPath();
                if (checkBoxRunAtStartup.Checked)
                {
                    if (System.IO.File.Exists(shortcutPath))
                        return;

                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

                    shortcut.IconLocation = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "lock.ico");
                    shortcut.TargetPath = Application.ExecutablePath;
                    shortcut.Save();
                }
                else
                {
                    if (System.IO.File.Exists(shortcutPath))
                        System.IO.File.Delete(shortcutPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message, MessageBoxButtons.OK);
            }
        }

        private string GetShortcutPath()
        {
            string startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            return Path.Combine(startup, "LockScreen Timer.lnk");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string shortcutPath = GetShortcutPath();
            checkBoxRunAtStartup.Checked = System.IO.File.Exists(shortcutPath);
        }
    }
}
