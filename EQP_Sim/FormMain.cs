using EQP_Sim.Comm;
using EQP_Sim.UI_Update;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQP_Sim
{
    public partial class FormMain : Form, ICommMessage
    {
        SocketServer Comm;
        public FormMain()
        {
            InitializeComponent();
        }

        public void On_Connection_Connected(Socket handler)
        {
            FormMainUpdate.LogUpdate(handler.RemoteEndPoint.ToString() + " Connected");
        }

        public void On_Connection_Connecting()
        {
            FormMainUpdate.LogUpdate("Connecting");
        }

        public void On_Connection_Disconnected()
        {
            FormMainUpdate.LogUpdate("Disconnected");
        }

        public void On_Connection_Error(string Msg)
        {
            FormMainUpdate.LogUpdate("Error");
        }

        public void On_Connection_Message(Socket handler, string Msg)
        {


            string NodeAdr = "";
            string Type = "";
            string Command = "";
            string Value = "";
            string[] content = { };
            if (Msg.IndexOf("$") != -1)
            {
                //Sanwa
                NodeAdr = Msg[1].ToString();
                content = Msg.Replace("\r", "").Replace("\n", "").Substring(2).Split(':');
                FormMainUpdate.LogUpdate(handler.RemoteEndPoint.ToString() + " Rev:" + Msg);
            }
            else
            {
                //TDK
                byte[] t = new byte[Encoding.ASCII.GetByteCount(Msg.ToString())];
                int c = Encoding.ASCII.GetBytes(Msg.ToString(), 0, Encoding.ASCII.GetByteCount(Msg.ToString()), t, 0);
                NodeAdr = Encoding.Default.GetString(t, 3, 2);
                string contentStr = Encoding.Default.GetString(t, 5, t.Length - 5 - 3).Replace(";", "").Trim();
                content = contentStr.Split(':', '/');
                FormMainUpdate.LogUpdate(handler.RemoteEndPoint.ToString() + " Rev:" + contentStr);
            }
            for (int i = 0; i < content.Length; i++)
            {
                switch (i)
                {
                    case 0:

                        Type = content[i];
                        break;
                    case 1:

                        Command = content[i];

                        break;
                    case 2:
                        Value = content[i];

                        break;
                }
            }

            string ReturnMessage = "";
            if (Msg.IndexOf("$") != -1)
            {
                string ReturnValue = "";
                //Sanwa
                if (Type.Equals("GET"))
                {
                    switch (Command)
                    {
                        case "RIO__":
                            switch (Value)
                            {
                                case "004":
                                    ReturnValue = ":" + Value + ",0";
                                    break;
                                case "005":
                                    ReturnValue = ":" + Value + ",0";
                                    break;
                                case "008":
                                    ReturnValue = ":" + Value + ",0";
                                    break;
                                case "009":
                                    ReturnValue = ":" + Value + ",0";
                                    break;
                                default:
                                    ReturnValue = ":" + Value + ",0";
                                    break;
                            }
                            break;
                    }
                    
                }
                
                ReturnMessage = "$" + NodeAdr + "ACK:" + Command + ReturnValue + "\r";
                Comm.Send(handler, ReturnMessage);
                FormMainUpdate.LogUpdate(handler.RemoteEndPoint.ToString() + " Snd:" + ReturnMessage);

                if (Type.Equals("CMD"))
                {
                    ReturnMessage = "$" + NodeAdr + "FIN:" + Command + ":00000000" + "\r";
                    Comm.Send(handler, ReturnMessage);
                    FormMainUpdate.LogUpdate(handler.RemoteEndPoint.ToString() + " Snd:" + ReturnMessage);
                }
            }
            else
            {
                if (!Type.Equals("FIN"))
                {
                    string ReturnValue = "";
                    if (Type.Equals("GET"))
                    {
                        switch (Command)
                        {
                            case "STATE":
                                ReturnValue = "/00000000100110111000"; 
                                break;
                        }
                    }
                    ReturnMessage = TDK_A("ACK:" + Command + ReturnValue);
                    Comm.Send(handler, ReturnMessage);
                    FormMainUpdate.LogUpdate(handler.RemoteEndPoint.ToString() + " Snd:" + "ACK:" + Command);

                    //System.Threading.Thread.Sleep(100);

                    ReturnMessage = TDK_A("INF:" + Command + ReturnValue);
                    Comm.Send(handler, ReturnMessage);
                    FormMainUpdate.LogUpdate(handler.RemoteEndPoint.ToString() + " Snd:" + "INF:" + Command);
                }
                //TDK
                //ReturnMessage =  NodeAdr + "ACK:" + Command;
                //Comm.Send(handler, ReturnMessage);
                //ReturnMessage = "$" + NodeAdr + "FIN:" + Command + "00000000";
                //Comm.Send(handler, ReturnMessage);
            }
        }

        public string TDK_A(string Command)
        {
            string strCommsnd = string.Empty;
            string strLen = string.Empty;
            string sCheckSum = string.Empty;
            int chrLH = 0;
            int chrLL = 0;

            try
            {
                strLen = Convert.ToString(Command.Length + 4, 16).PadLeft(2, '0');

                chrLH = Convert.ToInt32(strLen.Substring(0, 1), 16);
                chrLL = Convert.ToInt32(strLen.Substring(1, 1), 16);
                strLen = Convert.ToChar(chrLH).ToString() + Convert.ToChar(chrLL).ToString();
                sCheckSum = ProcCheckSum(strLen, Command);
                strCommsnd = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", Convert.ToChar(1), strLen, Convert.ToChar(48), string.Empty, Convert.ToChar(48), Command, sCheckSum, Convert.ToChar(3));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return strCommsnd;
        }

        public string ProcCheckSum(string Len, string Message)
        {
            string strCheckSum = string.Empty;
            string csHex = string.Empty;

            try
            {
                strCheckSum = string.Format("{0}{1}{2}{3}{4}", Len, Convert.ToChar(48), string.Empty, Convert.ToChar(48), Message.ToString());

                byte[] t = new byte[Encoding.ASCII.GetByteCount(strCheckSum)]; ;
                int ttt = Encoding.ASCII.GetBytes(strCheckSum, 0, Encoding.ASCII.GetByteCount(strCheckSum), t, 0);
                byte tt = 0;

                for (int i = 0; i < t.Length; i++)
                {
                    tt += t[i];
                }

                csHex = tt.ToString("X");
                if (csHex.Length == 1)
                {
                    csHex = "0" + csHex;
                }

                return csHex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Comm = new SocketServer(this);
        }
    }
}
