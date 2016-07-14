using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace U3DPlayerAxLib
{
    /// <summary>      
    /// 封装U3d WebPlayer控件，屏蔽ocx中的鼠标右键显示菜单功能       
    /// </summary>        

    public class U3DPlayer : AxUnityWebPlayerAXLib.AxUnityWebPlayer
    {
        #region 常量定义，鼠标信息
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_RBUTTONBLCLK = 0x206;
        #endregion
        /// <summary>      
        /// 
        /// 屏蔽鼠标右键消息，解决鼠标右键下，会出现菜单的问题       
        /// 
        /// 
        /// </summary>        
        /// 
        /// <param name="msg"></param>       
        /// 
        /// <returns></returns>        
        public override bool PreProcessMessage(ref Message msg)
        {
            switch (msg.Msg)
            {
                case 0x204://鼠标右键按下消息                  
                    this.SendMessage("ThiredViewCamera", "RightMouseButtonDown", null);
                    this.SendMessage("FirstViewCamera", "RightMouseButtonDown", null);
                    this.SendMessage("Main Camera", "RightMouseButtonDown", null);
                    this.Focus();
                    return true;
                case 0x205://鼠标右键抬起消息                  
                    this.SendMessage("ThiredViewCamera", "RightMouseButtonUp", null);
                    this.SendMessage("FirstViewCamera", "RightMouseButtonUp", null);
                    this.SendMessage("Main Camera", "RightMouseButtonUp", null);
                    return true;
                case 0x206://鼠标右键点击消息                  
                    return true;
            }
            return base.PreProcessMessage(ref msg);
        }
    }    
}
