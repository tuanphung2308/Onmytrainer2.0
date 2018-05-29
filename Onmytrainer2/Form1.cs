using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitmapDetector2;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using SharpAdbClient;

namespace Onmytrainer2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitNox();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        List<DeviceData> devices = AdbClient.Instance.GetDevices();
        ConsoleOutputReceiver reciever = new ConsoleOutputReceiver();

        public int DEVICE_WIDTH = 1920;
        public int DEVICE_HEIGHT = 1080;

        Image<Bgr, byte> stage20 = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/stage20.png");
        Image<Bgr, byte> closeLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/close.png");
        Image<Bgr, byte> inviteLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/invite.png");
        Image<Bgr, byte> partyLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/party.png");
        Image<Bgr, byte> darlingLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/darling.png");
        Image<Bgr, byte> selectedLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/selected.png");
        Image<Bgr, byte> emoticonLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/emoticon.png");
        Image<Bgr, byte> treasureLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/treasure.png");
        Image<Bgr, byte> continueLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/continue.png");
        Image<Bgr, byte> backLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/back.png");
        Image<Bgr, byte> bossLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/boss.png");
        Image<Bgr, byte> mobLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/mob.png");
        Image<Bgr, byte> endofmapLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/endofmap.png");
        Image<Bgr, byte> receiveLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/lead/receive.png");

        Image<Bgr, byte> inviteMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/invite_main.png");
        Image<Bgr, byte> avatarMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/avatar_main.png");
        Image<Bgr, byte> backMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/back_main.png");
        Image<Bgr, byte> leaveMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/leave_main.png");
        Image<Bgr, byte> okMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/ok_main.png");
        Image<Bgr, byte> closeMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/close_main.png");
        Image<Bgr, byte> treasureMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/treasure_main.png");
        Image<Bgr, byte> teamMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/team_main.png");
        Image<Bgr, byte> maxMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/maxlevel_main.png");
        Image<Bgr, byte> receiveMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/receive_main.png");
        Image<Bgr, byte> newSquadMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/main/newsquad.png");

        Image<Bgr, byte> acceptMain = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/acceptmain.png");
        Image<Bgr, byte> acceptLead = new Image<Bgr, byte>(Directory.GetCurrentDirectory() + "/acceptlead.png");

        public static int STAGE_LOOKING_FOR_INVITE = 0;
        public static int STAGE_IN_PARTY = 1;
        public static int STAGE_IN_BATTLE = 2;
        public static int STAGE_COLLECT_DROP = 3;

        public static string LEECH_NOX_MODEL = "SM_N950F";
        public static string LEAD_NOX_MODEL = "SM_N9005";

        public Boolean stop = true;

        public class Nox
        {
            public Rectangle rectNox;
            public string name;

            public int currentStage;
            public string model;
            public DeviceData device;

            public int getCurrentStage()
            {
                return currentStage;
            }

            public void setCurrentStage(int stage)
            {
                currentStage = stage;
            }

            public Nox()
            {
            }
        }

        public Nox leadNox = new Nox();
        public Nox leechNox = new Nox();

        public void InitNox()
        {
            leadNox.rectNox = NoxLocation("lead");
            leadNox.name = "lead";
            MessageBox.Show($"Lead Nox: { leadNox.rectNox.Width}  { leadNox.rectNox.Height}");
            leadNox.device = getDeviceByModel(LEAD_NOX_MODEL);
            setStage(STAGE_LOOKING_FOR_INVITE, leadNox);

            leechNox.rectNox = NoxLocation("leech");
            leechNox.name = "leech";
            MessageBox.Show($"Leech Nox: { leechNox.rectNox.Width}  { leechNox.rectNox.Height}");
            leechNox.device = getDeviceByModel(LEECH_NOX_MODEL);
            setStage(STAGE_LOOKING_FOR_INVITE, leechNox);
        }

        public DeviceData getDeviceByModel(string model)
        {
            foreach (DeviceData d in devices)
            {
                if (d.Model.Equals(model))
                {
                    return d;
                }
            }
            return null;
        }

        public void setStage(int stage, Nox n)
        {
            n.setCurrentStage(stage);
        }

        public Boolean isInStage(int stage, Nox n)
        {
            return n.getCurrentStage() == stage;
        }

        private void touch(Tuple<int, int> coor, DeviceData device)
        {
            string inputCommand = "input tap " + coor.Item1 + " " + coor.Item2;
            sendInputCommand(inputCommand, device);
        }

        private void swipe(int x1, int y1, int x2, int y2, int t, DeviceData device)
        {
            string inputCommand = $"input swipe {x1} {y1} {x2} {y2} {t}";
            sendInputCommand(inputCommand, device);
        }

        private void sendInputCommand(string inputCommand, DeviceData device)
        {
            AdbClient.Instance.ExecuteRemoteCommand(inputCommand, device, reciever);
        }

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public static Rectangle NoxLocation(string title)
        {
            Process lol;
            Process[] processes = Process.GetProcessesByName("Nox");
            foreach (Process p in processes)
            {
                if (p.MainWindowTitle.Equals(title))
                {
                    lol = p;
                    IntPtr ptr = lol.MainWindowHandle;
                    Rect NoxRect = new Rect();
                    GetWindowRect(ptr, ref NoxRect);
                    int hTitleBar = 29;
                    return new Rectangle(NoxRect.Left, NoxRect.Top + hTitleBar, NoxRect.Right - NoxRect.Left, NoxRect.Bottom - NoxRect.Top - hTitleBar);
                }
            }
            return new Rectangle(0, 0, 0, 0);
        }

        public Tuple<int, int> findImage(Nox nox, Image<Bgr, byte> small)
        {
            //Take screenshot of that device.
            Rectangle rect = nox.rectNox;
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            bmp.Save(nox.name + ".png", ImageFormat.Png);

            Image<Bgr, byte> source = new Image<Bgr, byte>(bmp); // Screen cap
            Image<Bgr, byte> imageToShow = source.Copy();

            using (Image<Gray, float> result = source.MatchTemplate(small, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                List<Rectangle> rectangles = new List<Rectangle>();
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > 0.9)
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    Rectangle match = new Rectangle(maxLocations[0], small.Size);
                    imageToShow.Draw(match, new Bgr(Color.Red), 3);
                    if (nox.name.Equals("lead"))
                    {
                        setImage1(imageToShow);
                    }
                    else
                    {
                        setImage2(imageToShow);
                    }

                    //Normalized coordinates
                    int X = maxLocations[0].X * DEVICE_WIDTH / nox.rectNox.Width;
                    int Y = maxLocations[0].Y * DEVICE_HEIGHT / nox.rectNox.Height;
                    return new Tuple<int, int>(X, Y);
                }
                else
                {
                    return null;
                }

                // Show imageToShow in an ImageBox (here assumed to be called imageBox1)

            }
        }

        private Boolean isImageFound(Nox n, Image<Bgr, byte> b)
        {
            Tuple<int, int> result = findImage(n, b);
            return result != null;
        }

        private void findAndTouch(Nox n, Image<Bgr, byte> b)
        {
            Tuple<int, int> result = findImage(n, b);
            if (result != null)
            {
                touch(result, n.device);
            }
        }

        private void handleInviteLeecher()
        {
            if (isImageFound(leadNox, closeLead))
            {
                if (!isImageFound(leadNox, inviteLead))
                {
                    if (!isImageFound(leadNox, partyLead))
                    {
                        touch(new Tuple<int, int>(730, 335), leadNox.device); //Hard button
                    }
                    else
                    {
                        touch(new Tuple<int, int>(985, 807), leadNox.device);//Party button
                    }
                }
                else
                {
                    if (isImageFound(leadNox, darlingLead))
                    {
                        findAndTouch(leadNox, darlingLead);
                    }
                    else
                    {
                        touch(new Tuple<int, int>(800, 200), leadNox.device); //friend tab
                    }
                }

                if (isImageFound(leadNox, selectedLead))
                {
                    touch(new Tuple<int, int>(1170, 858), leadNox.device);  // invite
                }
            }
            else
            {
                if (isImageFound(leadNox, emoticonLead))
                {
                    //MessageBox.Show("joined party");
                    setStage(STAGE_IN_PARTY, leadNox);
                }
                else
                {
                    findAndTouch(leadNox, stage20);
                }
            }
        }

        private void findAndBattleMob()
        {
            //MessageBox.Show("Finding mob");
            if (isImageFound(leadNox, treasureLead))
            {
                setStage(STAGE_COLLECT_DROP, leadNox);
                return;
            }
            if (isImageFound(leadNox, continueLead))
            {
                touch(new Tuple<int, int>(1100, 640), leadNox.device);  // Continue Btn
                setStage(STAGE_LOOKING_FOR_INVITE, leadNox);
                return;
            }

            if (isImageFound(leadNox, backLead))
            {
                if (isImageFound(leadNox, bossLead))
                {
                    Console.WriteLine("found boss");
                    findAndTouch(leadNox, bossLead);
                    Thread.Sleep(300);
                }
                else if (isImageFound(leadNox, mobLead))
                {
                    Console.WriteLine("found mob");
                    findAndTouch(leadNox, mobLead);
                    Thread.Sleep(300);
                }
                else
                {
                    if (isImageFound(leadNox, endofmapLead))
                    {
                        swipe(100, 1070, 1800, 1070, 1000, leadNox.device);
                        Thread.Sleep(1200); //sleep waiting for scroll
                    }
                    else
                    {
                        swipe(1800, 1070, 1400, 1070, 200, leadNox.device);
                        Thread.Sleep(300); //sleep waiting for scroll
                    }
                }
            }
            else
            {
                setStage(STAGE_IN_BATTLE, leadNox);
            }
        }

        private void handleBattleLead()
        {
            while (true && !stop)
            {
                if (isImageFound(leadNox, backLead))
                {
                    setStage(STAGE_IN_PARTY, leadNox);
                    break;
                }
                else
                {
                    //Console.WriteLine("targetting mob");
                    touch(new Tuple<int, int>(1423, 455), leadNox.device);  //target mob
                }
            }
        }

        private void handleTreasureLead()
        {
            Console.WriteLine("Collecting treasure");
            findAndTouch(leadNox, treasureLead);
            if (isImageFound(leadNox, receiveLead))
            {
                touch(new Tuple<int, int>(1870, 1043), leadNox.device);
            }
            if (isImageFound(leadNox, continueLead) || isImageFound(leadNox, stage20))
            {
                touch(new Tuple<int, int>(1100, 640), leadNox.device);  //Continue Btn
                setStage(STAGE_LOOKING_FOR_INVITE, leadNox);
            }
        }

        /* 
         * 
         * 
         * 
         * 
         * FROM THIS IS LEECHER SECTION
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * */

        private void handleInvitation()
        {
            Console.WriteLine("Leecher: Looking for invitation");
            findAndTouch(leechNox, inviteMain);

            if (isImageFound(leechNox, avatarMain))
            {
                setStage(STAGE_IN_PARTY, leechNox);
                Console.WriteLine("Leecher: Successfully joined party");
            }
        }

        private void handlePartyStatus()
        {
            if (!isImageFound(leechNox, backMain))
            {
                Console.WriteLine("Leecher: Battle engaged");
                setStage(STAGE_IN_BATTLE, leechNox);
                return;
            }

            if (isImageFound(leechNox, leaveMain))
            {
                Console.WriteLine("Leecher: Left party. Proceed to leave stage");
                touch(new Tuple<int, int>(86, 100), leechNox.device);  //Back button position
                while (true && !stop)
                {
                    if (isImageFound(leechNox, okMain)) break;
                }
                touch(new Tuple<int, int>(1149, 611), leechNox.device);  //OK Button

                while (true && !stop)
                {
                    if (isImageFound(leechNox, closeMain)) break;
                }
                touch(new Tuple<int, int>(1600, 210), leechNox.device);  // Close Btn
                setStage(STAGE_LOOKING_FOR_INVITE, leechNox);
            }
            else
            {
                Console.WriteLine("Leecher: Current in party. Waiting for battle and drop");
            }

            if (isImageFound(leechNox, treasureMain)) setStage(STAGE_COLLECT_DROP, leechNox);
            if (isImageFound(leechNox, closeMain))
            {
                touch(new Tuple<int, int>(1600, 210), leechNox.device);  // Close Btn
                setStage(STAGE_LOOKING_FOR_INVITE, leechNox);
            }
        }

        private void handleBattleOnMain()
        {
            while (true && !stop)
            {
                touch(new Tuple<int, int>(1423, 455), leechNox.device);  //target mob
                if (isImageFound(leechNox, teamMain))
                {
                    if (isImageFound(leechNox, maxMain))
                    {
                        Console.WriteLine("Leecher: Fodder reached max");
                        //increSquad() should be find and click hokigami 1
                        while (true && !stop)
                        {
                            if (isImageFound(leechNox, teamMain)) break;
                        }
                        touch(new Tuple<int, int>(96, 1025), leechNox.device);  //prebuit team button
                        Thread.Sleep(1000);
                        if (!isImageFound(leechNox, newSquadMain))
                        {
                            swipe(326, 929, 326, 575, 200, leechNox.device); //find new team
                            Thread.Sleep(300);
                        }

                        if (!isImageFound(leechNox, newSquadMain)) { MessageBox.Show("No empty squad"); }
                        else
                        {
                            findAndTouch(leechNox, newSquadMain);
                        }
                        //adbTap(330, 446 + 200 * (currentSquad - 1))  # team index
                        //time.sleep(1)
                        touch(new Tuple<int, int>(340, 1003), leechNox.device);  //go
                    }
                    touch(new Tuple<int, int>(1783, 769), leechNox.device);  //ready
                }
                else
                {
                    //Console.WriteLine("Leecher: couldnt find team png");
                    touch(new Tuple<int, int>(1423, 455), leechNox.device);  //target mob
                }
                if (isImageFound(leechNox, backMain))
                {
                    setStage(STAGE_IN_PARTY, leechNox);
                    break;
                }
                touch(new Tuple<int, int>(1423, 455), leechNox.device); //target mob
            }
        }

        private void handleTreasureMain()
        {
            Console.WriteLine("Collecting treasure");
            findAndTouch(leechNox, treasureMain);
            if (isImageFound(leechNox, receiveMain))
            {
                touch(new Tuple<int, int>(1870, 1043), leechNox.device);
            }
            if (isImageFound(leechNox, closeMain))
            {
                touch(new Tuple<int, int>(1600, 210), leechNox.device);  //Close Btn
                setStage(STAGE_LOOKING_FOR_INVITE, leechNox);
            }
        }

        delegate void SetImgCallBack(Image<Bgr, byte> img);
        private void setImage1(Image<Bgr, byte> img)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.imageBox1.InvokeRequired)
            {
                SetImgCallBack d = new SetImgCallBack(setImage1);
                this.Invoke(d, new object[] { img });
            }
            else
            {
                imageBox1.Image = img;
            }
        }

        private void setImage2(Image<Bgr, byte> img)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.imageBox2.InvokeRequired)
            {
                SetImgCallBack d = new SetImgCallBack(setImage2);
                this.Invoke(d, new object[] { img });
            }
            else
            {
                imageBox2.Image = img;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            stop = !stop;
            button1.Text = stop ? "Start" : "Stop";
            if (!stop)
            {
                new Thread(() =>
                {
                    while (!stop)
                    {
                        if (isInStage(STAGE_LOOKING_FOR_INVITE, leadNox))
                        {
                            handleInviteLeecher();
                        }
                        else if (isInStage(STAGE_IN_PARTY, leadNox))
                        {
                            findAndBattleMob();
                        }
                        else if (isInStage(STAGE_IN_BATTLE, leadNox))
                        {
                            handleBattleLead();
                        }
                        else if (isInStage(STAGE_COLLECT_DROP, leadNox))
                        {
                            handleTreasureLead();
                        }
                    }
                }).Start();

                new Thread(() =>
                {
                    while (!stop)
                    {
                        if (isInStage(STAGE_LOOKING_FOR_INVITE, leechNox))
                        {
                            handleInvitation();
                        }
                        else if (isInStage(STAGE_IN_PARTY, leechNox))
                        {
                            handlePartyStatus();
                        }
                        else if (isInStage(STAGE_IN_BATTLE, leechNox))
                        {
                            handleBattleOnMain();
                        }
                        else if (isInStage(STAGE_COLLECT_DROP, leechNox))
                        {
                            handleTreasureMain();
                        }
                    }
                }).Start();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Rectangle rect = leadNox.rectNox;
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            bmp.Save("lead.png", ImageFormat.Png);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Rectangle rect = leechNox.rectNox;
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            bmp.Save("main.png", ImageFormat.Png);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            touch(new Tuple<int, int>(96, 1025), leechNox.device);  //prebuit team button
            Thread.Sleep(1000);
            if (!isImageFound(leechNox, newSquadMain))
            {
                swipe(326, 929, 326, 575, 200, leechNox.device); //find new team
                Thread.Sleep(300);
            }

            if (!isImageFound(leechNox, newSquadMain)) { MessageBox.Show("No empty squad"); }
            else
            {
                findAndTouch(leechNox, newSquadMain);
            }
            //adbTap(330, 446 + 200 * (currentSquad - 1))  # team index
            //time.sleep(1)
            touch(new Tuple<int, int>(340, 1003), leechNox.device);  //go
        }

        private void button5_Click(object sender, EventArgs e)
        {
        }
    }
}
