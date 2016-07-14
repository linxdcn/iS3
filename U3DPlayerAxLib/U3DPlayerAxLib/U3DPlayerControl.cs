using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using AxUnityWebPlayerAXLib;

namespace U3DPlayerAxLib
{
    /// <summary>      
    /// U3d WebPlayer 控件
    /// </summary>        
    public partial class U3DPlayerControl : UserControl
    {
        public U3DPlayer axPlayer;

        public U3DPlayerControl()
        {
            InitializeComponent();
        }

        /*
        * 给unity设置src属性时，会自动生成字符串资源，并把它赋值给属性OcxState。
        * 由于没办法手动生成这个字符串，因而需要通过代码，即先赋值给OcxState，再取出来的方式得到需要的字符串资源。
        * 然后再将值赋给重新创建的控件。
        */
        public void LoadScence(string Src)
        {
            if (Src != "" && Src != null)//加载的路径地址不为空               
            {
                AxHost.State ocxState;
                if (axPlayer == null)
                {
                    //第一次加载用u3dPlayer的OcxState
                    ocxState = u3dPlayer.OcxState;
                    u3dPlayer.Dispose();
                }
                else
                {
                    //以后加载用axPlayer的OcxState 
                    ocxState = axPlayer.OcxState;
                    axPlayer.Dispose();
                }

                axPlayer = new U3DPlayer();
                axPlayer.Dock = DockStyle.Fill;
                axPlayer.BeginInit();
                axPlayer.OcxState = ocxState;
                base.Controls.Add(axPlayer);
                axPlayer.EndInit();

                axPlayer.src = Src;
                ocxState = axPlayer.OcxState;
                axPlayer.Dispose();//因为之后马上就要创建新的了

                axPlayer = new U3DPlayer();
                axPlayer.Dock = DockStyle.Fill;
                axPlayer.BeginInit();
                axPlayer.OcxState = ocxState;
                Controls.Add(axPlayer);
                axPlayer.EndInit();
            }
        }
    }
}
