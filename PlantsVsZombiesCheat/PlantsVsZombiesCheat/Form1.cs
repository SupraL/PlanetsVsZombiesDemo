using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace PlantsVsZombiesCheat
{
    public partial class Form1 : Form
    {
        private IntPtr hWnd;
        private IntPtr windowhWnd;
        private String processName = "trgame";
        private int processBaseAddress;

        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    const uint MOUSEEVENTF_LEFTUP = 0x0004;
        [Flags]
        public enum KeyEvenFlags: int
        {
            WM_MOUSEMOVE = 0x200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x101
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("user32", EntryPoint = "PostMessage")]
        public static extern IntPtr PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, UIntPtr lParam);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
        public static extern int ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, out int lpBuffer, int nSize, int lpNumberOfBytesWritten);

        [DllImport("kernel32", EntryPoint = "WriteProcessMemory")]
        public static extern int WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);

        public Form1()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void detectGameTimer_Tick(object sender, EventArgs e)
        {
            Process[] processArray = Process.GetProcessesByName(processName);
            if (processArray.Length > 0)
            {
                gameStatusLabel.Text = "遊戲運行中";
                if (hWnd == IntPtr.Zero)
                {
                    hWnd = OpenProcess(ProcessAccessFlags.All, false, processArray[0].Id);
                    windowhWnd = processArray[0].MainWindowHandle;
                    processBaseAddress = processArray[0].MainModule.BaseAddress.ToInt32();
                }
            } else
            {
                gameStatusLabel.Text = "遊戲未運行";
                hWnd = IntPtr.Zero;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            detectGameTimer.Enabled = true;
            readSunTimer.Enabled = true;
        }

        private int sunPointer()
        {
            int baseAddress = 0x0009d590;
            int offset1 = 0x768;
            int offset2 = 0x5560;

            if (hWnd != IntPtr.Zero)
            {
                int baseValue;
                ReadProcessMemory(hWnd, baseAddress, out baseValue, 4, 0);
                ReadProcessMemory(hWnd, baseValue + offset1, out baseValue, 4, 0);
                return baseValue + offset2;
            }
            else
            {
                return -1;
            }
        }

        private int getCurrentSunValue()
        {
            int address = sunPointer();
            if (address != -1) {
                int baseValue = 0;
                ReadProcessMemory(hWnd, address, out baseValue, 4, 0);
                return baseValue;
            } else
            {
                return -1;
            }
        }

        private void enableInfiniteSun()
        {
            int address = sunPointer();
            if (address != -1)
            {
                WriteProcessMemory(hWnd, address, BitConverter.GetBytes(999), 4, 0);
            }
        }

        private void enableRemovePlantsTimeInterval(Boolean status)
        {
            byte[] orginalBytes = new byte[] { 0x83, 0x47, 0x24, 0x01 };
            byte[] attackBytes = new byte[] { 0x83, 0x47, 0x24, 0x63 };

            if (status)
            {
                WriteProcessMemory(hWnd, processBaseAddress + 0x8728c, attackBytes, 4, 0);
            } else
            {
                WriteProcessMemory(hWnd, processBaseAddress + 0x8728c, orginalBytes, 4, 0);
            }
        }

        private void readSunTimer_Tick(object sender, EventArgs e)
        {
            sunLabel.Text = getCurrentSunValue().ToString();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            infiniteSunTimer.Enabled = checkBox1.Checked;
        }

        private void infiniteSunTimer_Tick(object sender, EventArgs e)
        {
            enableInfiniteSun();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            enableRemovePlantsTimeInterval(checkBox2.Checked);
        }

        private static int MakePositionParam(int x, int y)
        {
            return (int)((x << 16) | (y & 0xFFFF));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int x = 551;
            int y = 164;
            int lParam = MakePositionParam(x, y);
            PostMessage(windowhWnd, (int)KeyEvenFlags.WM_LBUTTONDOWN, (IntPtr)0, (UIntPtr)lParam);
            Thread.Sleep(100);
            PostMessage(windowhWnd, (int)KeyEvenFlags.WM_LBUTTONUP, (IntPtr)0, (UIntPtr)lParam);
        }
    }
}
